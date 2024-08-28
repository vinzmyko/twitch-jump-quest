using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Godot;
using Vector2 = Godot.Vector2;
using UNLTeamJumpQuest.TwitchIntegration;

public partial class Player : CharacterBody2D
{
    [Signal]
    public delegate void DiedEventHandler(string displayName, string userID, string teamAbbrev);
    [Signal]
    public delegate void FaceplantedEventHandler(Player player, float fallDistance);
    [Signal]
    public delegate void ComboStreakingEventHandler(Player player, int comboStreak);
    [Signal]
    public delegate void ScoreUpdatedEventHandler(string teamAbbrev, int points);
	private AnimatedSprite2D animatedSprite;
    private Label displayLabel;

    [Export]
    private float distanceForHeadOnFloor = 225; // Base jump tiles is 8 or 128px
    private float BaseJumpVelocity = 500.0f;
    private float Gravity = 980.0f; 
    public bool headOnFloor = false;
    private float currentYPos = 0, previousYPos = 0, highestYPos = 0, jumpYPos = 0;
    private const float TILE_SIZE = 16f;
    private Vector2 lastScoredPosition;
    private int currentPlatformId = -1;
    private Vector2 _jumpVelocity = Vector2.Zero;

    public string userID, displayName;
    public UNL.Team team;
    private Color[] teamColours;
    private DebugTwitchChat debugger;
    private SettingsManager settingsManager;
    private LevelManager levelManager;
    public int points, combo = 0, comboStreak = 0, numOfFaceplants = 0, distanceOfFurthestFaceplant = 0, idxOfUniqueFeatherColour;
    private const int MAX_TRACKED_PLATFORMS = 10;
    private Queue<PlatformInfo> recentPlatforms = new Queue<PlatformInfo>();
    public float highestYPosition = float.MaxValue; // lower Y is higher in Godot
    public float startingYpos;
    private AudioStreamPlayer2D audioPlayer;
    private AudioStream jumpSFX;
    private AudioStream faceplantSFX;

    public struct PlatformInfo
    {
        public int PlatformId { get; }
        public float XPosition { get; }
        public float YPosition { get; }

        public PlatformInfo(int platformId, float x, float y)
        {
            PlatformId = platformId;
            XPosition = x;
            YPosition = y;
        }
    }
    private GameManager gameManager;

    public void Initialize(string _displayName, string _userID, UNL.Team _team)
    {
        displayName = _displayName;
        userID = _userID;
        points = 0;
        team = _team;
        SetColoursArray(_team);
    }

    public void SetupPlayerColors()
    {
        ShaderMaterial uniqueMaterial = (ShaderMaterial)animatedSprite.Material.Duplicate();
        SetTeamColours(teamColours, uniqueMaterial);
        animatedSprite.Material = uniqueMaterial;
    }

    public override async void _Ready()
    {
        base._Ready();

        debugger = GetNodeOrNull<DebugTwitchChat>("/root/Main/CanvasLayer/DebugTwitchChat");
        settingsManager = GetNodeOrNull<SettingsManager>("/root/SettingsManager");
        levelManager = GetNodeOrNull<LevelManager>("/root/LevelManager");
        gameManager = GetNodeOrNull<GameManager>("/root/GameManager");
        

        if (debugger != null)
        {
            debugger.DebuggerDeleteSelf += OnDeleteSelf;
        }

        audioPlayer = GetNodeOrNull<AudioStreamPlayer2D>("AudioStreamPlayer2D");
        jumpSFX = ResourceLoader.Load<AudioStream>("res://Audio/JumpSounds/JumpSFX.ogg");
        faceplantSFX = ResourceLoader.Load<AudioStream>("res://Audio/JumpSounds/Faceplant.ogg");
		animatedSprite = GetNode<AnimatedSprite2D>("AnimatedSprite2D");
        displayLabel = GetNode<Label>("DisplayNameLabel");
        displayLabel.Text = displayName;

        // ShaderMaterial uniqueMaterial = (ShaderMaterial)animatedSprite.Material.Duplicate();
        // SetTeamColours(teamColours, uniqueMaterial);
        // animatedSprite.Material = uniqueMaterial;

        TwitchBot.Instance.MessageReceived += OnMessageReceived;
        animatedSprite.AnimationFinished += OnHeadOnFloorAnimationFinished;

        if (userID == "DEBUG")
        {
            AddToGroup("DebugPlayer");
        }
        else
            AddToGroup("Player");

        // Some reason it won't change in Player scene so I do it through code.
        SetCollisionLayerValue(1, false);
        SetCollisionLayerValue(2, true);
        await showDisplayName(8.0f);
    }

    private void playJumpAudio(AudioStream audio)
    {
        audioPlayer.Stop();
        audioPlayer.Stream = audio;
        audioPlayer.Play();
    }

    private void CalculateScore()
    {
        if (gameManager.CurrentGameState != GameManager.GameState.Playing)
        {
            return;
        }
        
        // tileSetAtlasID is what image texture image the tile originates from
        var (newPlatformId, layerName, tileSetAtlasID) = GetCurrentPlatformId();
        Vector2 currentPosition = GlobalPosition;

        if (newPlatformId == -1 || layerName != "Midground")
        {
            return; // Not on a platform or not on Midground
        }

        bool isNewHighestPosition = currentPosition.Y < highestYPosition;
        bool isSameHeight = Math.Abs(currentPosition.Y - highestYPosition) < TILE_SIZE;
        bool isNewPlatform = !recentPlatforms.Any(p => 
            p.PlatformId == newPlatformId);

        // Calculate points
        float progressFraction = (levelManager.startMarkerYPos - currentPosition.Y) / levelManager.totalLevelYDistance;
        int pointsGained = Mathf.FloorToInt(progressFraction * 100);

        bool shouldAwardPointsAndIncreaseCombo = ((isNewHighestPosition || isSameHeight) && isNewPlatform) && pointsGained > 0;

        if (shouldAwardPointsAndIncreaseCombo)
        {
            combo++;
            float comboMultiplier = 1.0f;
            if (combo > comboStreak)
            {
                comboStreak = combo;
            }
            if (combo >= 7 && combo > comboStreak)
            {
                EmitSignal(SignalName.ComboStreaking, this, comboStreak);
                comboMultiplier = 1.0f + (combo / 100.0f);
            }

            int totalPointsGained = (int)(pointsGained * comboMultiplier); AddScore(totalPointsGained); // GD.Print($"Points given for Midground platform: {pointsGained}");
            recentPlatforms.Enqueue(new PlatformInfo(newPlatformId, currentPosition.X, currentPosition.Y));
            if (recentPlatforms.Count > MAX_TRACKED_PLATFORMS)
            {
                recentPlatforms.Dequeue();
            }

            if (isNewPlatform)
            {
                recentPlatforms.Enqueue(new PlatformInfo(newPlatformId, currentPosition.X, currentPosition.Y));
                if (recentPlatforms.Count > MAX_TRACKED_PLATFORMS)
                {
                    recentPlatforms.Dequeue();
                }
            }

            if (isNewHighestPosition)
            {
                highestYPosition = currentPosition.Y;
            }
            lastScoredPosition = currentPosition;
        }
        // shouldAwardPointsAndIncreaseCombo == false
        else
            combo = 0;
    }

    private (int platformId, string layerName, int tileSetAtlasId) GetCurrentPlatformId()
    {
        var spaceState = GetWorld2D().DirectSpaceState;
        uint collisionMask = 1; // Assuming your jumpable tiles are on collision layer 1

        Vector2[] directions = { Vector2.Down, new Vector2(-1, 1), new Vector2(1, 1) };
        float rayLength = 5f;

        foreach (var direction in directions)
        {
            var query = PhysicsRayQueryParameters2D.Create(
                GlobalPosition,
                GlobalPosition + direction * rayLength,
                collisionMask,
                new Godot.Collections.Array<Rid> { GetRid() } // Exclude the player's own collision
            );

            var result = spaceState.IntersectRay(query);

            if (result.Count > 0)
            {
                var collider = result["collider"].As<Node2D>();
                if (collider is TileMap tileMap)
                {
                    Vector2 collisionPoint = (Vector2)result["position"];
                    Vector2I cellCoords = tileMap.LocalToMap(tileMap.ToLocal(collisionPoint));
                    
                    string layerName = tileMap.GetLayerName(levelManager.midgroundLayerId);
                    int sourceId = tileMap.GetCellSourceId(levelManager.midgroundLayerId, cellCoords);
                    if (sourceId != -1)
                    {
                        int platformId = levelManager.GetPlatformId(cellCoords);
                        // GD.Print($"Player at cell {cellCoords}, Platform ID: {platformId}, TileSet Atlas ID: {sourceId}");
                        return (platformId, layerName, sourceId);
                    }
                }
            }
        }
        GD.PrintErr("No valid platform found near player");
        return (-1, string.Empty, -1);
    }

    public override void _PhysicsProcess(double delta)
    {
        Vector2 velocity = Velocity;

        if (!IsOnFloor())
        {
            velocity.Y += Gravity * (float)delta;
            animatedSprite.Play("Jump");

            // If jumping up or is there is an x velocity during a jump then set highest y position while not on the floor
            var goingHorizontal = Math.Abs(velocity.X) > Math.Abs(velocity.Y); // Takes the highest even thought going horizonal
            if (velocity.Y < 0 && GlobalPosition.Y < jumpYPos || goingHorizontal)
            {
                highestYPos = GlobalPosition.Y;
            }
            jumpYPos = GlobalPosition.Y;

            // If collided with wall set highest vector.y to previous platform.y if platform.y is higher then highestYpos
            if(IsOnWall())
            {
                // If highestYPos is lowerer then platformYPos, then use the height from the last platformYPos
                if (highestYPos > previousYPos)
                {
                    highestYPos = previousYPos;
                }
            }
        }

        if (IsOnFloor())
        {
            if (lastScoredPosition == Vector2.Zero)
            {
                lastScoredPosition = GlobalPosition;
                GD.Print($"Initial landing position set: {lastScoredPosition}");
                startingYpos = lastScoredPosition.Y;
            }

            currentYPos = GlobalPosition.Y;
            if (highestYPos != 0)
            {
                playJumpAudio(jumpSFX);
                if (!IsOnCeiling() && !IsOnWall())
                {
                    CalculateScore();
                }
                
                float heightDifference = Math.Abs(currentYPos) - Math.Abs(highestYPos);
                if (heightDifference >= distanceForHeadOnFloor)
                {
                    headOnFloor = true;
                    numOfFaceplants++;
                    distanceOfFurthestFaceplant = (int)heightDifference;
                    playJumpAudio(faceplantSFX);
                    EmitSignal(SignalName.Faceplanted, this, heightDifference);
                }
                highestYPos = 0;
            }

            if (headOnFloor)
                animatedSprite.Play("HeadOnFloor");
            else
                animatedSprite.Play("Idle");

            // Apply jump velocity if there's a pending jump
            if (_jumpVelocity != Vector2.Zero)
            {
                previousYPos = currentYPos;
                // _ = stop warning error :)
                _ = showDisplayName(2.0);
                velocity = _jumpVelocity;
                _jumpVelocity = Vector2.Zero; // Reset jump velocity after applying
            }
            else
            {
                velocity.X = 0; // Stop horizontal movement when on floor and not jumping
            }
        }

        Velocity = velocity;
        MoveAndSlide();
    }

    public void ResetPlayerState()
    {
        points = 0;
        combo = 0;
        comboStreak = 0;
        numOfFaceplants = 0;
        distanceOfFurthestFaceplant = 0;
        
        currentYPos = GlobalPosition.Y;
        previousYPos = GlobalPosition.Y;
        highestYPos = 0;
        jumpYPos = GlobalPosition.Y;
        highestYPosition = float.MaxValue;
        
        headOnFloor = false;
        lastScoredPosition = Vector2.Zero;
        currentPlatformId = -1;
        
        Velocity = Vector2.Zero;
        _jumpVelocity = Vector2.Zero;
        
        recentPlatforms.Clear();
        
        animatedSprite.Play("Idle");
        animatedSprite.FlipH = false;
    }


    public void Die()
    {
        EmitSignal(SignalName.Died, displayName, userID, team.TeamAbbreviation);
        // death logic, play animation, remove from scene
    }

    public void AddScore(int points)
    {
        if (team == null)
        {
            // GD.PrintErr($"Player: Cannot add score for {displayName}. Team is null.");
            return;
        }
        this.points += points;
        // GD.Print($"Player: {displayName} scored {points} points. New total: {this.points}");
        EmitSignal(SignalName.ScoreUpdated, team.TeamAbbreviation.ToUpper(), points);
    }

    private void OnHeadOnFloorAnimationFinished()
    {
        if (animatedSprite.Animation == "HeadOnFloor")
        {
            currentYPos = 0;
            previousYPos = 0;
            headOnFloor = false;
        }
    }

    private void OnDeleteSelf()
    {
        if (userID == "DEBUG")
        {
            GameManager.Instance.DeletePlayer("DEBUG");
        }
        
    }

    private void OnDebuggerCommandsGiven(float angle, float power)
    {
        GD.Print($"{displayName} has {angle} degrees and {power} power, jumpvelocity {BaseJumpVelocity}");
        if (userID != "DEBUG")
            return;
        DoJumpPhysics(angle, power);
    }

    public void DoJumpPhysics(float angle, float power)
    {
        if (headOnFloor)
            return;

        _jumpVelocity = MyMaths.SetJumpVector(angle, power, BaseJumpVelocity);
        
        if (_jumpVelocity.X < 0)
            animatedSprite.FlipH = true;
        else
            animatedSprite.FlipH = false;
    }

    private void OnMessageReceived(string[] messageInfo)
    {
        if (!IsInstanceValid(this)) return; // Early return if this Player instance is no longer valid
        if (!IsOnFloor()) return; // Only process jump commands when on the floor

        if (messageInfo[1] != userID)
            return;

        var (isValid, angle, power) = MessageParser.ParseMessage(messageInfo[2]);
        if (isValid)
        {
            DoJumpPhysics(angle, power);
        }
    }

    public string GetUserID()
    {
        return userID;
    }

    public async Task showDisplayName(double time)
    {
        displayLabel.Visible = true;
        await ToSignal(GetTree().CreateTimer(time), "timeout");
        displayLabel.Visible = false;
    }

    public void SetTeamColours(Color[] colourArray, ShaderMaterial uniqueMaterial)
    {
        if (colourArray == null || colourArray.Length < 5)
        {
            GD.PrintErr($"Player: Invalid colourArray for {displayName}. Length: {colourArray?.Length ?? 0}");
            return;
        }

        if (uniqueMaterial == null)
        {
            GD.PrintErr($"Player: uniqueMaterial is null for {displayName}");
            return;
        }

        try
        {
            uniqueMaterial.SetShaderParameter("cape1_color_new", colourArray[0]);
            uniqueMaterial.SetShaderParameter("cape2_color_new", colourArray[1]); 
            uniqueMaterial.SetShaderParameter("armour_dark_new", colourArray[2]);
            uniqueMaterial.SetShaderParameter("armour_med_new", colourArray[3]);
            uniqueMaterial.SetShaderParameter("armour_light_new", colourArray[4]);

            if (displayName == "DEBUG")
            {
                GD.Print("Player: DEBUG player, skipping helmet feathers");
                return;
            }

            if (levelManager == null)
            {
                GD.PrintErr($"Player: levelManager is null for {displayName}");
                return;
            }

            if (levelManager.teamScores == null)
            {
                GD.PrintErr($"Player: levelManager.teamScores is null for {displayName}");
                return;
            }

            if (team == null)
            {
                GD.PrintErr($"Player: team is null for {displayName}");
                return;
            }

            int playerCount = levelManager.teamScores.GetTeamPlayerCount(team.TeamAbbreviation);
            // GD.Print($"Player: Team {team.TeamAbbreviation} player count: {playerCount}");

            if (levelManager.uniqueColours == null)
            {
                GD.PrintErr($"Player: levelManager.uniqueColours is null for {displayName}");
                return;
            }

            // GD.Print($"Player: levelManager.uniqueColours length: {levelManager.uniqueColours.Length}");

            if (playerCount >= 0 && levelManager.uniqueColours.Length > 0)
            {
                int colorIndex = playerCount % levelManager.uniqueColours.Length;
                uniqueMaterial.SetShaderParameter("helmet_feathers_new", levelManager.uniqueColours[colorIndex]);
                idxOfUniqueFeatherColour = colorIndex;
            }
            else
            {
                GD.PrintErr($"Player: Unable to set helmet feather color. PlayerCount: {playerCount}, UniqueColours length: {levelManager.uniqueColours.Length}");
            }

            if (levelManager.uniqueColours == null)
            {
                GD.PrintErr($"Player: levelManager.uniqueColours is null for {displayName}");
                return;
            }

            if (playerCount > 0 && levelManager.uniqueColours.Length > 0)
            {
                int colorIndex = (playerCount - 1) % levelManager.uniqueColours.Length;
                uniqueMaterial.SetShaderParameter("helmet_feathers_new", levelManager.uniqueColours[colorIndex]);
                idxOfUniqueFeatherColour = colorIndex;
            }
            else
            {
                GD.PrintErr($"Player: Unable to set helmet feather color. PlayerCount: {playerCount}, UniqueColours length: {levelManager.uniqueColours.Length}");
            }
        }
        catch (Exception e)
        {
            GD.PrintErr($"Player: Error in SetTeamColours for {displayName} - {e.Message}\n{e.StackTrace}");
        }
    }

    private void SetColoursArray(UNL.Team team)
    {
        teamColours = new Color[5];
        if (team == null)
        {
            GD.Print("Player: Team is null, using default colors");
            teamColours[0] = Color.FromHtml("#fff");
            teamColours[1] = Color.FromHtml("#eba724");
            teamColours[2] = Color.FromHtml("#d2202c");
            teamColours[3] = Color.FromHtml("#7c776f");
            teamColours[4] = Color.FromHtml("#eadfd1");
        }
        else
        {
            teamColours[0] = team.TeamColours.CapeMain;
            teamColours[1] = team.TeamColours.CapeTrim;
            teamColours[2] = team.TeamColours.ArmourLight;
            teamColours[3] = team.TeamColours.ArmourMedium;
            teamColours[4] = team.TeamColours.ArmourDark;
        }
    }

    public void OnlyShowDisplayName()
    {
        displayLabel.Visible = true;
    }

    public void OnlyHideDisplayName()
    {
        displayLabel.Visible = false;
    }
}

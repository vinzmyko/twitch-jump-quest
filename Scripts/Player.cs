using System;
using System.Threading.Tasks;
using Godot;
using UNLTeamJumpQuest.TwitchIntegration;

public partial class Player : CharacterBody2D
{
    [Signal]
    public delegate void DiedEventHandler(string displayName, string userID, string teamAbbrev);
    [Signal]
    public delegate void ScoreUpdatedEventHandler(string teamAbbrev, int points);
	AnimatedSprite2D animatedSprite;
    Label displayLabel;

    [Export]
    public float BaseJumpVelocity = 500.0f;
    [Export]
    public float Gravity = 980.0f; 
    [Export]
    public float distanceForHeadOnFloor = 300;
    public bool headOnFloor = false;

    private float currentYPos = 0;
    private float previousYPos = 0;
    private float highestYPos = 0;
    private float jumpYPos = 0;

    private Vector2 _jumpVelocity = Vector2.Zero;

    public string userID;
    public string displayName;
    private UNL.Team team;
    private Color[] teamColours;
    private DebugTwitchChat debugger;
    private SettingsManager settingsManager;
    private LevelManager levelManager;
    float personalHighestYPos;
    int points;

    private Label DEBUG_LABEL;

    public void Initialize(string _displayName, string _userID, UNL.Team _team)
    {
        displayName = _displayName;
        userID = _userID;
        points = 0;
        team = _team;
        SetColoursArray(_team);
        if (_team == null)
        {
            return;
        }
    }
    public override async void _Ready()
    {
        base._Ready();

        debugger = GetNodeOrNull<DebugTwitchChat>("/root/Main/CanvasLayer/DebugTwitchChat");
        settingsManager = GetNodeOrNull<SettingsManager>("/root/SettingsManager");
        levelManager = GetNodeOrNull<LevelManager>("/root/LevelManager");
        

        if (debugger != null)
        {
            debugger.DebuggerDeleteSelf += OnDeleteSelf;
        }

		animatedSprite = GetNode<AnimatedSprite2D>("AnimatedSprite2D");
        displayLabel = GetNode<Label>("DisplayNameLabel");
        displayLabel.Text = displayName;
        ShaderMaterial uniqueMaterial = (ShaderMaterial)animatedSprite.Material.Duplicate();
        SetTeamColours(teamColours, uniqueMaterial);
        animatedSprite.Material = uniqueMaterial;

        TwitchBot.Instance.MessageReceived += OnMessageReceived;
        animatedSprite.AnimationFinished += OnHeadOnFloorAnimationFinished;

        if (userID == "DEBUG")
        {
            AddToGroup("DebugPlayer");
        }
        else
            AddToGroup("Player");

        personalHighestYPos = GlobalPosition.Y;
        DEBUG_LABEL = GetNode<Label>("DEBUG_POINTS_LABEL");
        // Some reason it won't change in Player scene so I do it through code.
        SetCollisionLayerValue(1, false);
        await showDisplayName(3.5);
    }

    private void CalculatePercentageDistance()
    {
        // Total level distance
        float totalDistance = levelManager.totalLevelYDistance;
        
        // Player's starting Y position (assumed to be at the bottom of the level)
        float startY = levelManager.startMarkerYPos;
        
        // Player's current Y position
        float currentY = GlobalPosition.Y;
        
        // Calculate distance traveled
        float distanceTraveled = Math.Abs(currentY - startY);
        
        // Calculate progress as a fraction
        float progressFraction = distanceTraveled / totalDistance;
        
        // Convert to a percentage (optional)
        float progressPercentage = progressFraction * 100;

        int pointsGiven = (int)(progressFraction * 100) / 2;
        AddScore(pointsGiven);
        DEBUG_LABEL.Visible = true;
        DEBUG_LABEL.Text = $"+{pointsGiven}";
    }

    public void Die()
    {
        EmitSignal(SignalName.Died, displayName, userID, team.TeamAbbreviation);
        // death logic, play animation, remove from scene
    }

    public void AddScore(int points)
    {
        this.points += points;
        EmitSignal(SignalName.ScoreUpdated, team.TeamAbbreviation, points);
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
        if (!IsOnFloor()) return; // Only process jump commands when on the floor

        if (messageInfo[1] != userID)
            return;

        var (isValid, angle, power) = MessageParser.ParseMessage(messageInfo[2]);
        if (isValid)
        {
            DoJumpPhysics(angle, power);
        }
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
            // Sets landing y pos to current y value
            // If just landed from jump calculate jump distance, if distance > required distance, faceplant
            currentYPos = GlobalPosition.Y;
            if (highestYPos != 0)
            {
                var differenceBetweenPlatforms = currentYPos - previousYPos;
                bool isOnSamePlatform = differenceBetweenPlatforms > -15 && differenceBetweenPlatforms < 15;
                if (!isOnSamePlatform)
                {
                    if (IsOnCeiling())
                    {
                        return;
                    }
                    if (IsOnWall())
                    {
                        return;
                    }
                    if (GlobalPosition.Y < personalHighestYPos)
                    {
                        personalHighestYPos = GlobalPosition.Y;
                        CalculatePercentageDistance();
                    }
                    // Check to see if it's your highest position
                    // If it's your highest position set the points through
                }
                
                float heightDifference = Math.Abs(currentYPos) - Math.Abs(highestYPos);
                if (heightDifference >= distanceForHeadOnFloor)
                {
                    headOnFloor = true;
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

    public string GetUserID()
    {
        return userID;
    }

    private async Task showDisplayName(double time)
    {
        displayLabel.Visible = true;
        await ToSignal(GetTree().CreateTimer(time), "timeout");
        displayLabel.Visible = false;
    }

    public void SetPlayerColours(Color cape1Color, Color cape2Color, Color helmetFeathersColor, 
        Color armourDarkColor, Color armourMedColor, Color armourLightColor)
    {
        ShaderMaterial material = (ShaderMaterial)animatedSprite.Material;
        material.SetShaderParameter("cape1_color_new", cape1Color); // 0a7030
        material.SetShaderParameter("cape2_color_new", cape2Color); // eba724
        material.SetShaderParameter("helmet_feathers_new", helmetFeathersColor); // d2202c
        material.SetShaderParameter("armour_dark_new", armourDarkColor); // 7c776f
        material.SetShaderParameter("armour_med_new", armourMedColor); // b3aaa1
        material.SetShaderParameter("armour_light_new", armourLightColor); // eadfd1
    }

    public void SetTeamColours(Color[] colourArray, ShaderMaterial uniqueMaterial)
    {
        // int teamPlayerCount = levelManager.teamScores.GetTeamPlayerCount(team.TeamAbbreviation);
        uniqueMaterial.SetShaderParameter("cape1_color_new", colourArray[0]);
        uniqueMaterial.SetShaderParameter("cape2_color_new", colourArray[1]); 
        uniqueMaterial.SetShaderParameter("armour_dark_new", colourArray[2]);
        uniqueMaterial.SetShaderParameter("armour_med_new", colourArray[3]);
        uniqueMaterial.SetShaderParameter("armour_light_new", colourArray[4]);
        if (displayName == "DEBUG")
        {
            return;
        }
        uniqueMaterial.SetShaderParameter("helmet_feathers_new", 
            levelManager.uniqueColours[(levelManager.teamScores.GetTeamPlayerCount(team.TeamAbbreviation) - 1) % 15]); 
    }

    private void SetColoursArray(UNL.Team team)
    {
        teamColours = new Color[5];
        if (team == null)
        {
            teamColours[0] = Color.FromHtml("#fff");
            teamColours[1] = Color.FromHtml("#eba724");
            teamColours[2] = Color.FromHtml("#d2202c");
            teamColours[3] = Color.FromHtml("#7c776f");
            teamColours[4] = Color.FromHtml("#eadfd1");
            return;
        }
        teamColours[0] = team.TeamColours.CapeMain;
        teamColours[1] = team.TeamColours.CapeTrim;
        teamColours[2] = team.TeamColours.ArmourLight;
        teamColours[3] = team.TeamColours.ArmourMedium;
        teamColours[4] = team.TeamColours.ArmourDark;
    }
}

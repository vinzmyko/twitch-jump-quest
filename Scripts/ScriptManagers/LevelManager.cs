using Godot;
using System;
using System.Linq;

public partial class LevelManager : Node
{
    [Signal]
    public delegate void PlayerSpawnedEventHandler(Player player);
    [Signal]
    public delegate void TeamScoreUpdatedEventHandler(string teamAbbrev, int points);
    [Signal]
    public delegate void PlayerComboStreakingToUIEventHandler(Player player, int comboStreak);
    [Signal]
    public delegate void PlayerFaceplantToUIEventHandler(Player player, float distance);

    public UNL.TeamScoreManager teamScores = new UNL.TeamScoreManager();
    public Color[] uniqueColours = new Color[15]
    {
        Color.FromHtml("#0000FF"),  // Blue
        Color.FromHtml("#FF0000"),  // Red
        Color.FromHtml("#00FF00"),  // Green
        Color.FromHtml("#FFFF00"),  // Yellow
        Color.FromHtml("#00FFFF"),  // Cyan
        Color.FromHtml("#800080"),  // Purple
        Color.FromHtml("#FFA500"),  // Orange
        Color.FromHtml("#FF69B4"),  // Hot Pink
        Color.FromHtml("#808080"),  // Gray
        Color.FromHtml("#008000"),  // Dark Green
        Color.FromHtml("#8B4513"),  // Saddle Brown
        Color.FromHtml("#40E0D0"),  // Turquoise
        Color.FromHtml("#FF00FF"),  // Magenta
        Color.FromHtml("#C0C0C0"),  // Silver
        Color.FromHtml("#800000")   // Maroon
    };
    private Marker2D debugSpawnPosition;
    private Node2D spawnPositions;
    private PackedScene playerScene;
    SettingsManager settingsManager;
    private int currentLevel;
    public int totalLevelYDistance = 0;
    public int endMarkerYPos = 0;
    public int startMarkerYPos = 0;
    public float startMarkerXPos = 0;

    private TileMap tileMap;
    private PlatformIdentifier platformIdentifier;
    public int midgroundLayerId;
    private Node root;
    private Node2D levelNode;
    private PackedScene GGScene;
    private EndGameAnimationToEndGameScreen ggInstance;
    int rng;


    public override void _Ready()
    {
        base._Ready();
        GetNode<SceneManager>("/root/SceneManager").LevelReady += InitLevelDependencies;
        
        settingsManager = GetNodeOrNull<SettingsManager>("/root/SettingsManager");
        playerScene = ResourceLoader.Load<PackedScene>("res://Scenes/Player.tscn");

        GGScene = ResourceLoader.Load<PackedScene>("res://Scenes/EndGameScreen/EndGameAnimationToEndGameScreen.tscn");

        GameManager.Instance.PlayerJoined += SpawnPlayer;
        GameManager.Instance.GameStateChanged += OnGameStateChanged;
        Random random = new Random();
        rng = random.Next();
    }

    private void OnGameStateChanged(int gameState)
    {
        if (gameState == (int)GameManager.GameState.GameOver)
        {
            ShowEndGameAnimation();

            var alivePlayers = GetTree().GetNodesInGroup("Player");
            foreach (Player player in alivePlayers)
            {
                GameManager.Instance.AddToStatTrackingList(player);
            }
        }
    }

    private void OnEndGameAnimationFinished()
    {
        if (ggInstance != null && IsInstanceValid(ggInstance))
        {
            // Remove the instance from its parent
            if (ggInstance.GetParent() != null)
            {
                ggInstance.GetParent().RemoveChild(ggInstance);
            }
            ggInstance.Cleanup();
            ggInstance = null;
        }
        SceneManager.Instance.ChangeScene("EndGameScreen");
    }

    private void ShowEndGameAnimation()
    {
        if (ggInstance != null && IsInstanceValid(ggInstance))
        {
            // If the instance already exists, remove it from its current parent
            if (ggInstance.GetParent() != null)
            {
                ggInstance.GetParent().RemoveChild(ggInstance);
            }
        }
        else
        {
            // If the instance doesn't exist or is invalid, create a new one
            ggInstance = GGScene.Instantiate<EndGameAnimationToEndGameScreen>();
            ggInstance.AnimationFinished += OnEndGameAnimationFinished;
        }

        if (levelNode != null && IsInstanceValid(levelNode))
        {
            var canvasLayer = levelNode.GetNodeOrNull<CanvasLayer>("CanvasLayer");
            if (canvasLayer != null && IsInstanceValid(canvasLayer))
            {
                canvasLayer.AddChild(ggInstance);
                ggInstance.PlayAnimation();
                GD.Print("EndGameAnimation added to CanvasLayer and started playing");
            }
            else
            {
                GD.PrintErr("CanvasLayer not found or not valid");
            }
        }
        else
        {
            GD.PrintErr("LevelNode not found or not valid");
        }
    }

    private void InitLevelDependencies(string levelName)
    {
        root = GetTree().Root;
        levelNode = root.GetChild(root.GetChildCount() - 1) as Node2D;

        debugSpawnPosition = levelNode.GetNode<Marker2D>("DebugSpawnMarker2D");

        tileMap = levelNode.GetNode<TileMap>("TileMap");
        midgroundLayerId = FindMidgroundLayerId();
        platformIdentifier = new PlatformIdentifier(tileMap, midgroundLayerId);
        platformIdentifier.IdentifyPlatforms();

        spawnPositions = levelNode.GetNode<Node2D>("PlayerSpawns");
        InitLevelMarkers();

        var gameTimer = levelNode.GetNode<GameTimer>("CanvasLayer/GameTimer");
        gameTimer.waitTimeFinished += OnWaitTimeFinished;
    }

    private void OnWaitTimeFinished()
    {
        CallDeferred(nameof(MovePlayersToSpawnPositions));
    }

    private async void MovePlayersToSpawnPositions()
    {
        var playerArray = GetTree().GetNodesInGroup("Player");
        var spawnPositionsArray = spawnPositions.GetChildren().Cast<Node2D>().ToArray();

        Random random = new Random();
        var shuffledSpawnPositions = spawnPositionsArray.OrderBy(x => random.Next()).ToList();

        GD.Print($"Number of players: {playerArray.Count}");
        for (int i = 0; i < playerArray.Count; i++)
        {
            if (playerArray[i] is Player player)
            {
                int smallDisplacementRng = random.Next(-10, 11);
                Godot.Vector2 smallDisplacement = new Godot.Vector2(smallDisplacementRng, 0);

                int spawnIndex = i % shuffledSpawnPositions.Count;
                player.GlobalPosition = shuffledSpawnPositions[spawnIndex].GlobalPosition + smallDisplacement;
                player.ResetPlayerState();
                GD.Print($"Moved player {player.Name} to position {player.GlobalPosition}");
                player.OnlyShowDisplayName();
            }
            else
            {
                GD.PrintErr($"Node in 'Player' group is not a Player: {playerArray[i].Name}");
            }
        }
        
        await ToSignal(GetTree().CreateTimer(10.0f), "timeout");

        foreach (var node in playerArray)
        {
            if (node is Player player)
            {
                player.OnlyHideDisplayName();
            }
        }
    }

    private int FindMidgroundLayerId()
    {
        for (int i = 0; i < tileMap.GetLayersCount(); i++)
        {
            if (tileMap.GetLayerName(i) == "Midground")
            {
                return i;
            }
        }
        GD.PrintErr("Midground layer not found!");
        return -1;
    }

    public int GetPlatformId(Vector2I cellPos)
    {
        int platformId = platformIdentifier.GetPlatformId(cellPos);
        // GD.Print($"GetPlatformId called for cell {cellPos}, returned ID: {platformId}");
        return platformId;
    }

    private void InitLevelMarkers()
    {
        var root = GetTree().Root;
        var levelNode = root.GetChild(root.GetChildCount() - 1);
        Marker2D startingMarker = levelNode.FindChild("LevelMarkers").GetChild(0) as Marker2D;
        Marker2D endingMarker = levelNode.FindChild("LevelMarkers").GetChild(1) as Marker2D;
        totalLevelYDistance = (int)Math.Abs(endingMarker.GlobalPosition.Y - startingMarker.GlobalPosition.Y);
        endMarkerYPos = (int)endingMarker.GlobalPosition.Y;
        startMarkerYPos = (int)startingMarker.GlobalPosition.Y;
        startMarkerXPos = (float)startingMarker.GlobalPosition.X;
    }

    private void OnPlayerDied(string displayName, string userID, string teamAbbrev)
    {
        
    }

    public void LoadLevel(int levelNumber)
    {
        // TODO: Implement level loading logic
    }

    // Method to spawn a player in the current level
    public void SpawnPlayer(string displayName, string userID, string teamAbbrev)
    {
        // GD.Print($"LevelManager: Attempting to spawn player - Name: {displayName}, ID: {userID}, Team: {teamAbbrev}");

        if (playerScene == null)
        {
            GD.PushError("LevelManager: Player scene is not set in the LevelManager");
            return;
        }

        CharacterBody2D instance = (CharacterBody2D)playerScene.Instantiate();

        if (instance is not Player playerInstance)
        {
            GD.PrintErr("LevelManager: Failed to instantiate Player");
            return;
        }

        // GD.Print("LevelManager: Player instance created successfully");

        UNL.Team targetTeam = null;
        GD.Print($"LevelManager: Number of teams in settingsManager: {settingsManager.UNLTeams.Teams.Count}");
        foreach (UNL.Team team in settingsManager.UNLTeams.Teams)
        {
            // GD.Print($"LevelManager: Checking team {team.TeamAbbreviation}");
            if (team.TeamAbbreviation.ToLower() == teamAbbrev.ToLower())
            {
                targetTeam = team;
                // GD.Print($"LevelManager: Matched team found: {team.TeamAbbreviation}");
                break;
            }
        }

        if (targetTeam == null)
        {
            GD.PrintErr($"LevelManager: No matching team found for abbreviation {teamAbbrev}");
        }

        UNL.Team isATeam = IsATeam(teamAbbrev);
        if (isATeam != null)
        {
            // GD.Print($"LevelManager: IsATeam returned non-null for {teamAbbrev}");
            // GD.Print($"LevelManager: Team details - Name: {isATeam.TeamName}, Abbreviation: {isATeam.TeamAbbreviation}");
            teamScores.AddTeam(isATeam);
            teamScores.AddPlayerToTeam(isATeam.TeamAbbreviation);
            // GD.Print($"LevelManager: Added team {isATeam.TeamAbbreviation} and player to the team");
            // PrintTeamScoresState();
        }
        else
        {
            GD.PrintErr($"LevelManager: IsATeam returned null for {teamAbbrev}");
        }

        playerInstance.Initialize(displayName, userID, targetTeam);
        playerInstance.Name = $"Player_{userID}";
        GD.Print($"LevelManager: Player initialized with name {playerInstance.Name}");

        if (playerInstance is Player player)
        {
            player.ScoreUpdated += OnPlayerScoreUpdated;
            player.Died += (string name, string userid, string teamAbbrev) => {};
            player.ComboStreaking += (Player player, int comboStreak) => {EmitSignal(SignalName.PlayerComboStreakingToUI, player, comboStreak);};
            player.Faceplanted += (Player player, float distance) => {EmitSignal(SignalName.PlayerFaceplantToUI, player, distance);};
            GD.Print("LevelManager: Player events connected");
        }
        
        if (spawnPositions == null)
        {
            GD.PushError("LevelManager: spawnPosition not found");
        }
        else
        {
            Random random = new Random();

            var spawnPositionsArray = spawnPositions.GetChildren().Cast<Node2D>().ToArray();
            GameManager gameManager = GetNode<GameManager>("/root/GameManager");

            int spawnIndex = ( rng + gameManager.players.Count ) % spawnPositionsArray.Length;
            playerInstance.GlobalPosition = spawnPositionsArray[spawnIndex].GlobalPosition;

            GD.Print($"LevelManager: Player spawned at position {playerInstance.GlobalPosition}");
        }

        if (playerInstance.displayName == "DEBUG")
        {
            playerInstance.GlobalPosition = debugSpawnPosition.GlobalPosition;
            GD.Print($"LevelManager: DEBUG player spawned at position {playerInstance.GlobalPosition}");
        }


        Node levelScene = GetNode<Node>("/root/Main");
        levelScene.AddChild(playerInstance);

        playerInstance.SetupPlayerColors();
        
        EmitSignal(SignalName.PlayerSpawned, playerInstance);
    }

    private void OnPlayerScoreUpdated(string teamAbbrev, int playerAdditionalPoints)
    {
        if (string.IsNullOrEmpty(teamAbbrev))
        {
            GD.PrintErr("LevelManager: OnPlayerScoreUpdated received null or empty teamAbbrev");
            return;
        }

        if (teamScores == null)
        {
            GD.PrintErr("LevelManager: teamScores is null in OnPlayerScoreUpdated");
            return;
        }

        string key = teamAbbrev.ToUpper();
        UNL.TeamScore teamScore = teamScores.GetTeamScore(key);
        if (teamScore == null)
        {
            GD.PrintErr($"LevelManager: No team score found for team {key}");
            return;
        }

        teamScores.AddScoreToTeam(key, playerAdditionalPoints);
        EmitSignal(SignalName.TeamScoreUpdated, key, teamScore.TotalScore);
        // GD.Print($"LevelManager: Updated score for team {key}. New total: {teamScore.TotalScore}");
    }

    private UNL.Team IsATeam(string teamAbbrev)
    {
        UNL.Team returnTeam = new UNL.Team
        {
            TeamName = "Bug in IsATeam() function",
            TeamAbbreviation = "bug",
            LogoPath = "null",
            TeamColours = null,
            HexColourCode = "#BUG"

        };
        UNL.Team teamAbbrevTeam = settingsManager.UNLTeams.Teams.Find(team => team.TeamAbbreviation.ToLower() == teamAbbrev.ToLower());
        if (teamAbbrevTeam != null)
        {
            returnTeam = teamAbbrevTeam;
        }
        return returnTeam;
    }
}
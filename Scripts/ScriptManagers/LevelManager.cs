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
    private PackedScene GG;

    public override void _Ready()
    {
        base._Ready();
        GetNode<SceneManager>("/root/SceneManager").LevelReady += InitLevelDependencies;
        
        settingsManager = GetNodeOrNull<SettingsManager>("/root/SettingsManager");
        playerScene = ResourceLoader.Load<PackedScene>("res://Scenes/Player.tscn");

        GG = ResourceLoader.Load<PackedScene>("res://Scenes/EndGameScreen/EndGameAnimationToEndGameScreen.tscn");
        var ggInstance = GG.Instantiate();

        GameManager.Instance.PlayerJoined += SpawnPlayer;
        // GameManager.Instance.PlayerDied += OnPlayerDied;
        GameManager.Instance.GameStateChanged += (int gameState) => 
        {
            if (gameState == (int)GameManager.GameState.GameOver)
            { 
                levelNode.GetNode<CanvasLayer>("CanvasLayer").AddChild(ggInstance);
                var alivePlayers = GetTree().GetNodesInGroup("Player");
                GameManager gameManager = GetNode<GameManager>("/root/GameManager");
                foreach (Player player in alivePlayers)
                {
                    gameManager.AddToStatTrackingList(player);
                }
            }
        };
    }

    private void InitLevelDependencies(string levelName)
    {
        root = GetTree().Root;
        levelNode = root.GetChild(root.GetChildCount() - 1) as Node2D;

        debugSpawnPosition = levelNode.GetNode<Marker2D>("DebugSpawnMarker2D");

        tileMap = levelNode.GetNode<TileMap>("TileMap"); // Make sure this path is correct
        midgroundLayerId = FindMidgroundLayerId();
        platformIdentifier = new PlatformIdentifier(tileMap, midgroundLayerId);
        platformIdentifier.IdentifyPlatforms();

        int playerCount = 0;
        Random random = new Random();
        int rng = random.Next(0, 10);
        levelNode.GetNode<GameTimer>("CanvasLayer/GameTimer").waitTimeFinished += () => 
        {
            var playerArray = GetTree().GetNodesInGroup("Player");
            foreach (var node in playerArray)
            {
                // Spread it out so all players use the spawn positions
                var player = node as Player;
                int idxPos = ( rng + playerCount ) % playerArray.Count;
                playerCount++;
                Node2D positionToSpawn = spawnPositions.GetChild<Node2D>(idxPos);
                player.GlobalPosition = positionToSpawn.GlobalPosition;
                player.ResetPlayerState();               
            }
        };
        spawnPositions = levelNode.GetNode<Node2D>("PlayerSpawns");
        InitLevelMarkers();
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
        GD.Print($"GetPlatformId called for cell {cellPos}, returned ID: {platformId}");
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
        GD.Print($"LevelManager: Attempting to spawn player - Name: {displayName}, ID: {userID}, Team: {teamAbbrev}");

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

        GD.Print("LevelManager: Player instance created successfully");

        UNL.Team targetTeam = null;
        GD.Print($"LevelManager: Number of teams in settingsManager: {settingsManager.UNLTeams.Teams.Count}");
        foreach (UNL.Team team in settingsManager.UNLTeams.Teams)
        {
            GD.Print($"LevelManager: Checking team {team.TeamAbbreviation}");
            if (team.TeamAbbreviation.ToLower() == teamAbbrev.ToLower())
            {
                targetTeam = team;
                GD.Print($"LevelManager: Matched team found: {team.TeamAbbreviation}");
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
            GD.Print($"LevelManager: IsATeam returned non-null for {teamAbbrev}");
            GD.Print($"LevelManager: Team details - Name: {isATeam.TeamName}, Abbreviation: {isATeam.TeamAbbreviation}");
            // Does check for dupe teams
            teamScores.AddTeam(isATeam);
            teamScores.AddPlayerToTeam(isATeam.TeamAbbreviation);
            GD.Print($"LevelManager: Added team {isATeam.TeamAbbreviation} and player to the team");
            PrintTeamScoresState();
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
            int rng = random.Next(0, spawnPositions.GetChildren().Count);
            Node2D positionToSpawn = spawnPositions.GetChild<Node2D>(rng);
            playerInstance.GlobalPosition = positionToSpawn.GlobalPosition;
            GD.Print($"LevelManager: Player spawned at position {playerInstance.GlobalPosition}");
        }

        if (playerInstance.displayName == "DEBUG")
        {
            playerInstance.GlobalPosition = debugSpawnPosition.GlobalPosition;
            GD.Print($"LevelManager: DEBUG player spawned at position {playerInstance.GlobalPosition}");
        }

        Node levelScene = GetNode<Node>("/root/Main");
        levelScene.AddChild(playerInstance);
        GD.Print($"LevelManager: Player added to scene {levelScene.Name}");

        EmitSignal(SignalName.PlayerSpawned, playerInstance);
        GD.Print("LevelManager: PlayerSpawned signal emitted");
    }

    private void PrintTeamScoresState()
    {
        GD.Print("LevelManager: Printing teamScores state");
        GD.Print($"LevelManager: teamScores is null: {teamScores == null}");
        if (teamScores != null)
        {
            var allTeams = teamScores.GetAllTeamScores().Select(ts => ts.TeamInfo.TeamAbbreviation).ToList();
            GD.Print($"LevelManager: All teams in teamScores: {string.Join(", ", allTeams)}");
            foreach (var team in allTeams)
            {
                GD.Print($"LevelManager: Team {team} exists: {teamScores.TeamExists(team)}");
            }
        }
    }


    private void OnPlayerScoreUpdated(string teamAbbrev, int playerAdditionalPoints)
    {
        teamScores.AddScoreToTeam(teamAbbrev, playerAdditionalPoints);
        UNL.TeamScore teamsScore = teamScores.GetTeamScore(teamAbbrev);
        EmitSignal(SignalName.TeamScoreUpdated, teamAbbrev, teamsScore.TotalScore);
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

    // Method to update level state (e.g., obstacles, challenges)
    public void UpdateLevelState()
    {
        // TODO: Implement level state update logic
    }

    // Method to check if a player has completed the level
    public bool IsLevelComplete(Player player)
    {
        // TODO: Implement level completion check
        return false;
    }
}

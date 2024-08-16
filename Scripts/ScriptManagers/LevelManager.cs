using Godot;
using System;
using System.Collections.Generic;

public partial class LevelManager : Node
{
    [Signal]
    public delegate void PlayerSpawnedEventHandler(Player player);
    // [Signal]
    // public delegate void TeamScoresUpdatedEventHandler();
    [Signal]
    public delegate void TeamScoreUpdatedEventHandler(string teamAbbrev, int points);

    // public Dictionary<string, UNL.TeamScore> teamScores = new Dictionary<string, UNL.TeamScore>();
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

    Marker2D spawnPosition;
    private PackedScene playerScene;
    SettingsManager settingsManager;
    private int currentLevel;
    public int totalLevelYDistance = 0;
    public int endMarkerYPos = 0;
    public int startMarkerYPos = 0;
    public override void _Ready()
    {
        base._Ready();
        settingsManager = GetNodeOrNull<SettingsManager>("/root/SettingsManager");
        playerScene = ResourceLoader.Load<PackedScene>("res://Scenes/Player.tscn");
        GameManager.Instance.PlayerJoined += SpawnPlayer;
        // GameManager.Instance.PlayerJoined += OnPlayerDied;

        spawnPosition = GetNodeOrNull<Marker2D>("/root/Main/SpawnMarker2D");
        InitLevelMarkers();
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
    }

    private void OnPlayerDied(string displayName, string userID, string teamAbbrev)
    {

    }

    // Method to load a specific level

    public void LoadLevel(int levelNumber)
    {
        // TODO: Implement level loading logic
    }

    // Method to spawn a player in the current level
    public void SpawnPlayer(string displayName, string userID, string teamAbbrev)
    {
        if (playerScene == null)
        {
            GD.PushError("Player scene is not set in the LevelManager");
            return;
        }

        CharacterBody2D instance = (CharacterBody2D)playerScene.Instantiate();

        if (instance is not Player playerInstance)
        {
            GD.PrintErr("Failed to instantiate Player");
            return;
        }

        UNL.Team targetTeam = null;
        foreach (UNL.Team team in settingsManager.UNLTeams.Teams)
        {
            if (team.TeamAbbreviation.ToLower() == teamAbbrev.ToLower())
            {
                targetTeam = team;
                break;
            }
        }

        playerInstance.Initialize(displayName, userID, targetTeam);
        playerInstance.Name = $"Player_{userID}";

        if (playerInstance is Player player)
        {
            player.ScoreUpdated += OnPlayerScoreUpdated;
        }
        
        if (spawnPosition == null)
        {
            GD.PushError("spawnPosition not found");
        }
        else
            playerInstance.GlobalPosition = spawnPosition.GlobalPosition;

        // Add player to Level owner scene

        UNL.Team isATeam = IsATeam(teamAbbrev);
        if (isATeam != null)
        {
            // Does check for dupe teams
            teamScores.AddTeam(isATeam);
            teamScores.AddPlayerToTeam(isATeam.TeamAbbreviation);
            GD.Print("added team and player to the team");
        }

        Node levelScene = GetNode<Node>("/root/Main");
        levelScene.AddChild(playerInstance);

        // teamScores.AddTeam();
        EmitSignal(SignalName.PlayerSpawned, playerInstance);
    }

    private void OnPlayerScoreUpdated(string teamAbbrev, int playerAdditionalPoints)
    {
        GD.Print($"points being added to {teamAbbrev} = {playerAdditionalPoints}");
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

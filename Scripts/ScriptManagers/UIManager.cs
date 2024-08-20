using Godot;
using System;
using System.Collections.Generic;

public partial class UIManager : Node
{
    [Signal]
    public delegate void SetupCompletedEventHandler();
    private InformationDisplay informationDisplay;
    private TeamScoresDisplay teamScoresDisplay;

    public override void _Ready()
    {
        var rootNode = GetTree().Root;
        var levelNodesCanvasLayer = rootNode.GetChild(rootNode.GetChildCount() - 1).FindChild("CanvasLayer");
        informationDisplay = levelNodesCanvasLayer.FindChild("InformationDisplay") as InformationDisplay;
        teamScoresDisplay = levelNodesCanvasLayer.FindChild("TeamScoresDisplay") as TeamScoresDisplay;

        GameManager gameManager = GetNode<GameManager>("/root/GameManager");
        gameManager.PlayerJoined += OnPlayerJoined;
        gameManager.PlayerDied += OnPlayerDied;
        LevelManager levelManager = GetNode<LevelManager>("/root/LevelManager");
        levelManager.TeamScoreUpdated += OnTeamScoreUpdated;
    }

    private void OnTeamScoreUpdated(string teamAbbrev, int teamsTotalScore)
    {
        teamScoresDisplay.UpdateTeamScore(teamAbbrev, teamsTotalScore);
    }

    private void OnPlayerJoined(string displayName, string userID, string teamAbbrev)
    {
        informationDisplay.AddJoinMessage(displayName, teamAbbrev);
        teamScoresDisplay.AddTeamIfNotExists(teamAbbrev);
    }

    private void OnPlayerDied(string displayName, string userID, string teamAbbrev)
    {
        // Show that the player has died
        // player has died info display
        // teamScoresDisplay team--
    }
}
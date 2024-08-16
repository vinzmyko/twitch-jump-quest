using Godot;
using System;
using System.Collections.Generic;

public partial class UIManager : Node
{
    private InformationDisplay informationDisplay;
    private TeamScoresDisplay teamScoresDisplay;
   private string[] playerJoinMessages = new string[]
    {
        "{0} has joined the {1} roster!",
        "Welcome aboard, {0}! You're now part of {1}.",
        "{1} just got stronger with {0} joining the squad!",
        "Breaking news: {0} signs with {1}!",
        "{0} dons the {1} jersey for the first time.",
        "{1} fans, give a warm welcome to your newest teammate, {0}!",
        "{0} is ready to make waves with {1}.",
        "The {1} family grows as {0} comes on board.",
        "Look out, opponents! {0} is now playing for {1}.",
        "{1} just leveled up by adding {0} to the team!"
    };

    public override void _Ready()
    {
        var rootNode = GetTree().Root;
        var levelNodesCanvasLayer = rootNode.GetChild(rootNode.GetChildCount() - 1).FindChild("CanvasLayer");
        informationDisplay = levelNodesCanvasLayer.FindChild("InformationDisplay") as InformationDisplay;
        teamScoresDisplay = levelNodesCanvasLayer.FindChild("TeamScoresDisplay") as TeamScoresDisplay;

        GameManager gameManager = GetNode<GameManager>("/root/GameManager");
        gameManager.PlayerJoined += OnPlayerJoined;
        gameManager.PlayerDied += OnPlayerDied;
    }

    public void UpdateTeamScores(IEnumerable<UNL.TeamScore> teamScores)
    {
        // Update your UI elements here
        // For example, updating a scoreboard or team info display
        foreach (var teamScore in teamScores)
        {
            // Update UI for each team
            // Example:
            // scoreboardLabel.Text += $"{teamScore.TeamInfo.TeamAbbreviation}: {teamScore.TotalScore} (Players: {teamScore.PlayerCount})\n";
        }
    }
    private void OnPlayerJoined(string displayName, string userID, string teamAbbrev)
    {
        informationDisplay.AddJoinMessage(displayName, teamAbbrev);
        teamScoresDisplay.AddTeamIfNotExists(teamAbbrev);
    }

    private void OnPlayerDied(string displayName, string userID, string teamAbbrev)
    {
        // Show that the player has died
    }
}


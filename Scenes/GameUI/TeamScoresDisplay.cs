using Godot;
using System.Collections.Generic;
using System.Linq;

public partial class TeamScoresDisplay : Control
{
    [Export]
    private PackedScene teamScoreScene;
    private VBoxContainer teamsVBoxContainer;
    // Stores the teamAbbrev<score position>
    public Dictionary<string, int> teamPositionDictionary = new Dictionary<string, int>();
    private LevelManager levelManager;
    private SettingsManager settingsManager;

    public override void _Ready()
    {
        base._Ready();

        levelManager = GetNode<LevelManager>("/root/LevelManager");
        GD.Print($"TeamScoresDisplay: LevelManager reference obtained: {(levelManager != null ? "Yes" : "No")}");
        settingsManager = GetNode<SettingsManager>("/root/SettingsManager");

        teamsVBoxContainer = GetNode<VBoxContainer>("PanelContainer/VBoxContainer");
    }

    public void AddTeamIfNotExists(string teamAbbrev)
    {
        GD.Print($"TeamScoresDisplay: Attempting to add team {teamAbbrev}");
        if (levelManager == null || levelManager.teamScores == null)
        {
            GD.PrintErr($"TeamScoresDisplay: levelManager or teamScores is null for team {teamAbbrev}");
            return;
        }

        string upperTeamAbbrev = teamAbbrev.ToUpper();
        if (!levelManager.teamScores.TeamExists(upperTeamAbbrev))
        {
            if (upperTeamAbbrev == "DEBUG") { return; }
            GD.PushError($"Team {upperTeamAbbrev} does not exist in levelmanager");
            return;
        }

        if (!teamPositionDictionary.ContainsKey(upperTeamAbbrev))
        {
            TeamScore teamScoreInstance = teamScoreScene.Instantiate<TeamScore>();
            teamScoreInstance.Name = $"TEAM_{upperTeamAbbrev}";
            teamsVBoxContainer.AddChild(teamScoreInstance);
            teamScoreInstance.Initialise(upperTeamAbbrev);

            teamPositionDictionary[upperTeamAbbrev] = teamPositionDictionary.Count;
            GD.Print($"TeamScoresDisplay: Added new team score display for {upperTeamAbbrev}");
        }

        UpdateTeamScoreDisplay(upperTeamAbbrev, 0);
    }

    public void UpdateTeamScore(string teamAbbrev, int teamScore)
    {
        string upperTeamAbbrev = teamAbbrev.ToUpper();
        GD.Print($"TeamScoresDisplay: Updating score for team {upperTeamAbbrev} to {teamScore}");

        if (!teamPositionDictionary.ContainsKey(upperTeamAbbrev))
        {
            GD.PrintErr($"TeamScoresDisplay: Team {upperTeamAbbrev} not found in teamPositionDictionary. Adding it now.");
            AddTeamIfNotExists(upperTeamAbbrev);
        }

        UpdateTeamScoreDisplay(upperTeamAbbrev, teamScore);
    }

    private void UpdateTeamScoreDisplay(string upperTeamAbbrev, int teamScore)
    {
        TeamScore teamScoreLabel = teamsVBoxContainer.GetNodeOrNull($"TEAM_{upperTeamAbbrev}") as TeamScore;
        if (teamScoreLabel == null)
        {
            GD.PrintErr($"TeamScoresDisplay: TeamScore node for {upperTeamAbbrev} not found");
            return;
        }

        int playerCount = levelManager.teamScores.GetTeamPlayerCount(upperTeamAbbrev);
        teamScoreLabel.Text = $"{upperTeamAbbrev.PadRight(4)}\t[{playerCount}] -\t{teamScore:D6}";
        teamScoreLabel.teamScore = teamScore;

        SortTeamScores();
    }
    private void SortTeamScores()
    {
        var teamScores = teamsVBoxContainer.GetChildren()
            .Cast<TeamScore>()
            .OrderByDescending(team => team.teamScore)
            .ToList();

        for (int i = 0; i < teamScores.Count; i++)
        {
            teamsVBoxContainer.MoveChild(teamScores[i], i);
        }
    }
}
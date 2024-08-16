using Godot;
using System.Collections.Generic;

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
        settingsManager = GetNode<SettingsManager>("/root/SettingsManager");

        teamsVBoxContainer = GetNode<VBoxContainer>("PanelContainer/VBoxContainer");
    }

    public void AddTeamIfNotExists(string teamAbbrev)
    {
        if (!levelManager.teamScores.TeamExists(teamAbbrev))
        {
            GD.PushError("team does not exist in levelmanager");
            return;
        }
        if (!teamPositionDictionary.ContainsKey(teamAbbrev))
        {
            // Instantiate the teamScoreScene
            TeamScore teamScoreInstance = teamScoreScene.Instantiate<TeamScore>();
            teamScoreInstance.Name = $"TEAM_{teamAbbrev}";

            // Add the new instance to the VBoxContainer
            teamsVBoxContainer.AddChild(teamScoreInstance);

            // Set up properties for the new TeamScore instance
            teamScoreInstance.Initialise(teamAbbrev);

            // Store the position of the new team in the dictionary
            teamPositionDictionary[teamAbbrev] = teamPositionDictionary.Count;
        }
    }

    public void UpdateTeamScore(string teamAbbrev, int teamScore)
    {
        TeamScore teamRichTextLabel = teamsVBoxContainer.GetNode($"TEAM_{teamAbbrev}") as TeamScore;
        GD.Print($"playercount{ levelManager.teamScores.GetTeamPlayerCount(teamAbbrev) }");
        teamRichTextLabel.Text = $"{teamAbbrev} [{levelManager.teamScores.GetTeamPlayerCount(teamAbbrev)}] - {teamScore:D4}";
    }
}
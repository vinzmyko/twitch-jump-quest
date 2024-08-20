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
        settingsManager = GetNode<SettingsManager>("/root/SettingsManager");

        teamsVBoxContainer = GetNode<VBoxContainer>("PanelContainer/VBoxContainer");
    }

    public void AddTeamIfNotExists(string teamAbbrev)
    {
        if (!levelManager.teamScores.TeamExists(teamAbbrev))
        {
            if (teamAbbrev == "DEBUG") { return; }
            GD.PushError("team does not exist in levelmanager, problem if debug player is not spawned");
            return;
        }
        if (!teamPositionDictionary.ContainsKey(teamAbbrev))
        {
            TeamScore teamScoreInstance = teamScoreScene.Instantiate<TeamScore>();
            // Instantiate the teamScoreScene
            teamScoreInstance.Name = $"TEAM_{teamAbbrev}";
            // Add the new instance to the VBoxContainer
            teamsVBoxContainer.AddChild(teamScoreInstance);
            // Set up properties for the new TeamScore instance
            teamScoreInstance.Initialise(teamAbbrev);

            // Store the position of the new team in the dictionary
            teamPositionDictionary[teamAbbrev] = teamPositionDictionary.Count;
        }
        TeamScore teamRichTextLabel = teamsVBoxContainer.GetNode($"TEAM_{teamAbbrev}") as TeamScore;
        teamRichTextLabel.Text = $"{teamAbbrev}\t[{levelManager.teamScores.GetTeamPlayerCount(teamAbbrev)}] -\t00000";
    }

    public void UpdateTeamScore(string teamAbbrev, int teamScore)
    {
        TeamScore teamRichTextLabel = teamsVBoxContainer.GetNode($"TEAM_{teamAbbrev}") as TeamScore;
        teamRichTextLabel.Text = $"{teamAbbrev}\t[{levelManager.teamScores.GetTeamPlayerCount(teamAbbrev)}] -\t{teamScore:D5}";
        teamRichTextLabel.teamScore = teamScore;
    
        var teamScores = teamsVBoxContainer.GetChildren()
            .Cast<TeamScore>()
            .OrderByDescending(team => team.teamScore)
            .ToList();

        for (int i = 0; i < teamScores.Count; i++)
        {
            teamsVBoxContainer.MoveChild(teamScores[i], i);
        }

        // Debug
        // foreach (var team in teamScores)
        // {
        //     GD.Print($"{team.Name}: {team.teamScore}");
        // }
    }
}
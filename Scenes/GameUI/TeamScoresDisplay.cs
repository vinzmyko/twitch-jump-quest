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
        if (levelManager == null)
        {
            GD.PrintErr("TeamScoresDisplay: levelManager is null");
            return;
        }
        if (levelManager.teamScores == null)
        {
            GD.PrintErr("TeamScoresDisplay: levelManager.teamScores is null");
            return;
        }
        
        var allTeams = levelManager.teamScores.GetAllTeamScores().Select(ts => ts.TeamInfo.TeamAbbreviation).ToList();
        GD.Print($"TeamScoresDisplay: All teams in LevelManager: {string.Join(", ", allTeams)}");
        
        bool teamExists = levelManager.teamScores.TeamExists(teamAbbrev);
        GD.Print($"TeamScoresDisplay: TeamExists method returns {teamExists} for team {teamAbbrev}");

        if (!teamExists)
        {
            GD.PrintErr($"TeamScoresDisplay: Team {teamAbbrev} does not exist in LevelManager");
            // You might want to add logic here to handle this case, such as creating the team
        }
        else
        {
            GD.Print($"TeamScoresDisplay: Team {teamAbbrev} exists in LevelManager");
        }
        if (!levelManager.teamScores.TeamExists(teamAbbrev))
        {
            if (teamAbbrev == "DEBUG") { return; }
            GD.PushError("team does not exist in levelmanager, problem if debug player is not spawned");
            return;
        }
        if (!teamPositionDictionary.ContainsKey(teamAbbrev))
        {
            TeamScore teamScoreInstance = teamScoreScene.Instantiate<TeamScore>();
            teamScoreInstance.Name = $"TEAM_{teamAbbrev}";
            teamsVBoxContainer.AddChild(teamScoreInstance);
            teamScoreInstance.Initialise(teamAbbrev);

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
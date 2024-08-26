using Godot;
using System;
using System.Collections.Generic;
using System.Linq;
using UNLTeamJumpQuest.TwitchIntegration;

public partial class EndGameScreen : Control
{
    private List<PlayerInfo> stats;
    [Export]
    private ShowTeamWinner showTeamsWinner;
    [Export]
    private ShowMVP showMVP;
    [Export]
    private ShowHallOfFame showHOF;
    private SettingsManager settingsManager;
    private LevelManager levelManager;
    private AnimationPlayer animPlayer;

    UNL.TeamScore teamThatWon;
    PlayerInfo MVP;
    GameManager gameManager;

    public override void _Ready()
    {
        base._Ready();

        animPlayer = GetNode<AnimationPlayer>("AnimationPlayer");

        // Access AutoLoad scripts
        settingsManager = GetNode<SettingsManager>("/root/SettingsManager");
        levelManager = GetNode<LevelManager>("/root/LevelManager");
        gameManager = GetNode<GameManager>("/root/GameManager");
        if (gameManager != null)
        {
            stats = gameManager.playerStatsInfo;
            if (stats == null || stats.Count == 0)
            {
                GD.PrintErr("No player stats available in EndGameScreen!");
            }
        }
        else
        {
            GD.PrintErr("GameManager not found in EndGameScreen!");
        }

        stats = gameManager.playerStatsInfo;

        teamThatWon = WinningTeam();

        MVP = FindMVP(stats);
        SetShowTeamWinnerNodes();
        SetShowMVPNodes();
        SetShowHallOfFameNodes();

        animPlayer.Play("EndGameScreen");
        animPlayer.AnimationFinished += OnAnimationFinished;
    }

    private void OnAnimationFinished(StringName animName)
    {
        // gameManager.ResetPlayers();
        levelManager.teamScores.ClearTeams();
        gameManager.StartNewGame();
        
        CallDeferred(nameof(ChangeToMainMenu));
    }

    private void ChangeToMainMenu()
    {
        SceneManager.Instance.ChangeScene("MainMenu");
    }


    private UNL.TeamScore WinningTeam()
    {
        var list = levelManager.teamScores.GetAllTeamScores().ToList();
        return list.MaxBy(entry => entry.TotalScore);
    }

    private UNL.TeamScore DummyDataWinningTeam()
    {
        UNL.TeamScoreManager scores = new UNL.TeamScoreManager();
        scores.AddTeam(settingsManager.UNLTeams.Teams[1]);
        scores.AddScoreToTeam(settingsManager.UNLTeams.Teams[1].TeamAbbreviation, 1000);
        scores.AddTeam(settingsManager.UNLTeams.Teams[0]);
        scores.AddScoreToTeam(settingsManager.UNLTeams.Teams[0].TeamAbbreviation, 500);
        var list = scores.GetAllTeamScores().ToList();
        return list.MaxBy(entry => entry.TotalScore);
    }

    private PlayerInfo FindMVP(List<PlayerInfo> allPlayers)
    {
        return allPlayers.MaxBy(p => p.points);
    }

    private PlayerInfo HighestDistanceTravelled(List<PlayerInfo> allPlayers)
    {
        return allPlayers.MaxBy(p => p.HighestYPos);
    }

    private PlayerInfo LowestDistanceTravelled(List<PlayerInfo> allPlayers)
    {
        return allPlayers.Where(p => p.HighestYPos > 0).MinBy(p => p.HighestYPos);
    }

    private PlayerInfo LowestPoints(List<PlayerInfo> allPlayers)
    {
        return allPlayers.Where(p => p.points > 0).MinBy(p => p.points);
    }

    private PlayerInfo HighestCombo(List<PlayerInfo> allPlayers)
    {
        return allPlayers.MaxBy(p => p.ComboStreak);
    }

    private PlayerInfo NumOfFaceplants(List<PlayerInfo> allPlayers)
    {
        return allPlayers.MaxBy(p => p.NumOfFaceplants);
    }
    private PlayerInfo FaceplantDistance(List<PlayerInfo> allPlayers)
    {
        return allPlayers.MaxBy(p => p.DistanceOfFurthestFaceplant);
    }

    private void generateDebugInformation()
    {
        stats = new List<PlayerInfo>();
        Player one = generatePlayer("steve", settingsManager.UNLTeams.Teams[0], 213, 524, 0, 0, 0, "3453");
        Player two = generatePlayer("jeff", settingsManager.UNLTeams.Teams[1], 12, 0, 0, 0, 0,"3453");
        Player three = generatePlayer("fadsjo09", settingsManager.UNLTeams.Teams[1], 2, 524, 5, 2, 1000,"3453");
        Player four = generatePlayer("blue", settingsManager.UNLTeams.Teams[1], 20, 3, 5, 2, 5,"3453");
        Player five = generatePlayer("tiger", settingsManager.UNLTeams.Teams[2], 40, 12, 5, 1, 510,"3453");
        Player six = generatePlayer("book", settingsManager.UNLTeams.Teams[1], 523, 345, 5, 6, 532,"3453");
        Player seven = generatePlayer("lol", settingsManager.UNLTeams.Teams[0], 90, 524, 5, 5, 3215,"235");
        Player eight = generatePlayer("mvp", settingsManager.UNLTeams.Teams[2], 2000, 1234, 20, 2, 1000,"23523");
        stats.Add(new PlayerInfo(one));
        stats.Add(new PlayerInfo(two));
        stats.Add(new PlayerInfo(three));
        stats.Add(new PlayerInfo(four));
        stats.Add(new PlayerInfo(five));
        stats.Add(new PlayerInfo(six));
        stats.Add(new PlayerInfo(seven));
        stats.Add(new PlayerInfo(eight));
    }

    private Player generatePlayer(string displayName, UNL.Team team, int points, float highestYpos, int comboStreak, int numOfFaceplants, int distanceOfFurthestFaceplant, string userId)
    {
        Player p = new Player();
        p.userID = userId;
        p.displayName = displayName;
        p.team = team;
        p.points = points;
        p.highestYPosition = highestYpos;
        p.comboStreak = comboStreak;
        p.numOfFaceplants = numOfFaceplants;
        p.distanceOfFurthestFaceplant = distanceOfFurthestFaceplant;

        return p;
    }
}

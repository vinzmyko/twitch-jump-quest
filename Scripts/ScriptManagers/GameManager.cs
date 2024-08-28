using Godot;
using System;
using System.Collections.Generic;
using TwitchLib.Api.Helix.Models.Games;
using UNLTeamJumpQuest.TwitchIntegration;

public partial class GameManager : Node
{
    [Signal]
    public delegate void PlayerJoinedEventHandler(string displayName, string userID, string teamAbbrev);
    [Signal]
    public delegate void PlayerDiedEventHandler(string displayName, string userID, string teamAbbrev);
    [Signal]
    public delegate void GameStateChangedEventHandler(int gameState);
    
    // I didn't know I made the instance but I've been refference this via GetNode<>()
    public static GameManager Instance { get; private set; }
    public List<Player> players = new List<Player>();
    public enum GameState
    {
        WaitingForPlayers,
        Playing,
        GameOver
    }
    public GameState CurrentGameState { get; private set; }
    public float gameTime = 1000.0f; //In seconds
    public float waitTime = 90.0f;
    public GameTimer gameTimer;
    public List<PlayerInfo> playerStatsInfo = new List<PlayerInfo>();
    public int totalPlayers;
    public bool easyMode = false;
    public bool autoPlay = false;
    public bool quitNextRound = false;
    public bool tillEveryoneDies = false;

    public void ResetPlayers()
    {
        foreach (var player in players)
        {
            if (IsInstanceValid(player))
            {
                player.QueueFree();
            }
        }
        playerStatsInfo.Clear();
        players.Clear();
        CurrentGameState = GameState.WaitingForPlayers;
    }

    public void StartNewGame()
    {
        ResetPlayers();
        CurrentGameState = GameState.WaitingForPlayers;
        gameTimer.waitTimeFinishedEmitted = false;
        gameTimer.gameTimeFinishedEmitted = false;
    }
    public void AddToStatTrackingList(Player player)
    {
        if (playerStatsInfo.Contains(new PlayerInfo(player)))
            return;
        playerStatsInfo.Add(new PlayerInfo(player));
        GD.Print($"Added player {player.displayName} to stat tracking list. Total players: {playerStatsInfo.Count}");
    }

    public override void _Ready()
    {
        if (Instance == null)
        {
            Instance = this;
        }
        else
        {
            QueueFree();
            return;
        }

        // Initialize game state
        CurrentGameState = GameState.WaitingForPlayers;

        GetNode<LevelManager>("/root/LevelManager").PlayerSpawned += OnPlayerSpawned;
        GetNode<SceneManager>("/root/SceneManager").LevelReady += InitLevelDependecies;

    }

    private void InitLevelDependecies(string levelName)
    {
        var root = GetTree().Root;
        var levelNode = root.GetChild(root.GetChildCount() - 1);
        gameTimer = levelNode.GetNode<GameTimer>("CanvasLayer/GameTimer");
        gameTimer.waitTimeFinished += () => {CurrentGameState = GameState.Playing;};
        gameTimer.gameTimeFinished += EndGame;
        levelNode.GetNode<LevelCamera>("LevelCamera").PlayerHitKillZone += (Player player) =>
        {
            AddToStatTrackingList(player);
            player.QueueFree();
            UpdateTotalPlayers();
            totalPlayers--;

            if (tillEveryoneDies)
            {
                GD.Print($"tillEveryoneDies: True, TotalPlayers{totalPlayers}");
                if (totalPlayers <= 0)
                {
                    EndGame();
                }
            }
            else
            {
                if (totalPlayers <= 1)
                {
                    EndGame();
                }
            }
        };
    }

    public int UpdateTotalPlayers()
    {
        totalPlayers = GetTree().GetNodesInGroup("Player").Count;
        return totalPlayers;
    }

    public void EndGame()
    {
        if (CurrentGameState == GameState.GameOver)
        {
            return; // Game is already over, no need to end it again
        }

        CurrentGameState = GameState.GameOver;
        
        // Collect data for all remaining players
        var remainingPlayers = GetTree().GetNodesInGroup("Player");
        foreach (Player player in remainingPlayers)
        {
            AddToStatTrackingList(player);
        }

        // Ensure playerStatsInfo is not empty
        if (playerStatsInfo.Count == 0)
        {
            GD.PrintErr("No player stats collected at end of game!");
        }

        EmitSignal(SignalName.GameStateChanged, (int)CurrentGameState);
    }

    private void OnPlayerSpawned(Player player)
    {
        player.Died += OnPlayerDied;
    }

    private void OnPlayerDied(string displayName, string userID, string teamAbbrev)
    {
        EmitSignal(SignalName.PlayerDied, displayName, userID, teamAbbrev);
    }

    public void HandleJoinRequest(string[] messageInfo, UNL.Team team)
    {
        if (CurrentGameState != GameState.WaitingForPlayers && team != null)
        {
            GD.Print("No in waiting for players state");
            return;
        }
        UNL.Team targetTeam = team;

        if (targetTeam == null)
        {
            UNL.Team.AvatarColours defaultColors = new UNL.Team.AvatarColours
            {
                CapeMain = Color.FromHtml("#0a7030"),
                CapeTrim = Color.FromHtml("#eba724"),
                ArmourLight = Color.FromHtml("#eadfd1"),
                ArmourMedium = Color.FromHtml("#b3aaa1"),
                ArmourDark = Color.FromHtml("#7c776f")
            };
            targetTeam = new UNL.Team
            {
                TeamName = "DEBUG",
                TeamAbbreviation = "DEBUG",
                LogoPath = "res://icon.svg",
                TeamColours = defaultColors
            };
        }
        string displayName = messageInfo[0];
        string userID = messageInfo[1];

        GD.Print($"{messageInfo[0]} has requested to join the lobby!");
        //Checks if joining is allowed and if player doesn't already exist
        if (CurrentGameState == GameState.WaitingForPlayers || team == null)
        {
            if (!PlayerExists(userID))
            {
                Player newPlayer = new Player();
                newPlayer.Initialize(displayName, userID, team);
                players.Add(newPlayer);
                CallDeferred(nameof(EmitPlayerHasJoined), displayName, userID, targetTeam.TeamAbbreviation);
                GD.Print($"{displayName} (ID: {userID} has joined the game.)");
            }
            else
                GD.Print($"{displayName} (ID: {userID}) is already in the game.");
        }
        else
            GD.Print($"Cannot join game. Current State: {CurrentGameState}");
    }

    public void DeletePlayer(string userID)
    {
        players.RemoveAll(player => player.userID == "DEBUG");
        var debugPlayerArray = GetTree().GetNodesInGroup("DebugPlayer");

        foreach (var debugPlayer in debugPlayerArray)
        {
            debugPlayer.QueueFree();
        }
    }

    public void ClearPlayerLobby()
    {
        players.Clear();
        var playerGroup = GetTree().GetNodesInGroup("Player");

        foreach (var player in playerGroup)
        {
            player.QueueFree();
        }
    }

    private void EmitPlayerHasJoined(string displayName, string userID, string teamAbbrev)
    {
        EmitSignal(SignalName.PlayerJoined, displayName, userID, teamAbbrev);
    }

    private bool PlayerExists(string userID)
    {
        foreach (var player in players)
        {
            if (player.GetUserID() == userID)
                return true;
        }
        return false;
    }

    // Method to change game state
    public void SetGameState(GameState newState)
    {
        CurrentGameState = newState;
        EmitSignal(nameof(GameStateChanged), (int)newState);
        GD.Print($"Game state changed to {newState}");
    }
}

using Godot;
using System;
using System.Collections.Generic;

public partial class GameManager : Node
{
    [Signal]
    public delegate void PlayerJoinedEventHandler(string displayName, string userID, string teamAbbrev);
    [Signal]
    public delegate void PlayerDiedEventHandler(string displayName, string userID, string teamAbbrev);
    

    // Singleton instance
    public static GameManager Instance { get; private set; }
    // List to store all active players
    private List<Player> players = new List<Player>();

    private DebugTwitchChat debugger;

    // Game state
    public enum GameState
    {
        WaitingForPlayers,
        Playing,
        GameOver
    }
    public GameState CurrentGameState { get; private set; }

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

        debugger = GetNodeOrNull<DebugTwitchChat>("/root/Main/CanvasLayer/DebugTwitchChat");
        GetNode<LevelManager>("/root/LevelManager").PlayerSpawned += OnPlayerSpawned;
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
        if (CurrentGameState == GameState.WaitingForPlayers)
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

    // Method to add a new player

    public void AddPlayer(string twitchUsername)
    {
        // TODO: Implement player creation and addition logic
    }

    // Method to update player scores and distances
    public void UpdatePlayerProgress(string twitchUsername, float distance)
    {
        // TODO: Implement player progress update logic
    }

    // Method to change game state
    public void SetGameState(GameState newState)
    {
        // TODO: Implement game state change logic
    }

    // Method to get all player scores for UI display
    public Dictionary<string, float> GetPlayerScores()
    {
        // TODO: Implement method to return all player scores
        return new Dictionary<string, float>();
    }
}

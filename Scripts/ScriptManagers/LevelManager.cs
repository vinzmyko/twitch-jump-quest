using Godot;
using System;

public partial class LevelManager : Node
{
    Marker2D spawnPosition;

    private PackedScene playerScene;
    // Current level information
    private int currentLevel;
    public override void _Ready()
    {
        base._Ready();
        playerScene = ResourceLoader.Load<PackedScene>("res://Scenes/Player.tscn");
        GameManager.Instance.PlayerJoined += SpawnPlayer;

        spawnPosition = GetNodeOrNull<Marker2D>("/root/Main/SpawnMarker2D");
    }

    // Method to load a specific level
    public void LoadLevel(int levelNumber)
    {
        // TODO: Implement level loading logic
    }

    // Method to spawn a player in the current level
    public void SpawnPlayer(string displayName, string userID)
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

        playerInstance.Initialize(displayName, userID);
        playerInstance.Name = $"Player_{userID}";
        
        if (spawnPosition == null)
        {
            GD.PushError("spawnPosition not found");
        }
        else
            playerInstance.GlobalPosition = spawnPosition.GlobalPosition;

        // Add player to Level owner scene
        Node levelScene = GetNode<Node>("/root/Main");
        levelScene.AddChild(playerInstance);

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

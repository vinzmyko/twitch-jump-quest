using Godot;
using System;
using System.Collections.Generic;

public partial class SceneManager : Node
{
    // Dictionary to store scene paths
    private Dictionary<string, string> scenePaths = new Dictionary<string, string>();

    public override void _Ready()
    {
        // Initialize scene paths
        scenePaths.Add("MainMenu", "res://Scenes/MainMenu.tscn");
        scenePaths.Add("Game", "res://Scenes/Game.tscn");
        scenePaths.Add("Settings", "res://Scenes/Settings.tscn");
    }

    // Method to change the current scene
    public void ChangeScene(string sceneName)
    {
        if (scenePaths.TryGetValue(sceneName, out string path))
        {
            GetTree().ChangeSceneToFile(path);
        }
        else
        {
            GD.PushError($"Scene '{sceneName}' not found.");
        }
    }
}

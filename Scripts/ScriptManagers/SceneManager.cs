using Godot;
using System.Collections.Generic;

public partial class SceneManager : Node
{
    public static SceneManager Instance { get; private set; }
    private Dictionary<string, string> scenePaths = new Dictionary<string, string>();

    public override void _Ready()
    {
        if (Instance == null)
        {
            Instance = this;
        }
        else
        {
            GD.PushError("More than one SceneManager instance detected. Removing duplicate.");
            QueueFree();
            return;
        }

        scenePaths.Add("MainMenu", "res://Scenes/Menus/MainMenu.tscn");
        scenePaths.Add("Settings", "res://Scenes/Settings.tscn");
        scenePaths.Add("ManageTeams", "res://Scenes/Menus/Settings.tscn");
        scenePaths.Add("EndGameScreen", "res://Scenes/EndGameScreen/EndGameScreen.tscn");
        scenePaths.Add("LevelOne", "");
    }

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
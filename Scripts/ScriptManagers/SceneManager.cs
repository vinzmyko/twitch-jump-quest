using Godot;
using System.Collections.Generic;

public partial class SceneManager : Node
{
    [Signal]
    public delegate void LevelReadyEventHandler(string levelName);
    public static SceneManager Instance { get; private set; }
    private Dictionary<string, string> scenePaths = new Dictionary<string, string>();
    private GameManager gameManager;

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

        gameManager = GetNode<GameManager>("/root/GameManager");

        scenePaths.Add("MainMenu", "res://Scenes/Menus/MainMenu.tscn");
        scenePaths.Add("Settings", "res://Scenes/Menus/Settings.tscn");
        scenePaths.Add("ManageTeams", "res://Scenes/Menus/ManageTeams/ManageTeams.tscn");
        scenePaths.Add("EndGameScreen", "res://Scenes/EndGameScreen/EndGameScreen.tscn");
        scenePaths.Add("SelectLevel", "res://Scenes/Menus/SelectLevel.tscn");
        scenePaths.Add("LevelOne", "res://Levels/LevelOne.tscn");
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

    public void LevelReadySignal(string levelName)
    {
        EmitSignal(SignalName.LevelReady, levelName);
    }
}
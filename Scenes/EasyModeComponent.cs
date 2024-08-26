using System;
using Godot;

public partial class EasyModeComponent : Node
{
    [Export] public bool Enabled { get; set; } = false;

    private LevelCamera levelCamera;

    public override void _Ready()
    {
        levelCamera = GetParent<Node>().GetParent<LevelCamera>();
        if (levelCamera == null)
        {
            GD.PushError("EasyModeComponent must be a child of LevelCamera");
        }
    }

    public bool ShouldTriggerCameraMovement(int activePlayers, int currentPointIndex, int totalPlayerCount)
    {
        if (!Enabled)
        {
            return true; // Always trigger in normal mode
        }
        return activePlayers >= CalculatePlayerThreshold(totalPlayerCount, currentPointIndex);
    }

    public int CalculatePlayerThreshold(int totalPlayerCount, int currentPointIndex)
    {
        int difficulty = currentPointIndex / 2;
        // GD.Print($"difficulty = {currentPointIndex} / 2 = {difficulty}");
        // int requiredPlayers = ((totalPlayerCount + 1) / 2) - difficulty;
        int requiredPlayers = (int)Math.Ceiling((double)totalPlayerCount / 2);
        int playerThreshold = Mathf.Max(1, requiredPlayers);
        // GD.Print("\n\t");
        // GD.Print($"PlayerThreshold = requiredPlayers - difficulty, {requiredPlayers} = {requiredPlayers} - {difficulty}");
        return playerThreshold;
    }

    public void ToggleEasyMode(bool enabled)
    {
        Enabled = enabled;
        GD.Print($"Easy Mode: {(Enabled ? "Enabled" : "Disabled")}");
    }
}
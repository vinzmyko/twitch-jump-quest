using Godot;
using System;

public partial class TestLevelOne : Node2D
{
    public override void _Ready()
    {
        base._Ready();
        SceneManager.Instance.LevelReadySignal("LevelOne");
    }
}

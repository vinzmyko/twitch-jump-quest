using Godot;
using System;

public partial class TestCamera : Camera2D
{
    Player JoinedPlayer;
    public override void _Ready()
    {
        base._Ready();
        GameManager.Instance.PlayerJoined += MoveCameraToPlayer;
    }

    private void MoveCameraToPlayer(string displayName, string userID)
    {
        JoinedPlayer = GetTree().CurrentScene.GetNode<Player>("/root/Main/Player_DEBUG");
    }

    public override void _PhysicsProcess(double delta)
    {
        base._PhysicsProcess(delta);
        if (JoinedPlayer == null)
        {
            return;
        }
        else
        {
            GlobalPosition = JoinedPlayer.GlobalPosition;
        }
    }

}

using Godot;
using System;
using System.Diagnostics;

public partial class UpwardsCamera : Camera2D
{
    [Export]
    public float cameraSpeed = 1.75f;
    private Marker2D startingMarker;
    private Marker2D endingMarker;
    private DebugTwitchChat debugger;

    enum State
    {
        PAUSE,
        UP,
        DOWN
    }
    private State cameraState = State.PAUSE;

    public override void _Ready()
    {
        base._Ready();
        startingMarker = GetParent().GetNode<Marker2D>("LevelMarkers/StartingMarker");
        endingMarker = GetParent().GetNode<Marker2D>("LevelMarkers/EndingMarker");
        debugger = GetParent().GetNode<CanvasLayer>("CanvasLayer").GetNode<DebugTwitchChat>("DebugTwitchChat");
        debugger.ActivateCamera += OnReceivedDebugInstructions;
    }

    private void OnReceivedDebugInstructions(string text)
    {
        switch (text)
        {
            case "go":
                cameraState = State.UP;
                break;
            case "pause":
                cameraState = State.PAUSE;
                break;
            case "down":
                cameraState = State.DOWN;
                break;
        }
    }


    public override void _PhysicsProcess(double delta)
    {
        base._PhysicsProcess(delta);

        Vector2 newPosition = GlobalPosition;

        switch (cameraState)
        {
            case State.UP:
                newPosition.Y -= cameraSpeed * (float)delta;
                newPosition.Y = Mathf.Max(newPosition.Y, endingMarker.GlobalPosition.Y);
                break;
            case State.DOWN:
                newPosition.Y += cameraSpeed * (float)delta;
                newPosition.Y = Mathf.Min(newPosition.Y, startingMarker.GlobalPosition.Y);
                break;
            case State.PAUSE:
                break;
        }
        GlobalPosition = newPosition;
    }
}

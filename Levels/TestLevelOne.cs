using Godot;

public partial class TestLevelOne : Node2D
{
    [Export]
    private PressedEscape pressedEscape;
    public override void _Ready()
    {
        base._Ready();
        SceneManager.Instance.LevelReadySignal("LevelOne");
    }

    public override void _Input(InputEvent @event)
    {
        if (@event.IsActionPressed("Escape"))
        {
            pressedEscape.ToggleEscapeScreen();
        }
    }

}

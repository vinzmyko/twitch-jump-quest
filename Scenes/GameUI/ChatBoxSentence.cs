using Godot;

public partial class ChatBoxSentence : RichTextLabel
{
    float timerTime = 5.0f;
    public override async void _Ready()
    {
        base._Ready();
        await ToSignal(GetTree().CreateTimer(5.0f), SceneTreeTimer.SignalName.Timeout);
        QueueFree();
    }
}

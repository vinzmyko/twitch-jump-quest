using Godot;

public partial class EndGameAnimationToEndGameScreen : Control
{
    private AnimationPlayer animationPlayer;

    [Signal]
    public delegate void AnimationFinishedEventHandler();

    public override void _Ready()
    {
        base._Ready();
        animationPlayer = GetNode<AnimationPlayer>("AnimationPlayer");
        animationPlayer.AnimationFinished += OnAnimationFinished;
    }

    public void PlayAnimation()
    {
        animationPlayer.Play("GG!");
    }

    private void OnAnimationFinished(StringName animName)
    {
        EmitSignal(nameof(AnimationFinished));
    }

    public void Cleanup()
    {
        if (IsInstanceValid(this))
        {
            // Disconnect the signal to prevent any lingering connections
            animationPlayer.AnimationFinished -= OnAnimationFinished;
            QueueFree();
        }
    }
}
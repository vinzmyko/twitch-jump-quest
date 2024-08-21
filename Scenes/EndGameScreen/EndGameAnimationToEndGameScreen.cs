using Godot;

public partial class EndGameAnimationToEndGameScreen : Control
{
    private AnimationPlayer animationPlayer;

    public override void _Ready()
    {
        base._Ready();
        animationPlayer = GetNode<AnimationPlayer>("AnimationPlayer");
        animationPlayer.AnimationFinished += (StringName animName) => {SceneManager.Instance.ChangeScene("EndGameScreen");};
        animationPlayer.Play("GG!");
    }
}

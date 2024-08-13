using System.IO;
using Godot;

public partial class FloatingMessage : Control
{
    public Label messageLabel;
    private AnimationPlayer animationPlayer;
    private AudioStreamPlayer audioStreamPlayer;
    private AudioStreamMP3 success;
    private AudioStreamMP3 unsuccessful;
    private Color greenColour = Color.FromHtml("#ccffcc");
    private Color redColour = Color.FromHtml("#ffcccc");
    private string messageText = string.Empty;

    public override void _Ready()
    {
        base._Ready();

        messageLabel = GetNode<Label>("Label");
        animationPlayer = GetNode<AnimationPlayer>("AnimationPlayer");
        audioStreamPlayer = GetNode<AudioStreamPlayer>("AudioStreamPlayer");

        success = ResourceLoader.Load<AudioStreamMP3>("res://Audio/UI/SuccessUI.mp3");
        unsuccessful = ResourceLoader.Load<AudioStreamMP3>("res://Audio/UI/NotSuccessUI.mp3");

        CallDeferred(nameof(SetLabelPositionAndPivot));

        animationPlayer.Play("PopUp");
    }

    private void SetLabelPositionAndPivot()
    {
        messageLabel.PivotOffset = new Vector2(messageLabel.Size.X / 2, messageLabel.Size.Y /2);
        messageLabel.SetPosition(new Vector2(getXSpawnPosition(), 720));
    }

    private float getXSpawnPosition()
    {
        float screenSizeX = (float)ProjectSettings.GetSetting("display/window/size/viewport_width") / 2;
        float labelSizeX = messageLabel.Size.X / 2;
        return screenSizeX - labelSizeX;
    }

    public void displaySuccessful(string text)
    {
        messageLabel.Modulate = greenColour;
        audioStreamPlayer.Stream = success;
        messageLabel.Text = text;
    }
    public void displayUnsuccessful(string text)
    {
        messageLabel.Modulate = redColour;
        audioStreamPlayer.Stream = unsuccessful;
        messageLabel.Text = text;
    }

}
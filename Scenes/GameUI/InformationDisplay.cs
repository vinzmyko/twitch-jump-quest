using Godot;

public partial class InformationDisplay : Control
{
    [Export]
    private PackedScene sentence;
    private VBoxContainer sentenceContainer;

    public override void _Ready()
    {
        base._Ready();

        sentenceContainer = GetNode<VBoxContainer>("MarginContainer/PanelContainer/MarginContainer/SentenceContainer");
    }

    public void AddJoinMessage(string playerName, string teamAbbrev)
    {
        ChatBoxSentence newSentence = sentence.Instantiate<ChatBoxSentence>();
        
        if (newSentence == null)
        {
            GD.PushError("Failed to instantiate sentence scene");
            return;
        }

        newSentence.Name = $"{playerName}_{teamAbbrev}_Sentence";
        newSentence.Text = $"{playerName} has joined {teamAbbrev}";
        
        sentenceContainer.AddChild(newSentence);
    }
}
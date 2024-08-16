using System;
using Godot;

public partial class InformationDisplay : Control
{
    [Export]
    private PackedScene sentence;
    private VBoxContainer sentenceContainer;
    private Random random;
    private string[] playerJoinMessages = new string[]
        {
            "{0} has joined the {1} roster!",
            "Welcome aboard, {0}! You're now part of {1}.",
            "{1} just got stronger with {0} joining the squad!",
            "Breaking news: {0} signs with {1}!",
            "{0} dons the {1} gear!",
            "{1} fans, give a warm welcome to your newest teammate, {0}!",
            "{0} is ready to make waves with {1}.",
            "The {1} family grows as {0} comes on board.",
            "Look out! {0} is now playing for {1}.",
            "{1} just leveled up with {0}!"
        };

    public override void _Ready()
    {
        base._Ready();

        sentenceContainer = GetNode<VBoxContainer>("MarginContainer/PanelContainer/MarginContainer/SentenceContainer");
        random = new Random();
    }

    public void AddJoinMessage(string playerName, string teamAbbrev)
    {
        if (sentenceContainer.GetChildCount() == 5)
        {
            sentenceContainer.GetChild(0).QueueFree();
        }
        ChatBoxSentence newSentence = sentence.Instantiate<ChatBoxSentence>();
        
        if (newSentence == null)
        {
            GD.PushError("Failed to instantiate sentence scene");
            return;
        }

        newSentence.Name = $"{playerName}_{teamAbbrev}_Sentence";
        string template = playerJoinMessages[random.Next(playerJoinMessages.Length)];
        newSentence.Text = string.Format(template, playerName, teamAbbrev);

                
        sentenceContainer.AddChild(newSentence);
    }
}
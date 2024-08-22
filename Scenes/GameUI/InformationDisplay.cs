using System;
using Godot;

public partial class InformationDisplay : Control
{
    [Export]
    private PackedScene sentence;
    private VBoxContainer sentenceContainer;
    private Random random;
    private SettingsManager settingsManager;

    public override void _Ready()
    {
        base._Ready();
        sentenceContainer = GetNode<VBoxContainer>("MarginContainer/PanelContainer/MarginContainer/SentenceContainer");
        random = new Random();
        settingsManager = GetNode<SettingsManager>("/root/SettingsManager");
    }

    public void SetSentenceVisibleSize(int size)
    {
        if (sentenceContainer.GetChildCount() >= size)
        {
            sentenceContainer.GetChild(0).QueueFree();
        }
    }

    public void AddJoinMessage(string playerName, string teamAbbrev)
    {
        SetSentenceVisibleSize(5);
        ChatBoxSentence newSentence = sentence.Instantiate<ChatBoxSentence>();
        
        if (newSentence == null)
        {
            GD.PushError("Failed to instantiate sentence scene");
            return;
        }

        newSentence.Name = $"{playerName}_{teamAbbrev}_Sentence";
        newSentence.Text = FormatJoinMessage(playerName, teamAbbrev);
                
        sentenceContainer.AddChild(newSentence);
    }

    public void AddDiedMessage(string playerName, string teamAbbrev)
    {
        SetSentenceVisibleSize(5);
        ChatBoxSentence newSentence = sentence.Instantiate<ChatBoxSentence>();
        
        if (newSentence == null)
        {
            GD.PushError("Failed to instantiate sentence scene");
            return;
        }

        newSentence.Name = $"{playerName}_{teamAbbrev}_Died_Sentence";
        newSentence.Text = FormatDeathMessage(playerName, teamAbbrev);

        sentenceContainer.AddChild(newSentence);
    }

    public void AddFaceplantMessage(Player player, float distance)
    {
        SetSentenceVisibleSize(5);
        ChatBoxSentence newSentence = sentence.Instantiate<ChatBoxSentence>();
        
        if (newSentence == null)
        {
            GD.PushError("Failed to instantiate sentence scene");
            return;
        }

        int convertedDistance = (int)distance / 16;
        newSentence.Name = $"{player.displayName}_{player.team.TeamAbbreviation}_faceplant_Sentence";
        newSentence.Text = FormatFaceplantMessage(player.displayName, player.team.TeamAbbreviation, convertedDistance);

        sentenceContainer.AddChild(newSentence);
    }

    public void AddComboStreakMessage(Player player, int comboStreak)
    {
        SetSentenceVisibleSize(5);
        ChatBoxSentence newSentence = sentence.Instantiate<ChatBoxSentence>();
        
        if (newSentence == null)
        {
            GD.PushError("Failed to instantiate sentence scene");
            return;
        }

        newSentence.Name = $"{player.displayName}_{player.team.TeamAbbreviation}_combostreak_Sentence";
        newSentence.Text = FormatComboStreakMessage(player.displayName, player.team.TeamAbbreviation, comboStreak);

        sentenceContainer.AddChild(newSentence);
    }
}
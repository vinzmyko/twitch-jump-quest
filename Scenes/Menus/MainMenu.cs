using Godot;
using System;
using UNLTeamJumpQuest.TwitchIntegration;

public partial class MainMenu : Control
{
    [Export]
    private Button selectLevel;
    [Export]
    private Button settings;
    [Export]
    private Button manageTeams;
    [Export]
    private Button exit;

    public override void _Ready()
    {
        base._Ready();
        
        TwitchBot.Instance.TwitchClientSuccessfullyConnected += () => 
        {
            selectLevel.Disabled = false;
        };

        selectLevel.ButtonDown += OnSelectLevelDown;
        settings.ButtonDown += OnSettingsDown;
        manageTeams.ButtonDown += OnManageTeamsDown;
        exit.ButtonDown += OnExitDown;

        if (TwitchBot.Instance.botConnected == true)
            selectLevel.Disabled = false;
    }

    private void OnExitDown()
    {
        throw new NotImplementedException();
    }

    private void OnManageTeamsDown()
    {
        SceneManager.Instance.ChangeScene("ManageTeams");
    }

    private void OnSettingsDown()
    {
        SceneManager.Instance.ChangeScene("Settings");
    }

    private void OnSelectLevelDown()
    {
        SceneManager.Instance.ChangeScene("SelectLevel");
    }

}

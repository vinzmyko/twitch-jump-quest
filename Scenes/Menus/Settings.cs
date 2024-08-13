using Godot;
using UNLTeamJumpQuest.TwitchIntegration;

public partial class Settings : Control
{
    private LineEdit twitchUserNameLineEdit;
    private LineEdit twitchAccessTokenLineEdit;
    private Button showHideButton;
    private Button connectButton;
    private HSlider masterSlider;
    private HSlider musicSlider;
    private HSlider sfxSlider;
    private AudioStreamPlayer musicStreamPlayer;
    private AudioStreamPlayer sfxStreamPlayer;
    private SettingsManager settingsManager;

    public override void _Ready()
    {
        base._Ready();

        twitchUserNameLineEdit = GetNode<LineEdit>("VBoxContainer/HBoxContainer/TwitchUsernameLineEdit");
        twitchAccessTokenLineEdit = GetNode<LineEdit>("VBoxContainer/VBoxContainer/HBoxContainer/TwitchAccessTokenLineEdit");
        showHideButton = GetNode<Button>("VBoxContainer/VBoxContainer/HBoxContainer/ShowHideButton");
        connectButton = GetNode<Button>("VBoxContainer/MarginContainer/Connect");
        masterSlider = GetNode<HSlider>("VBoxContainer/HBoxContainer4/MasterHSlider");
        musicSlider = GetNode<HSlider>("VBoxContainer/HBoxContainer3/MusicHSlider");
        sfxSlider = GetNode<HSlider>("VBoxContainer/HBoxContainer2/SFXHSlider");
        musicStreamPlayer = GetNode<AudioStreamPlayer>("MusicStreamPlayer");
        sfxStreamPlayer = GetNode<AudioStreamPlayer>("SFXMusicPlayer");

        settingsManager = GetNode<SettingsManager>("/root/SettingsManager");

        Texture2D open = ResourceLoader.Load<Texture2D>("res://Sprites/ButtonIcons/eye.png");
        Texture2D closed = ResourceLoader.Load<Texture2D>("res://Sprites/ButtonIcons/hidden.png");

        twitchUserNameLineEdit.TextSubmitted += (string submittedText) => {settingsManager.SetTwitchUserName(submittedText);};
        twitchAccessTokenLineEdit.TextSubmitted += (string submittedText) => {settingsManager.SetTwitchAccessToken(submittedText);};
        showHideButton.Pressed += () => 
        {
            twitchAccessTokenLineEdit.Secret = !twitchAccessTokenLineEdit.Secret;

            if (twitchAccessTokenLineEdit.Secret)
                showHideButton.Icon = open;
            else
                showHideButton.Icon = closed;
        };
        connectButton.Pressed += () => 
        {
            settingsManager.SetTwitchUserName(twitchUserNameLineEdit.Text);
            settingsManager.SetTwitchAccessToken(twitchAccessTokenLineEdit.Text);

            TwitchBot.Instance.ConnectFailSafe(false);
        };
        masterSlider.DragEnded += (bool valueChange) => 
        {
            SetAudioBusSlider(valueChange, 0, masterSlider);
        };
        musicSlider.DragEnded += (bool valueChange) => 
        {
            SetAudioBusSlider(valueChange, AudioServer.GetBusIndex("Music"), musicSlider);
            musicStreamPlayer.Play();
        };
        sfxSlider.DragEnded += (bool valueChange) => 
        {
            SetAudioBusSlider(valueChange, AudioServer.GetBusIndex("SFX"), sfxSlider);
            sfxStreamPlayer.Play();
        };

        LoadExistingTwitchInfo();
        LoadExistingVolume();
    }

    private void SetAudioBusSlider(bool valueChange, int busIndex, HSlider hslider)
    {
            if (valueChange)
            {
                AudioServer.SetBusVolumeDb(busIndex, Mathf.LinearToDb((float)hslider.Value));
                AudioServer.SetBusMute(busIndex, hslider.Value < .01f);
            }
            settingsManager.SetBusVolume(busIndex, (float)hslider.Value);
    }

    private void LoadExistingVolume()
    {
        if (settingsManager.GetBusVolume(AudioServer.GetBusIndex("Master")) != null)
        {
            masterSlider.Value = (float)settingsManager.GetBusVolume(AudioServer.GetBusIndex("Master"));
            AudioServer.SetBusVolumeDb(0, Mathf.LinearToDb((float)masterSlider.Value));
            AudioServer.SetBusMute(0, masterSlider.Value < .01f);
        }
        if (settingsManager.GetBusVolume(AudioServer.GetBusIndex("Music")) != null)
        {
            musicSlider.Value = (float)settingsManager.GetBusVolume(AudioServer.GetBusIndex("Music"));
            AudioServer.SetBusVolumeDb(1, Mathf.LinearToDb((float)musicSlider.Value));
            AudioServer.SetBusMute(1, musicSlider.Value < .01f);
        }
        if (settingsManager.GetBusVolume(AudioServer.GetBusIndex("SFX")) != null)
        {
            sfxSlider.Value = (float)settingsManager.GetBusVolume(AudioServer.GetBusIndex("SFX"));
            AudioServer.SetBusVolumeDb(2, Mathf.LinearToDb((float)sfxSlider.Value));
            AudioServer.SetBusMute(2, sfxSlider.Value < .01f);
        }
    }
    private void LoadExistingTwitchInfo()
    {
        if (settingsManager.GetTwitchUserName() != null)
        {
            twitchUserNameLineEdit.Text = settingsManager.GetTwitchUserName();
        }

        if (settingsManager.GetTwitchAccessToken() != null)
        {
            twitchAccessTokenLineEdit.Text = settingsManager.GetTwitchAccessToken();
        }
    }
}

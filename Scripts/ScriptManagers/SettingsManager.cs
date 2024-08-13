using Godot;
using System;
using System.Collections.Generic;
using System.Text;
using TwitchLib.Client;
using UNLTeamJumpQuest.TwitchIntegration;

public partial class SettingsManager : Node
{
    private const string CONFIG_FILE = "user://settings.cfg";
    private const string KEY_FILE = "user://encryption.key";
    private PackedScene floatingMessageScene;

    private ConfigFile config;
    private Crypto crypto;
    private CryptoKey key;

    public override void _Ready()
    {
        base._Ready();
        config = new ConfigFile();
        crypto = new Crypto();
        if (!LoadOrCreateKey())
        {
            GD.PushError("Failed to load or create encryption key. Settings will not be encrypted.");
            return;
        }
        LoadSettings();

        floatingMessageScene = ResourceLoader.Load<PackedScene>("res://Scenes/FloatingMessage.tscn");

        TwitchBot.Instance.TwitchClientSuccessfullyConnected += () => 
        {
                ShowFloatingMessage("Sucessfully Connected!", true);
        };
    }

    public void ShowFloatingMessage(string message, bool success)
    {
        if (floatingMessageScene == null)
        {
            GD.PushError("Invalid FloatingMessageScene path.");
            return;
        }

        var floatingMessageInstance = floatingMessageScene.Instantiate() as FloatingMessage;
        
        GetTree().CurrentScene.AddChild(floatingMessageInstance);

        if (success)
            floatingMessageInstance.displaySuccessful(message);
        else
            floatingMessageInstance.displayUnsuccessful(message);
        
    }

    private bool LoadOrCreateKey()
    {
        key = new CryptoKey();
        if (FileAccess.FileExists(KEY_FILE))
        {
            Error err = key.Load(KEY_FILE);
            if (err != Error.Ok)
            {
                GD.PushError($"Failed to load encryption key: {err}. Generating a new one.");
                return GenerateNewKey();
            }
            return true;
        }
        return GenerateNewKey();
    }

    private void LoadSettings()
    {
        Error err = config.Load(CONFIG_FILE);
        if (err != Error.Ok)
            GD.PushError($"Failed to load settings: {err}");
    }

    private bool GenerateNewKey()
    {
        key = crypto.GenerateRsa(2048);
        Error err = key.Save(KEY_FILE);
        if (err != Error.Ok)
        {
            GD.PushError($"Failed to save new key: {err}");
            return false;
        }
        return true;
    }

    public void SetTwitchAccessToken(string accessToken)
    {
        SetEncryptedValue("Twitch", "AccessToken", accessToken);
    }

    public string GetTwitchAccessToken()
    {
        return GetDecryptedValue("Twitch", "AccessToken");
    }

    public void SetTwitchUserName(string userName)
    {
        SetEncryptedValue("Twitch", "UserName", userName);
    }

    public string GetTwitchUserName()
    {
        return GetDecryptedValue("Twitch", "UserName");
    }

    private void SetEncryptedValue(string section, string keyName, string value)
    {
        try
        {
            if (string.IsNullOrEmpty(value))
            {
                GD.PushWarning($"Attempted to set empty or null value for {section}/{keyName}");
                return;
            }

            byte[] encrypted = crypto.Encrypt(key, Encoding.UTF8.GetBytes(value));
            if (encrypted == null || encrypted.Length == 0)
            {
                GD.PushError($"Encryption failed for {section}/{keyName}: Result is null or empty");
                return;
            }
            config.SetValue(section, keyName, Convert.ToBase64String(encrypted));
            SaveSettings();
        }
        catch (Exception e)
        {
            GD.PushError($"Error in SetEncryptedValue for {section}/{keyName}: {e.Message}");
        }
    }

    private string GetDecryptedValue(string section, string keyName)
    {
        try
        {
            string encryptedBase64 = (string)config.GetValue(section, keyName, "");
            if (string.IsNullOrEmpty(encryptedBase64))
                return "";
            
            byte[] encrypted = Convert.FromBase64String(encryptedBase64);
            byte[] decrypted = crypto.Decrypt(key, encrypted);
            if (decrypted == null || decrypted.Length == 0)
            {
                GD.PushError($"Decryption failed for {section}/{keyName}: Result is null or empty");
                return "";
            }
            return Encoding.UTF8.GetString(decrypted);
        }
        catch (Exception e)
        {
            GD.PushError($"Error in GetDecryptedValue for {section}/{keyName}: {e.Message}");
            return "";
        }
    }
    // Set the volume for a specific bus index
    public void SetBusVolume(int busIndex, float volume)
    {
        config.SetValue("Audio", $"Bus{busIndex}Volume", volume);
        SaveSettings();
    }

    public float? GetBusVolume(int busIndex)
    {
        if (config.HasSectionKey("Audio", $"Bus{busIndex}Volume"))
        {
            return (float)config.GetValue("Audio", $"Bus{busIndex}Volume");
        }
        return null;
    }

    // Save all bus volumes
    public void SaveAllBusVolumes(Dictionary<int, float> busVolumes)
    {
        foreach (var kvp in busVolumes)
        {
            SetBusVolume(kvp.Key, kvp.Value);
        }
    }

    private void SaveSettings()
    {
        Error err = config.Save(CONFIG_FILE);
        if (err != Error.Ok)
            GD.PushError($"Failed to save settings: {err}");
    }
}

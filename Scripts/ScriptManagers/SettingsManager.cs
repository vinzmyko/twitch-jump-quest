using Godot;
using System;
using System.Collections.Generic;
using System.Text;

public partial class SettingsManager : Node
{
    private const string CONFIG_FILE = "user://settings.cfg";
    private const string KEY_FILE = "user://encryption.key";

    private ConfigFile config;
    private Crypto crypto;
    private CryptoKey key;

    public override void _Ready()
    {
        base._Ready();
        config = new ConfigFile();
        crypto = new Crypto();
        LoadOrCreateKey();
        LoadSettings();
    }

    private void LoadOrCreateKey()
    {
        key = new CryptoKey();
        if (FileAccess.FileExists(KEY_FILE))
        {
            Error err = key.Load(KEY_FILE);
            if (err != Error.Ok)
            {
                GD.PushError("Failed to load encryption key. Generating a new one");
                GenerateNewKey();
            }
        }
    }


    private void LoadSettings()
    {
        Error err = config.Save(CONFIG_FILE);
        if (err != Error.Ok)
            GD.PushError("Failed to save settings");
    }

    private void GenerateNewKey()
    {
        key = crypto.GenerateRsa(2048);
        Error err = key.Save(KEY_FILE);
        if (err != Error.Ok)
        {
            GD.PushError("Failed to save new key.");
        }
    }

    public void SetTwitchAccessToken(string accessCode)
    {
        byte[] encrypted = crypto.Encrypt(key, Encoding.UTF8.GetBytes(accessCode));
        config.SetValue("Twitch", "AccessToken", Convert.ToBase64String(encrypted));
        SaveSettings();
    }

    public string GetTwitchAccessToken()
    {
        string encryptedBase64 = (string)config.GetValue("Twitch", "AccessToken", "");
        if (string.IsNullOrEmpty(encryptedBase64))
            return "";
        
        byte[] encrypted = Convert.FromBase64String(encryptedBase64);
        byte[] decrypted = crypto.Decrypt(key, encrypted);

        return Encoding.UTF8.GetString(decrypted);
    }

    private void SaveSettings()
    {
        Error err = config.Save(CONFIG_FILE);
        if (err != Error.Ok)
            GD.PushError("Failed to save settings");
    }
}

using Godot;
using System;
using System.Collections.Generic;
using System.Text;
using UNL;
using UNLTeamJumpQuest.TwitchIntegration;

public partial class SettingsManager : Node
{
    private const string CONFIG_FILE = "user://settings.cfg";
    private const string KEY_FILE = "user://encryption.key";
    
    private PackedScene floatingMessageScene;

    public TeamManager UNLTeams {get; private set;}
    public TeamPageManager TeamPages { get; private set; }
    private const string TEAMS_FILE = "user://teams.json";

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

        TeamPages = new TeamPageManager();
        InitialiseTeams();
        EnsureTeamImagesDirectoryExists();
    }

    private void InitialiseTeams()
    {
        UNLTeams = new TeamManager();
        LoadTeams();
        if (UNLTeams.Teams == null || UNLTeams.Teams.Count == 0)
        {
            UNLTeams.Teams = new List<Team>();
        }
        TeamPages.InitializeFromList(UNLTeams.Teams);
    }

    private void LoadTeams()
    {
        if (FileAccess.FileExists(TEAMS_FILE))
        {
            try
            {
                using var file = FileAccess.Open(TEAMS_FILE, FileAccess.ModeFlags.Read);
                string json = file.GetAsText();
                UNLTeams = TeamManager.FromJson(json);
                if (UNLTeams == null)
                {
                    UNLTeams = new TeamManager();
                }
            }
            catch (Exception e)
            {
                GD.PrintErr($"Failed to load teams: {e.Message}");
                UNLTeams = new TeamManager();
            }
        }
        else
        {
            GD.Print("No existing team file found.");
            UNLTeams = new TeamManager();
        }
    }

    public void SaveTeamsToJson()
    {
        try
        {
            SyncUNLTeamsWithTeamPages();
            
            // Convert logos to base64 before saving
            foreach (var team in UNLTeams.Teams)
            {
                string fullPath = $"user://{team.LogoPath.ToLower()}";
                if (FileAccess.FileExists(fullPath))
                {
                    var imageBytes = FileAccess.GetFileAsBytes(fullPath);
                    team.LogoBase64 = Convert.ToBase64String(imageBytes);
                }
            }

            using var file = FileAccess.Open(TEAMS_FILE, FileAccess.ModeFlags.Write);
            string json = UNLTeams.ToJson();
            file.StoreString(json);

            // Clear base64 data after saving to free up memory
            foreach (var team in UNLTeams.Teams)
            {
                team.LogoBase64 = null;
            }
        }
        catch (Exception e)
        {
            GD.PrintErr($"Error saving teams: {e.Message}");
            ShowFloatingMessage("Failed to save teams!", false);
        }
    }

    private void SyncUNLTeamsWithTeamPages()
    {
        // Clear UNLTeams and repopulate it with teams from TeamPages
        UNLTeams.Teams.Clear();
        for (int i = 0; i < TeamPages.GetTotalTeams() - 1; i++) // -1 to exclude the "new team" page
        {
            var team = TeamPages.GetTeamAtIndex(i);
            if (team != null)
            {
                UNLTeams.Teams.Add(team);
            }
        }
    }

    public void DeleteTeam(Team team)
    {
        UNLTeams.Teams.Remove(team);
        TeamPages.RemoveTeam(team);
        DeleteTeamLogo(team.LogoPath);

        SaveTeamsToJson();

        ShowFloatingMessage("Team deleted successfully!", true);
    }

    private void DeleteTeamLogo(string logoPath)
    {
        string fullPath = $"user://{logoPath}";
        if (FileAccess.FileExists(fullPath))
        {
            Error err = DirAccess.RemoveAbsolute(fullPath);
            if (err != Error.Ok)
            {
                GD.PrintErr($"Failed to delete logo file: {fullPath}. Error: {err}");
            }
        }
    }

    public void AddTeamToList(string teamName, string teamAbbrev, Texture2D logo, ColorPickerButton[] avatarColours, string hexColourCode)
    {
        if (UNLTeams == null)
        {
            UNLTeams = new TeamManager();
        }

        Color[] colours = new Color[5]
        {
            avatarColours[0].Color,
            avatarColours[1].Color,
            avatarColours[2].Color,
            avatarColours[3].Color,
            avatarColours[4].Color,
        };

        string fileName = $"TeamImages/{teamName.ToLower()}.png";
        string fullPath = $"user://{fileName}";

        Image image = logo.GetImage();
        image.SavePng(fullPath);

        UNLTeams.AddTeam(teamName, teamAbbrev, fileName, colours, hexColourCode);

        // Add team to the TeamPages version
        TeamPages.AddTeam(UNLTeams.Teams[UNLTeams.Teams.Count - 1]);
    }


    public string ExportTeams()
    {
        foreach (var team in UNLTeams.Teams)
        {
            string fullPath = $"user://{team.LogoPath.ToLower()}";
            if (FileAccess.FileExists(fullPath))
            {
                var imageBytes = FileAccess.GetFileAsBytes(fullPath);
                team.LogoBase64 = Convert.ToBase64String(imageBytes);
            }
        }
        return UNLTeams.ToJson();
    }

    public void ImportTeams(string json)
    {
        UNLTeams = TeamManager.FromJson(json);
        foreach (var team in UNLTeams.Teams)
        {
            if (!string.IsNullOrEmpty(team.LogoBase64))
            {
                string fullPath = $"user://{team.LogoPath.ToLower()}";
                var imageBytes = Convert.FromBase64String(team.LogoBase64);
                var file = FileAccess.Open(fullPath, FileAccess.ModeFlags.Write);
                file.StoreBuffer(imageBytes);
                file.Close();
                team.LogoBase64 = null; // Clear to save space
            }
        }
        TeamPages.InitializeFromList(UNLTeams.Teams);
    }

    private void EnsureTeamImagesDirectoryExists()
    {
        string directoryPath = "user://TeamImages";
        DirAccess dir = DirAccess.Open("user://");
        if (dir == null)
        {
            GD.PrintErr("Failed to open user:// directory");
            return;
        }

        if (!dir.DirExists(directoryPath))
        {
            Error err = dir.MakeDir(directoryPath);
            if (err == Error.Ok)
            {
                GD.Print($"Directory created successfully: {directoryPath}");
            }
            else
            {
                GD.PrintErr($"Failed to create directory: {directoryPath}. Error: {err}");
            }
        }
        else
        {
            GD.Print($"Directory already exists: {directoryPath}");
        }
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

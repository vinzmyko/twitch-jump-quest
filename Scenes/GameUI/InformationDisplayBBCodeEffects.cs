using System;
using System.Linq;
using Godot;

public partial class InformationDisplay: Control
{
    private const int OUTLINE_SIZE = 12;
    private const int HIGHLIGHT_FONT_SIZE = 20; 
    private string GetTeamHexColour(string teamAbbrev)
    {
        if (settingsManager.UNLTeams == null || settingsManager.UNLTeams.Teams == null)
        {
            GD.PrintErr("UNLTeams or Teams list is null");
            return "#FFFFFF"; 
        }

        UNL.Team team = settingsManager.UNLTeams.Teams.FirstOrDefault(t => t.TeamAbbreviation.ToLower().Equals(teamAbbrev.ToLower(), StringComparison.OrdinalIgnoreCase));

        if (team != null)
        {
            return team.HexColourCode;
        }
        else
        {
            GD.PrintErr($"Team with abbreviation {teamAbbrev} not found");
            return "#FFFFFF";
        }
    }

    private string FormatTeamAbbrev(string teamAbbrev)
    {
        string teamHex = GetTeamHexColour(teamAbbrev);
        return $"[color=white][outline_color={teamHex}][outline_size={OUTLINE_SIZE}][font_size={HIGHLIGHT_FONT_SIZE}]{teamAbbrev}[/font_size][/outline_size][/outline_color][/color]";
    }

    private string FormatJoinMessage(string playerName, string teamAbbrev)
    {
        string template = playerJoinMessages[random.Next(playerJoinMessages.Length)];
        return string.Format(template, 
            $"[color=green][font_size={HIGHLIGHT_FONT_SIZE}]{playerName}[/font_size][/color]", 
            FormatTeamAbbrev(teamAbbrev));
    }

    private string FormatDeathMessage(string playerName, string teamAbbrev)
    {
        string template = playerDiedMessages[random.Next(playerDiedMessages.Length)];
        return string.Format(template, 
            $"[color=red][font_size={HIGHLIGHT_FONT_SIZE}]{playerName}[/font_size][/color]", 
            FormatTeamAbbrev(teamAbbrev));
    }

    private string FormatFaceplantMessage(string playerName, string teamAbbrev, int distance)
    {
        var rng = random.Next(playerFaceplantMessages.Length);
        string template = playerFaceplantMessages[rng];
        GD.Print(rng);
        return string.Format(template, 
            $"[color=orange][font_size={HIGHLIGHT_FONT_SIZE}]{playerName}[/font_size][/color]", 
            FormatTeamAbbrev(teamAbbrev), 
            $"[color=orange][b][u][font_size={HIGHLIGHT_FONT_SIZE}]{distance}[/font_size][/u][/b][/color]");
    }

    private string FormatComboStreakMessage(string playerName, string teamAbbrev, int comboStreak)
    {
        var rng = random.Next(playerComboStreakMessages.Length);
        string template = playerComboStreakMessages[rng];
        return string.Format(template, 
            $"[color=purple][font_size={HIGHLIGHT_FONT_SIZE}]{playerName}[/font_size][/color]", 
            FormatTeamAbbrev(teamAbbrev), 
            $"[color=yellow][b][u][font_size={HIGHLIGHT_FONT_SIZE}]{comboStreak}[/font_size][/u][/b][/color]");
    }

    // You might want to add a method to set the base font size for the entire RichTextLabel
    public void SetBaseFontSize(int size)
    {
        // Assuming you have a reference to your RichTextLabel
        // chatBoxLabel.AddThemeFontSizeOverride("normal_font_size", size);
    }
}
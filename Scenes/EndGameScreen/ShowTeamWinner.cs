using Godot;
using System;

public partial class ShowTeamWinner : Control
{
    [Export]
    public TextureRect winningTeamLogoRect;
    [Export]
    public Label winningTeamLabel;
    [Export]
    public Label winningTeamPointsLabel;
    public UNL.TeamScore winningTeam;

    public override void _Ready()
    {
        base._Ready();
    }

    public void SetWinningTeamNameLabel(string text)
    {
        winningTeamLabel.Text = text;
    }
    public void SetWinningTeamPointsLabel(string text)
    {
        winningTeamPointsLabel.Text = text;
    }

    public void DisplayWinningTeamLogo()
    {
        // WinningTeamMethod in parent class. Set a variable like UNL.TeamScore winningTeam in class scope during parent initalisation.
        if (winningTeam != null)
        {
            string logoPath = winningTeam.TeamInfo.LogoPath;
            var winningTeamLogo = LoadLogoTexture(logoPath);
            if (winningTeamLogo != null && winningTeamLogoRect != null)
            {
                winningTeamLogoRect.Texture = winningTeamLogo;
            }
            else
            {
                GD.PrintErr("Failed to load winning team logo or TextureRect not assigned");
            }
        }
        else
        {
            GD.PrintErr("No winning team found");
        }
    }

    private ImageTexture LoadLogoTexture(string logoPath)
    {
        string fullPath = $"user://{logoPath}";
        if (FileAccess.FileExists(fullPath))
        {
            var image = Image.LoadFromFile(fullPath);
            if (image != null)
            {
                return ImageTexture.CreateFromImage(image);
            }
            else
            {
                GD.PrintErr($"Failed to load image from file: {fullPath}");
            }
        }
        else
        {
            GD.PrintErr($"Logo file does not exist: {fullPath}");
        }
        return null;
    }
}

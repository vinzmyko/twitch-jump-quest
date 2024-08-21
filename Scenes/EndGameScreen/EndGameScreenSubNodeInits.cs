using Godot;

public partial class EndGameScreen : Control
{
    public void SetShowTeamWinnerNodes()
    {
        showTeamsWinner.winningTeam = teamThatWon;
        showTeamsWinner.SetWinningTeamNameLabel($"WP! {teamThatWon.TeamInfo.TeamName}");
        showTeamsWinner.SetWinningTeamPointsLabel($"Points: {teamThatWon.TotalScore}");
        showTeamsWinner.DisplayWinningTeamLogo();
    }

    public void SetShowMVPNodes()
    {
        showMVP.SetMVPControlNodeInfo(MVP);
    }

    public void SetShowHallOfFameNodes()
    {
        showHOF.SetLeastPoints(LowestPoints(stats));
        showHOF.SetHighestDistanceTravelled(HighestDistanceTravelled(stats));
        showHOF.SetLeastDistanceTravelled(LowestDistanceTravelled(stats));
        showHOF.SetHighestComboStreak(HighestCombo(stats));
        showHOF.SetMostFaceplants(NumOfFaceplants(stats));
        showHOF.SetLongestFaceplant(FaceplantDistance(stats));
    }
}

using Godot;
using System;

public partial class ShowMVP : Control
{
    [Export]
    public Label mvpDisplayName;
    [Export]
    public Label mvpPlayerPoints;
    [Export]
    public Label mvpDistanceTravelled;
    [Export]
    public Label mvpNumOfFaceplants;
    [Export]
    public Label mvpLongestFaceplantDistance;
    [Export]
    public Label mvpComboStreak;

    public void SetMVPControlNodeInfo(PlayerInfo mvp)
    {
        mvpDisplayName.Text = mvp.DisplayName;
        mvpPlayerPoints.Text = $"Points: {mvp.points}";
        mvpDistanceTravelled.Text = $"Distance Travelled: {mvp.HighestYPos:F1}m";

        string noOfFp = mvp.NumOfFaceplants == 0 ? "N/A" : mvp.NumOfFaceplants.ToString();
        mvpNumOfFaceplants.Text = $"Number of Faceplants: {noOfFp}";

        string disOfLongestFp = mvp.DistanceOfFurthestFaceplant == 0 ? "N/A" : $"{mvp.DistanceOfFurthestFaceplant:F1}m";
        mvpLongestFaceplantDistance.Text = $"Longest Faceplant Distance: {disOfLongestFp}";

        string comboStreak = mvp.ComboStreak == 0 ? "N/A" : $"{mvp.ComboStreak}";
        mvpComboStreak.Text = $"Highest Combo Streak: {comboStreak}";
    }
}

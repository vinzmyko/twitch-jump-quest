using Godot;

public partial class ShowHallOfFame : Control
{
    [Export]
    public Label leastPoints;
    [Export]
    public Label leastDistanceTravelled;
    [Export]
    public Label highestDistanceTravelled;
    [Export]
    public Label highestComboStreak;
    [Export]
    public Label mostFacePlants;
    [Export]
    public Label longestDistanceOfFaceplant;

    public void SetLeastPoints(PlayerInfo player)
    {
        leastPoints.Text = $"Lowest Points Gained: {player.DisplayName} ({player.points})";
    }

    public void SetLeastDistanceTravelled(PlayerInfo player)
    {
        leastDistanceTravelled.Text = $"Lowest Distance Travelled: {player.DisplayName} ({player.HighestYPos:F1}m)";
    }
    
    public void SetHighestComboStreak(PlayerInfo player)
    {
        highestComboStreak.Text = $"Highest Combo Streak: {player.DisplayName} ({player.ComboStreak})";
    }

    public void SetMostFaceplants(PlayerInfo player)
    {
        mostFacePlants.Text = $"Most Faceplants: {player.DisplayName} ({player.NumOfFaceplants})";
    }

    public void SetLongestFaceplant(PlayerInfo player)
    {
        longestDistanceOfFaceplant.Text = $"Longest Faceplant Distance: {player.DisplayName} ({player.DistanceOfFurthestFaceplant})";
    }

    public void SetHighestDistanceTravelled(PlayerInfo player)
    {
        float roundedDistance = Mathf.RoundToInt(player.HighestYPos);
        highestDistanceTravelled.Text = $"Highest Distance Travelled: {player.DisplayName} ({roundedDistance:F1}m)";
    }
}

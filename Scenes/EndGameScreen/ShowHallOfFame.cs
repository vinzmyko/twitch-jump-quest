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
        if (player == null)
            leastPoints.Text = "Lowest Points Gained: N/A";
        else
            leastPoints.Text = $"Lowest Points Gained: {player.DisplayName} ({player.points})";
    }

    public void SetLeastDistanceTravelled(PlayerInfo player)
    {
        if (player == null)
            leastDistanceTravelled.Text = "Lowest Distance Travelled: N/A";
        else
            leastDistanceTravelled.Text = $"Lowest Distance Travelled: {player.DisplayName} ({player.HighestYPos:F1}m)";
    }
    
    public void SetHighestComboStreak(PlayerInfo player)
    {
        if (player == null)
            highestComboStreak.Text = "Highest Combo Streak: N/A";
        else
            highestComboStreak.Text = $"Highest Combo Streak: {player.DisplayName} ({player.ComboStreak})";
    }

    public void SetMostFaceplants(PlayerInfo player)
    {
        if (player == null)
            mostFacePlants.Text = "Most Faceplants: N/A";
        else
            mostFacePlants.Text = $"Most Faceplants: {player.DisplayName} ({player.NumOfFaceplants})";
    }

    public void SetLongestFaceplant(PlayerInfo player)
    {
        if (player == null)
            longestDistanceOfFaceplant.Text = "Longest Faceplant Distance: N/A";
        else
            longestDistanceOfFaceplant.Text = $"Longest Faceplant Distance: {player.DisplayName} ({player.DistanceOfFurthestFaceplant})";
    }

    public void SetHighestDistanceTravelled(PlayerInfo player)
    {
        if (player == null)
            highestDistanceTravelled.Text = "Highest Distance Travelled: N/A";
        else
            highestDistanceTravelled.Text = $"Highest Distance Travelled: {player.DisplayName} ({player.HighestYPos:F1}m)";
    }
}

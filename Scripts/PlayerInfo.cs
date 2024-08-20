public class PlayerInfo
    {
    public string DisplayName { get; }
    public string UserID { get; }
    public UNL.Team team { get; }
    public string TeamAbbrev { get; }
    public float HighestYPos { get; }
    public int ComboStreak { get; }
    public int NumOfFaceplants { get; }
    public int DistanceOfFurthestFaceplant { get; }
    public int IdxOfUniqueFeatherColours { get; }

    public PlayerInfo(Player player)
    {
        DisplayName = player.displayName;
        UserID = player.userID;
        team = player.team;
        HighestYPos = player.highestYPosition;
        ComboStreak = player.comboStreak;
        NumOfFaceplants = player.numOfFaceplants;
        DistanceOfFurthestFaceplant = player.distanceOfFurthestFaceplant;
        IdxOfUniqueFeatherColours = player.idxOfUniqueFeatherColour;
    }
}
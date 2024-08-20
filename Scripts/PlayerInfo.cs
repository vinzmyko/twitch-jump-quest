public class PlayerInfo
    {
    public string DisplayName { get; }
    public string UserID { get; }
    public string TeamAbbrev { get; }
    public float HighestYPos { get; }
    public int ComboStreak { get; }
    public int NumOfFaceplants { get; }
    public int DistanceOfFurthestFaceplant { get; }
    public int IdxOfUniqueFeatherColours { get; }

    public PlayerInfo(string displayName, string userID, string teamAbbrev, float highestYPos, int comboStreak, int numOfFaceplants, int distanceOfFurthestFaceplant, int idxOfUniqueFeatherColours)
    {
        DisplayName = displayName;
        UserID = userID;
        TeamAbbrev = teamAbbrev;
        HighestYPos = highestYPos;
        ComboStreak = comboStreak;
        NumOfFaceplants = numOfFaceplants;
        DistanceOfFurthestFaceplant = distanceOfFurthestFaceplant;
        IdxOfUniqueFeatherColours = idxOfUniqueFeatherColours;
    }
    public PlayerInfo(string displayName, string userID, string teamAbbrev)
    {
        DisplayName = displayName;
        UserID = userID;
        TeamAbbrev = teamAbbrev;
        HighestYPos = 0.0f; 
        ComboStreak = 0; 
        NumOfFaceplants = 0; 
        DistanceOfFurthestFaceplant = 0; 
        IdxOfUniqueFeatherColours = -1; 
    }
}
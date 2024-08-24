using Godot;
using System;
using System.Collections.Generic;

public class PlayerInfo : IEquatable<PlayerInfo>
{
    public string DisplayName { get; }
    public string UserID { get; }
    public UNL.Team team { get; }
    public string TeamAbbrev { get; }
    public int points { get; }
    public int HighestYPos { get; }
    public int ComboStreak { get; }
    public int NumOfFaceplants { get; }
    public int DistanceOfFurthestFaceplant { get; }
    public int IdxOfUniqueFeatherColours { get; }

    public PlayerInfo(Player player)
    {
        DisplayName = player.displayName;
        UserID = player.userID;
        team = player.team;
        TeamAbbrev = player.team?.TeamAbbreviation;
        points = player.points;
        ComboStreak = player.comboStreak;
        NumOfFaceplants = player.numOfFaceplants;
        // DistanceOfFurthestFaceplant = player.distanceOfFurthestFaceplant;
        IdxOfUniqueFeatherColours = player.idxOfUniqueFeatherColour;

        HighestYPos = (int)Mathf.Abs(player.startingYpos - player.highestYPosition) / 16;
        DistanceOfFurthestFaceplant = player.distanceOfFurthestFaceplant / 16;
    }
    public override bool Equals(object obj)
    {
        return Equals(obj as PlayerInfo);
    }

    public bool Equals(PlayerInfo other)
    {
        return other != null && UserID == other.UserID;
    }

    public override int GetHashCode()
    {
        return UserID.GetHashCode();
    }

    public static bool operator ==(PlayerInfo left, PlayerInfo right)
    {
        return EqualityComparer<PlayerInfo>.Default.Equals(left, right);
    }

    public static bool operator !=(PlayerInfo left, PlayerInfo right)
    {
        return !(left == right);
    }
}
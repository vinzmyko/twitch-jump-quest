using System;
using System.Linq;
using Godot;
using TwitchLib.Api.Helix.Models.Teams;

public static class MessageParser
{
    public static (bool isValid, float angle, float power) ParseMessage(string message)
    {
        float angle = 0;
        float power = 100;
        bool isValid = false;
        
        string[] messageParts = message.ToLower().Split(new[] { ' ' }, StringSplitOptions.RemoveEmptyEntries);
        
        if (messageParts.Length == 0)
        {
            return (false, angle, power);
        }

        string command = messageParts[0];

        // Parse direction and angle
        if (command == "u")
        {
            angle = 0;
            isValid = true;
        }
        else if (command.StartsWith("l") || command.StartsWith("r") || 
                 command.StartsWith("jump") || command.StartsWith("j"))
        {
            string angleStr;
            int minAngle, maxAngle;
            bool isLeftOrRight = command.StartsWith("l") || command.StartsWith("r");
            
            if (isLeftOrRight)
            {
                angleStr = command.Length > 1 ? command.Substring(1) : (messageParts.Length > 1 ? messageParts[1] : "");
                minAngle = 0;
                maxAngle = 90;
            }
            else // jump or j
            {
                angleStr = command.Length > (command.StartsWith("jump") ? 4 : 1) 
                    ? command.Substring(command.StartsWith("jump") ? 4 : 1) 
                    : (messageParts.Length > 1 ? messageParts[1] : "");
                minAngle = -90;
                maxAngle = 90;
            }

            if (ParseAngle(angleStr, out angle))
            {
                angle = Math.Clamp(angle, minAngle, maxAngle);
                if (command.StartsWith("l")) angle = -Math.Abs(angle);
                if (command.StartsWith("r")) angle = Math.Abs(angle);
                isValid = true;
            }
        }

        // Parse power
        if (isValid)
        {
            int powerIndex = (command.StartsWith("l") || command.StartsWith("r") || command.StartsWith("j")) && command.Length > 1 ? 1 : 2;
            if (messageParts.Length > powerIndex)
            {
                string stringPower = messageParts[messageParts.Length - 1];
                if (float.TryParse(stringPower, out float parsedPower))
                {
                    power = Math.Clamp(parsedPower, 1, 100);
                }
            }
        }

        return (isValid, angle, power);
    }

    private static bool ParseAngle(string stringAngle, out float angle)
    {
        return float.TryParse(stringAngle, out angle);
    }

    public static (bool isValid, UNL.Team team) ParseTeamJoin(string message, UNL.TeamManager UNLTeams)
    {
        bool isValid = false;
        string teamAbbrev = string.Empty;
        UNL.Team targetTeam = null;
        string[] messageParts = message.ToLower().Split(new[] { ' ' }, StringSplitOptions.RemoveEmptyEntries);

        // Ignore join request if message is one word or less
        if (messageParts.Length <= 1)
        {
            return (false, targetTeam);
        }

        string command = messageParts[0];
        // If first word in not command
        if (command != "join")
        {
            return (false, targetTeam);
        }

        // Problem that bugs the parser is messageParts exceeded array size
        foreach (UNL.Team team in UNLTeams.Teams)
        {
            if (team.TeamAbbreviation.ToLower() == messageParts[1])
            {
                teamAbbrev = messageParts[1];
            }
        }

        foreach (UNL.Team team in UNLTeams.Teams)
        {
            if (teamAbbrev == team.TeamAbbreviation.ToLower())
            {
                targetTeam = team;
                isValid = true;
            }
        }

        return (isValid, targetTeam);
    }
}

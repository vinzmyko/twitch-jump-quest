using Godot;
using System;
using System.Collections.Generic;
using Newtonsoft.Json;

namespace UNL
{
    [Serializable]
    public class Team
    {
        public string TeamName { get; set; }
        public string TeamAbbreviation { get; set; }
        public string LogoPath { get; set; }
        public AvatarColours TeamColours { get; set; }
        public string LogoBase64 { get; set; }

        [Serializable]
        public class AvatarColours
        {
            [JsonConverter(typeof(ColorJsonConverter))]
            public Color CapeMain { get; set; } 
            [JsonConverter(typeof(ColorJsonConverter))]
            public Color CapeTrim { get; set; } 
            [JsonConverter(typeof(ColorJsonConverter))]
            public Color ArmourLight { get; set; } 
            [JsonConverter(typeof(ColorJsonConverter))]
            public Color ArmourMedium { get; set; } 
            [JsonConverter(typeof(ColorJsonConverter))]
            public Color ArmourDark { get; set; } 
        }
    }

    [Serializable]
    public class TeamManager
    {
        public List<Team> Teams { get; set; }
        public int TotalTeams => Teams.Count;

        public TeamManager()
        {
            Teams = new List<Team>();
        }

        public string ToJson()
        {
            return JsonConvert.SerializeObject(this, Formatting.Indented);
        }

        public static TeamManager FromJson(string json)
        {
            var manager = JsonConvert.DeserializeObject<TeamManager>(json);
            if (manager == null || manager.Teams == null)
            {
                return new TeamManager();
            }
            return manager;
        }

        public void AddTeam(string teamName, string teamAbbrev, string logoPath, Color[] avatarColours)
        {
            var team = new Team
            {
                TeamName = teamName,
                TeamAbbreviation = teamAbbrev,
                LogoPath = logoPath,
                TeamColours = new Team.AvatarColours
                {
                    CapeMain = avatarColours[0],
                    CapeTrim = avatarColours[1],
                    ArmourLight = avatarColours[2],
                    ArmourMedium = avatarColours[3],
                    ArmourDark = avatarColours[4],
                }
            };
            Teams.Add(team);
        }

        public void RemoveTeam(Team team)
        {
            Teams.Remove(team);
        }
    }
    public class ColorJsonConverter : JsonConverter<Color>
    {
        public override Color ReadJson(JsonReader reader, Type objectType, Color existingValue, bool hasExistingValue, JsonSerializer serializer)
        {
            string htmlColor = (string)reader.Value;
            return Color.FromHtml(htmlColor);
        }

        public override void WriteJson(JsonWriter writer, Color value, JsonSerializer serializer)
        {
            writer.WriteValue(value.ToHtml());
        }
    }
}
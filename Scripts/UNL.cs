using Godot;
using System;
using System.Collections.Generic;
using Newtonsoft.Json;
using System.Linq;

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
        public string HexColourCode { get; set; }

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

        public void AddTeam(string teamName, string teamAbbrev, string logoPath, Color[] avatarColours, string hexColourCode)
        {
            var team = new Team
            {
                TeamName = teamName,
                TeamAbbreviation = teamAbbrev,
                LogoPath = logoPath,
                HexColourCode = hexColourCode,
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

        public Team GetTeam(UNL.Team team)
        {
            Team hasTeam = Teams.FirstOrDefault(team => team.TeamAbbreviation.ToLower() == team.TeamAbbreviation);
            return team;
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

    public class TeamScore
    {
        public Team TeamInfo { get; private set; }
        public int PlayerCount { get; private set; }
        public int TotalScore { get; private set; }

        public TeamScore(Team team)
        {
            TeamInfo = team;
            PlayerCount = 0;
            TotalScore = 0;
        }

        public void AddPlayer()
        {
            PlayerCount++;
        }

        public void RemovePlayer()
        {
            if (PlayerCount > 0)
                PlayerCount--;
        }

        public void AddScore(int score)
        {
            TotalScore += score;
        }
    }

    public class TeamScoreManager
    {
        private Dictionary<string, TeamScore> teamScores;

        public TeamScoreManager()
        {
            teamScores = new Dictionary<string, TeamScore>();
        }

        public string GetTeamName(string teamAbbrev)
        {
            return teamScores[teamAbbrev].TeamInfo.TeamName;
        }


        public void AddTeam(UNL.Team team)
        {
            string key = team.TeamAbbreviation.ToUpper();
            GD.Print($"TeamScoreManager: Adding team {key}");
            if (!teamScores.ContainsKey(key))
            {
                teamScores[key] = new TeamScore(team);
                GD.Print($"TeamScoreManager: Team {key} added successfully");
            }
            else
            {
                GD.Print($"TeamScoreManager: Team {key} already exists");
            }
        }

        public void AddPlayerToTeam(string teamAbbrev)
        {
            if (teamScores.TryGetValue(teamAbbrev, out TeamScore teamScore))
            {
                teamScore.AddPlayer();
            }
        }

        public void RemovePlayerFromTeam(string teamAbbrev)
        {
            if (teamScores.TryGetValue(teamAbbrev, out TeamScore teamScore))
            {
                teamScore.RemovePlayer();
            }
        }

        public void AddScoreToTeam(string teamAbbrev, int score)
        {
            if (teamScores.TryGetValue(teamAbbrev, out TeamScore teamScore))
            {
                teamScore.AddScore(score);
            }
        }

        public TeamScore GetTeamScore(string teamAbbrev)
        {
            return teamScores.TryGetValue(teamAbbrev, out TeamScore teamScore) ? teamScore : null;
        }

        public IEnumerable<TeamScore> GetAllTeamScores()
        {
            GD.Print($"TeamScoreManager: GetAllTeamScores called. Teams: {string.Join(", ", teamScores.Keys)}");
            return teamScores.Values;
        }

        public void ClearTeams()
        {
            teamScores.Clear();
        }


        public bool TeamExists(string teamAbbrev)
        {
            string key = teamAbbrev.ToUpper();
            bool exists = teamScores.ContainsKey(key);
            GD.Print($"TeamScoreManager: TeamExists check for {key} - Result: {exists}");
            GD.Print($"TeamScoreManager: Current teams: {string.Join(", ", teamScores.Keys)}");
            return exists;
        }

        public int GetTeamPlayerCount(string teamAbbrev)
        {
            teamAbbrev = teamAbbrev.ToUpper();
            if (teamScores.TryGetValue(teamAbbrev, out TeamScore teamScore))
            {
                return teamScore.PlayerCount;
            }
            return -1; 
        }
    }
}
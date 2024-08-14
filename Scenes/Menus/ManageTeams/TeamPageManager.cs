using System.Collections.Generic;
using UNL;

public class TeamPageManager
{
    private List<Team> teams;
    private int currentIndex;

    public TeamPageManager()
    {
        teams = new List<Team>();
        currentIndex = -1;
    }

    public void AddTeam(Team team)
    {
        teams.Add(team);
        if (currentIndex == -1) currentIndex = 0;
    }

    public void RemoveTeam(Team team)
    {
        int index = teams.IndexOf(team);
        if (index != -1)
        {
            teams.RemoveAt(index);
            if (currentIndex >= teams.Count)
            {
                currentIndex = teams.Count - 1;
            }
        }
    }

    public Team GetTeamAtIndex(int index)
    {
        if (index >= 0 && index < teams.Count)
        {
            return teams[index];
        }
        return null;
    }

    public Team GetCurrentTeam()
    {
        return (currentIndex >= 0 && currentIndex < teams.Count) ? teams[currentIndex] : null;
    }

    public bool MoveNext()
    {
        if (currentIndex < teams.Count)
        {
            currentIndex++;
            return true;
        }
        return false;
    }

    public bool MovePrevious()
    {
        if (currentIndex > 0)
        {
            currentIndex--;
            return true;
        }
        return false;
    }

  public int GetCurrentIndex() => currentIndex + 1;
    public int GetTotalTeams() => teams.Count + 1; // +1 for the new blank page

    public void Clear()
    {
        teams.Clear();
        currentIndex = -1;
    }

    public void InitializeFromList(List<Team> teamList)
    {
        teams = new List<Team>(teamList);
        currentIndex = teams.Count > 0 ? 0 : -1;
    }
}

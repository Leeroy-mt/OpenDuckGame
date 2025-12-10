using System.Collections.Generic;

namespace DuckGame;

public class Results
{
    public static List<ResultData> teams
    {
        get
        {
            List<ResultData> data = new List<ResultData>();
            foreach (Team t in Teams.all)
            {
                if (t.activeProfiles.Count > 0)
                {
                    data.Add(new ResultData(t));
                }
            }
            return data;
        }
    }

    public static ResultData winner
    {
        get
        {
            List<ResultData> list = teams;
            list.Sort((ResultData a, ResultData b) => (a.score != b.score) ? ((a.score < b.score) ? 1 : (-1)) : 0);
            return list[0];
        }
    }

    public static ResultData runnerUp
    {
        get
        {
            List<ResultData> list = teams;
            list.Sort((ResultData a, ResultData b) => (a.score != b.score) ? ((a.score < b.score) ? 1 : (-1)) : 0);
            return list[1];
        }
    }

    public static ResultData loser
    {
        get
        {
            List<ResultData> list = teams;
            list.Sort((ResultData a, ResultData b) => (a.score != b.score) ? ((a.score > b.score) ? 1 : (-1)) : 0);
            return list[0];
        }
    }
}

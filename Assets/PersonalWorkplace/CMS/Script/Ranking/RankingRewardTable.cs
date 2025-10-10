using System.Collections.Generic;

[System.Serializable]
public class RankingReward
{
    public int minRank;
    public int maxRank;
    public int jadeAmount; // 용옥
}

public static class RankingRewardTable
{
    private static readonly List<RankingReward> rewards = new List<RankingReward>
    {
        new RankingReward { minRank = 1,    maxRank = 10,    jadeAmount = 5000 },
        new RankingReward { minRank = 11,   maxRank = 50,    jadeAmount = 4000 },
        new RankingReward { minRank = 51,   maxRank = 100,   jadeAmount = 3000 },
        new RankingReward { minRank = 101,  maxRank = 500,   jadeAmount = 2000 },
        new RankingReward { minRank = 501,  maxRank = 1000,  jadeAmount = 1000 },
        new RankingReward { minRank = 1001, maxRank = int.MaxValue, jadeAmount = 500 }
    };

    public static int GetRewardByRank(int rank)
    {
        foreach (var r in rewards)
        {
            if (rank >= r.minRank && rank <= r.maxRank)
                return r.jadeAmount;
        }
        return 0;
    }
}
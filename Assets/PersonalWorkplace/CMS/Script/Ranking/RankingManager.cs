using System.Collections.Generic;
using UnityEngine;
using System.Linq;

public class RankingManager : MonoBehaviour
{
    public static RankingManager Instance { get; private set; }

    private List<PlayerRankingData> rankingList = new List<PlayerRankingData>();
    private int myRank;
    private bool rewardClaimedToday = false;

    private void Awake()
    {
        Instance = this;
    }

    // 하루에 한 번 초기화 (서버에서 전투력 기반 랭킹 정렬)
    public void RefreshRanking(List<PlayerRankingData> allPlayers, string myPlayerId)
    {
        // 전투력 기준 내림차순 정렬
        rankingList = allPlayers.OrderByDescending(p => p.combatPower).ToList();

        for (int i = 0; i < rankingList.Count; i++)
        {
            rankingList[i].rank = i + 1;
            rankingList[i].isMyRanking = (rankingList[i].playerName == myPlayerId);

            if (rankingList[i].isMyRanking)
                myRank = rankingList[i].rank;
        }
    }

    public List<PlayerRankingData> GetRankingList() => rankingList;

    public PlayerRankingData GetMyRanking()
    {
        return rankingList.FirstOrDefault(p => p.isMyRanking);
    }

    public int GetMyReward()
    {
        return RankingRewardTable.GetRewardByRank(myRank);
    }

    public bool CanClaimReward()
    {
        return !rewardClaimedToday;
    }

    public void ClaimReward()
    {
        if (rewardClaimedToday) return;

        int jade = GetMyReward();
        CurrencyManager.Instance.Add(CurrencyType.Jewel, new BigCurrency(jade, 0));

        rewardClaimedToday = true;
        Debug.Log($"랭킹 보상 수령 완료: 용옥 {jade} 지급");
    }
}

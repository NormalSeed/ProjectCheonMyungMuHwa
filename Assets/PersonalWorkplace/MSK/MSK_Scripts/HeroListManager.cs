using System.Collections.Generic;
using UnityEngine;

public class HeroListManager : MonoBehaviour
{
    [SerializeField] private GameObject cardPrefab;
    [SerializeField] private Transform parentTransform;
    [SerializeField] private int poolSize = 50;

    private Queue<GameObject> pool = new Queue<GameObject>();
    private Dictionary<string, GameObject> activeCards = new Dictionary<string, GameObject>();

    private void Awake()
    {
        // 1) 풀 초기화: 미리 Instantiate 후 비활성화
        for (int i = 0; i < poolSize; i++)
        {
            var go = Instantiate(cardPrefab, parentTransform);
            go.SetActive(false);
            pool.Enqueue(go);
        }
    }

    // 2) UI 오픈 시 호출
    public void ShowHeroes(List<CardInfo> ownedHeroes)
    {
        foreach (var hero in ownedHeroes)
        {
            // 이미 활성화된 카드면 건너뛰기
            if (activeCards.ContainsKey(hero.HeroID))
                continue;

            // 풀에서 카드 꺼내기 (없으면 추가 Instantiate)
            GameObject card = pool.Count > 0
                ? pool.Dequeue()
                : Instantiate(cardPrefab, parentTransform);

            // 활성화 및 매핑 등록
            card.SetActive(true);
            activeCards.Add(hero.HeroID, card);
        }
    }
}

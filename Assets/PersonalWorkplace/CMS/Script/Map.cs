using UnityEngine;

public class Map : MonoBehaviour
{
    public Transform endPoint; // 다음 맵이 이어붙을 위치
    public int totalEnemies;   // 몬스터 + 보스 수
    private int defeatedEnemies = 0;

    public void OnEnemyDefeated()
    {
        defeatedEnemies++;
    }

    public bool IsCleared()
    {
        return defeatedEnemies >= totalEnemies;
    }
}

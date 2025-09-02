using UnityEngine;

public class Map : MonoBehaviour
{
    public Transform endPoint;
    public int totalEnemies;

    public bool IsCleared => InGameManager.Instance.monsterDeathStack.Value <= 0;

    //private int defeatedEnemies = 0;
    //private bool cleared = false; // 중복 방지

    //public void OnEnemyDefeated()
    //{
    //    defeatedEnemies++;

    //    if (!cleared && IsCleared())
    //    {
    //        cleared = true; // 딱 한 번만 실행
    //        Debug.Log("맵 클리어! 다음 스테이지 생성");

    //        if (MapManager.Instance != null)
    //        {
    //            MapManager.Instance.GoToNextStage(endPoint.position);
    //        }
    //    }
    //}

    //public bool IsCleared()
    //{
    //    return defeatedEnemies >= totalEnemies;
    //}
}
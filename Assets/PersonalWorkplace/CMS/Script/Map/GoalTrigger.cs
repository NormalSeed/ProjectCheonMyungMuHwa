//using UnityEngine;
//
//public class GoalTrigger : MonoBehaviour
//{
//    [Header("이 스테이지의 모든 몬스터 처치해야 클리어 가능")]
//    [SerializeField] private bool requireClearEnemies = true;
//
//    private void OnTriggerEnter2D(Collider2D other)
//    {
//        if (other.CompareTag("Player"))
//        {
//            if (requireClearEnemies && !AllEnemiesCleared())
//            {
//                Debug.Log("아직 몬스터가 남아있습니다!");
//                return;
//            }
//
//            Debug.Log("스테이지 클리어! 다음 맵으로 이동합니다.");
//            MapManager.Instance.GoToNextStage();
//        }
//    }
//
//    private bool AllEnemiesCleared()
//    {
//        GameObject[] enemies = GameObject.FindGameObjectsWithTag("Monster");
//        return enemies.Length == 0;
//    }
//}

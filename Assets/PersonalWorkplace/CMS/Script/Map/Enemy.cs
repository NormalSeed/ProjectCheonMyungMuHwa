using UnityEngine;

public class Enemy : MonoBehaviour
{
    private bool isDead = false;

    public void Die()
    {
        if (isDead) return; // 이미 죽었으면 무시
        isDead = true;

        // 여기서 Map.OnEnemyDefeated() 호출
        Map currentMap = GetComponentInParent<Map>();
        if (currentMap != null)
            currentMap.OnEnemyDefeated();

        Destroy(gameObject);
    }
}
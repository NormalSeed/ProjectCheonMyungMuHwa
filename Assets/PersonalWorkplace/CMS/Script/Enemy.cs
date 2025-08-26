using UnityEngine;

public class Enemy : MonoBehaviour
{
    private Map parentMap;

    private void Start()
    {
        parentMap = GetComponentInParent<Map>();
    }

    public void Die()
    {
        parentMap.OnEnemyDefeated();
        Destroy(gameObject);
    }
}
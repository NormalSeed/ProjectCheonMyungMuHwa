using UnityEngine;

[RequireComponent(typeof(Collider2D))]
public class EnemyTest : MonoBehaviour
{
    private Enemy enemy;

    private void Awake()
    {
        enemy = GetComponent<Enemy>();
    }

    private void OnCollisionEnter2D(Collision2D collision)
    {
        if (collision.collider.CompareTag("Player"))
        {
            Debug.Log($"{gameObject.name} : Player와 충돌,사망");
            enemy.Die();
        }
    }
}

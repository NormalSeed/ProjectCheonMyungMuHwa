using UnityEngine;

public class TestMonster : MonoBehaviour, IDamagable
{
    private double health = 100f;

    public void TakeDamage(double amount)
    {
        health -= amount;
        Debug.Log($"현재 체력 : {health}");
    }
}

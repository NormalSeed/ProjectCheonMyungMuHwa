using UnityEngine;

public interface IDamagable
{
    /// <summary>
    /// 데미지를 입을 때 호출되는 메서드
    /// </summary>
    /// <param name="amount">데미지 양</param>
    public void TakeDamage(double amount);
}

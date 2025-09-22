using UnityEngine;

public class PooledParticle : PooledObject
{
    private ParticleSystem ps;
    private float returnDelay;

    private void Awake()
    {
        ps = GetComponent<ParticleSystem>();
        if (ps == null)
            Debug.LogError($"{name}에 ParticleSystem이 없습니다.");
    }

    protected void OnEnable()
    {
        if (ps == null) return;

        ps.Play();

        // 파티클 지속 시간 계산
        var main = ps.main;
        returnDelay = main.duration + main.startLifetime.constantMax;

        // 자동 반환 예약
        Invoke(nameof(ReturnPool), returnDelay);
    }

    public void PlayAt(Vector3 position, Quaternion rotation = default)
    {
        transform.position = position;
        transform.rotation = rotation;
        gameObject.SetActive(true); // OnEnable()에서 Play됨
    }

    public override void ReturnPool()
    {
        CancelInvoke();

        if (ps != null)
            ps.Stop(true, ParticleSystemStopBehavior.StopEmittingAndClear);

        base.ReturnPool(); // PooledObject의 PushPool 호출
    }
}

using System;
using UnityEngine;

public class MonsterProjectile : MonoBehaviour, IPooled<MonsterProjectile>
{
    [SerializeField] float lifetime;
    [SerializeField] float velocity;

    private Rigidbody2D rigid;
    public Vector2 TargetPos { get; set; }

    public double Damage { get; set; }

    private Vector2 direction => (TargetPos - (Vector2)transform.position).normalized;
    public Action<IPooled<MonsterProjectile>> OnLifeEnded { get; set; }

    private float timer;

    void Update()
    {
        if (timer >= lifetime)
        {
            OnLifeEnded?.Invoke(this);
        }
        timer += Time.deltaTime;
    }

    void Awake()
    {
        rigid = GetComponent<Rigidbody2D>();
    }
    public void Shot()
    {
        transform.right = direction;
        rigid.linearVelocity = direction * velocity;
        timer = 0;
    }

    void OnTriggerEnter2D(Collider2D collision)
    {
        if (collision.tag == "Player")
        {
            IDamagable damagable = collision.GetComponent<IDamagable>();
            if (damagable != null)
            {
                damagable.TakeDamage(Damage);
                DamageText text = PoolManager.Instance.DamagePool.GetItem(TargetPos);
                text.SetText(Damage);
            }
            OnLifeEnded?.Invoke(this);
        }

    }


}

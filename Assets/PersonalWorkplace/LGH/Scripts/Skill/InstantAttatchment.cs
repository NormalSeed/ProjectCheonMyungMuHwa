using UnityEngine;

public class InstantAttatchment : PooledObject
{
    protected PlayerController controller;
    protected PlayerSkillSO skillData;
    protected float duration;

    protected void OnEnable()
    {
        controller = GetComponentInParent<PlayerController>();
    }

    protected void OnDisable()
    {
        this.ReturnPool();
    }

    protected virtual void Update()
    {
        duration -= Time.deltaTime;
        if (duration <= 0)
        {
            this.ReturnPool();
        }
    }

    public void Configure(float duration, PlayerSkillSO skillSO)
    {
        this.duration = duration;
        skillData = skillSO;
    }
}

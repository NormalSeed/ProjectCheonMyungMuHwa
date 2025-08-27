using UnityEngine;
using static UnityEngine.GraphicsBuffer;

public class AS02_SkillSet : SkillSet
{
    public float skill1Cooldown;
    public int skill2Count;

    public float skill1Duration;
    public float skill2Duration;
    public float skill1Range;
    public float skill2Range;

    private void Awake()
    {
        SkillSetID = "AS02";
        skill1Duration = 3f;
        skill2Duration = 5f;
        skill1Range = skills[0].SkillRange;
        skill2Range = skills[1].SkillRange;
    }

    public override void Skill1(Transform target)
    {
        Debug.Log($"controller: {controller}");
        Debug.Log($"model: {controller?.model}");
        Debug.Log($"modelSO: {controller?.model?.modelSO}");
        Debug.Log($"skills[0]: {(skills != null && skills.Count > 0 ? skills[0] : null)}");


        // 임시 스킬 기능
        IDamagable damagable = target.GetComponent<IDamagable>();

        if (damagable != null)
        {
            damagable.TakeDamage(
            controller.model.modelSO.ExtAtkPoint * skills[0].ExtSkillDmg  * 5+
            controller.model.modelSO.InnAtkPoint * skills[0].InnSkillDmg);
        }
        else
        {
            Debug.Log("IDamagable이 없음");
        }
    }


    public override void Skill2(Transform target)
    {
        IDamagable damagable = target.GetComponent<IDamagable>();

        if (damagable != null)
        {
            damagable.TakeDamage(
            controller.model.modelSO.ExtAtkPoint * skills[1].ExtSkillDmg * 2 +
            controller.model.modelSO.InnAtkPoint * skills[1].InnSkillDmg);
        }
        else
        {
            Debug.Log("IDamagable이 없음");
        }
    }
}

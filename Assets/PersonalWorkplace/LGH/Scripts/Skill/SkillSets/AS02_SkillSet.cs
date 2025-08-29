using System.Collections;
using System.Collections.Generic;
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

    public List<GameObject> effects = new();

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
        //// 임시 스킬 기능
        //IDamagable damagable = target.GetComponent<IDamagable>();

        //if (damagable != null)
        //{
        //    damagable.TakeDamage(
        //    controller.model.modelSO.ExtAtkPoint * skills[0].ExtSkillDmg  * 5+
        //    controller.model.modelSO.InnAtkPoint * skills[0].InnSkillDmg);
        //}
        //else
        //{
        //    Debug.Log("IDamagable이 없음");
        //}
        StartCoroutine(Skill1Routine(target));
    }
    private IEnumerator Skill1Routine(Transform target)
    {
        GameObject effect0 = effects[0];
        GameObject effect1 = effects[1];

        // 0번 이펙트 활성화
        effect0.transform.position = target.position;
        effect0.SetActive(true);

        yield return new WaitForSeconds(0.3f);

        // 0번 이펙트 비활성화
        effect0.SetActive(false);

        // 1번 이펙트 활성화
        effect1.transform.position = target.position;
        var timed = effect1.GetComponent<JeokRang_explosion>();
        timed.SetParent(this.transform);
        effect1.SetActive(true);
        effect1.transform.SetParent(null);
    }

    public override void Skill2(Transform target)
    {
        //IDamagable damagable = target.GetComponent<IDamagable>();

        //if (damagable != null)
        //{
        //    damagable.TakeDamage(
        //    controller.model.modelSO.ExtAtkPoint * skills[1].ExtSkillDmg * 2 +
        //    controller.model.modelSO.InnAtkPoint * skills[1].InnSkillDmg);
        //}
        //else
        //{
        //    Debug.Log("IDamagable이 없음");
        //}
        StartCoroutine(Skill2Routine(target));
    }
    private IEnumerator Skill2Routine(Transform target)
    {
        GameObject effect0 = effects[0];
        GameObject effect3 = effects[3];

        // 0번 이펙트 활성화
        effect0.transform.position = target.position;
        effect0.SetActive(true);

        // 이펙트가 종료될 때까지 대기 (예: 1초 후 자동 종료)
        yield return new WaitForSeconds(1f);
        effect0.SetActive(false);

        // 3번 이펙트를 타겟 위치에 활성화
        effect3.transform.position = target.position;
        effect3.SetActive(true);

        yield return new WaitForSeconds(0.3f);
        effect3.SetActive(false);

        // 10% 확률로 방어력 증가
        if (Random.value <= 0.1f)
        {
            double originalDef = controller.model.modelSO.DefPoint;
            double buffAmount = originalDef * 0.3f;
            controller.model.modelSO.DefPoint += buffAmount;

            yield return new WaitForSeconds(skill2Duration);

            // 방어력 원상복구
            controller.model.modelSO.DefPoint -= buffAmount;
        }
    }
}

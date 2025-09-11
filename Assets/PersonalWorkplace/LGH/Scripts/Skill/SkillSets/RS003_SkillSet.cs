using System.Collections;
using UnityEngine;

public class RS003_SkillSet : SkillSet
{
    [SerializeField] private GameObject skill1Effect_1;
    [SerializeField] private GameObject skill1Effect_2;
    [SerializeField] private GameObject skill2Effect;

    WaitForSeconds skill1Interval = new WaitForSeconds(0.7f);
    WaitForSeconds skill1Duration = new WaitForSeconds(5f);

    private void Awake()
    {
        SkillSetID = "RS003";
    }

    public override void Skill1(Transform target)
    {
        isSkillPlaying = true;
        spumC.PlayAnimation(PlayerState.OTHER, 0);
        StartCoroutine(Skill1Routine());
    }

    private IEnumerator Skill1Routine()
    {
        Transform origin = controller.transform;
        skill1Effect_1.transform.position = origin.position + offset;
        skill1Effect_1.SetActive(true);
        yield return skill1Interval;
        skill1Effect_1.SetActive(false);
        skill1Effect_2.transform.position = origin.position + offset;
        skill1Effect_2.SetActive(true);
        spumC.gameObject.SetActive(false);
        yield return skill1Duration;
        spumC.gameObject.SetActive(true);
        skill1Effect_2.SetActive(false);
        isSkillPlaying = false;
    }

    public override void Skill2(Transform target)
    {
        isSkillPlaying = true;
        spumC.PlayAnimation(PlayerState.ATTACK, 1);
        StartCoroutine(Skill2Routine(target));
    }

    private IEnumerator Skill2Routine(Transform target)
    {
        Transform originT = controller.transform;

        yield return new WaitForSeconds(0.7f);

        // 이동 방향 계산
        Vector3 origin = new Vector3(transform.position.x, transform.position.y, 0);
        Vector3 direction = new Vector3((target.position.x - origin.x), (target.position.y - origin.y), 0).normalized;
        Vector3 destination = origin + direction * 5f;

        float moveSpeed = 10f;
        float distance = Vector3.Distance(origin, destination);
        float moved = 0f;

        // 이동 완료까지 기다리기 (간단한 거리 기반 대기)
        while (moved < distance)
        {
            float step = moveSpeed * Time.deltaTime;
            originT.position = Vector3.MoveTowards(originT.position, destination, step);
            moved += step;
            yield return null;
        }

        // 이펙트 위치: 시작점과 도착점의 중간
        Vector3 midPoint = (origin + destination) / 2f;
        skill2Effect.transform.position = midPoint;

        // 이펙트 회전: 돌진 방향 기준
        float angle = Mathf.Atan2(direction.y, direction.x) * Mathf.Rad2Deg;
        Quaternion rotation = Quaternion.Euler(0f, 0f, angle);
        skill2Effect.transform.rotation = rotation;
        
        //이펙트 활성화
        skill2Effect.SetActive(true);

        yield return new WaitForSeconds(0.6f);

        skill2Effect.SetActive(false);
        isSkillPlaying = false;
    }
}

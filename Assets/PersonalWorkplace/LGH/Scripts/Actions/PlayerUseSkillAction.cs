using System;
using Unity.Behavior;
using UnityEngine;
using Action = Unity.Behavior.Action;
using Unity.Properties;

[Serializable, GeneratePropertyBag]
[NodeDescription(name: "PlayerUseSkill", story: "[Self] Use Skill to [Target] if [isSkillReady]", category: "Action", id: "e6f708863bb84ccd760ad1f7e1b6bf1f")]
public partial class PlayerUseSkillAction : Action
{
    [SerializeReference] public BlackboardVariable<GameObject> Self;
    [SerializeReference] public BlackboardVariable<GameObject> Target;
    [SerializeReference] public BlackboardVariable<bool> IsSkillReady;

    private PlayerController controller;
    private BehaviorGraphAgent BGagent;

    protected override Status OnStart()
    {
        controller = Self.Value.GetComponent<PlayerController>();
        BGagent = Self.Value.GetComponent<BehaviorGraphAgent>();

        Target.Value = GetTarget();

        // Target으로부터 IDamagable을 받아와 데미지를 줄 수 있는지 체크
        return Status.Running;
    }

    /// <summary>
    /// Monster 태그를 가진 오브젝트 중 거리가 가장 가까운 오브젝트를 반환하는 메서드
    /// </summary>
    /// <returns></returns>
    private GameObject GetTarget()
    {
        GameObject[] monsters = GameObject.FindGameObjectsWithTag("Monster");
        GameObject closest = null;
        float minDistance = Mathf.Infinity;
        Vector3 selfPosition = Self.Value.transform.position;

        foreach (GameObject monster in monsters)
        {
            float distance = Vector3.Distance(selfPosition, monster.transform.position);
            if (distance < minDistance)
            {
                minDistance = distance;
                closest = monster;
            }
        }

        return closest;
    }

    protected override Status OnUpdate()
    {
        if (IsSkillReady.Value == false)
        {
            return Status.Success;
        }

        if (Target.Value != null && IsSkillReady.Value == true)
        {
            Debug.Log("스킬 공격 실행");
            IDamagable target = Target.Value.GetComponent<IDamagable>();
            if (target != null)
            {
                // 스킬 데미지 주기
                target.TakeDamage(20.0f);
                // 스킬 쿨타임 초기화(SkillSet의 스킬 쿨타임으로 재설정 해야함)
                controller.curCool = controller.skillCooldown;
            }
            else
            {
                Debug.Log("데미지를 입힐 수 없는 상대입니다.");
            }
        }

        return Status.Success;
    }

    protected override void OnEnd()
    {
        

        // 타겟 재탐지
        Target.Value = GetTarget();
        if (Target.Value == null)
        {
            BGagent.SetVariableValue<bool>("isTargetDetected", false);
        }
    }
}


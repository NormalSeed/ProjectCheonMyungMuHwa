using System;
using Unity.Behavior;
using Unity.Properties;
using UnityEngine;
using Action = Unity.Behavior.Action;

[Serializable, GeneratePropertyBag]
[NodeDescription(name: "PlayerAttack", story: "[Self] Attack to [Target] if not [isSkillReady] and [isInAttackRange]", category: "Action", id: "6af060fcca8cec03ba035db04ac86992")]
public partial class PlayerAttackAction : Action
{
    [SerializeReference] public BlackboardVariable<GameObject> Self;
    [SerializeReference] public BlackboardVariable<GameObject> Target;
    [SerializeReference] public BlackboardVariable<bool> IsSkillReady;
    [SerializeReference] public BlackboardVariable<bool> IsInAttackRange;

    private PlayerController controller;
    private PlayerModel model;
    private BehaviorGraphAgent BGagent;
    private SPUM_Prefabs spumC;

    private float attackDelay;

    protected override Status OnStart()
    {
        controller = Self.Value.GetComponent<PlayerController>();
        model = Self.Value.GetComponent<PlayerModel>();
        BGagent = Self.Value.GetComponent<BehaviorGraphAgent>();
        spumC = controller.spumController;

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
        if (IsInAttackRange.Value == false)
        {
            return Status.Failure;
        }

        if (attackDelay > 0f)
        {
            attackDelay -= Time.deltaTime;
            return Status.Success;
        }

        if (Target.Value != null && attackDelay <= 0f)
        {
            Debug.Log("기본 공격 실행");
            IDamagable target = Target.Value.GetComponent<IDamagable>();
            if (target != null && attackDelay <= 0f)
            {
                spumC.PlayAnimation(PlayerState.ATTACK, 0);
                // 데미지 주기 - 기본공격 데미지 공식 넣어야 함
                target.TakeDamage(model.ExtAtk);
                attackDelay = 1f / model.modelSO.AtkSpeed;
                controller.skill2Count--;
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
        // 타겟이 없으면(전부 사망했다면)
        if (Target.Value == null)
        {
            BGagent.SetVariableValue<bool>("isTargetDetected", false);
        }
    }
}


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
    MonsterController mController;

    private float attackDelay;

    private float deadTargetDelay = 0.01f;
    private float deadTargetTimer = 0f;
    private bool isWaitingForDeadTarget = false;

    protected override Status OnStart()
    {
        if (Self?.Value == null) return Status.Failure;
        controller = Self.Value.GetComponent<PlayerController>();
        if (controller == null) return Status.Failure;
        model = Self.Value.GetComponent<PlayerModel>();
        BGagent = Self.Value.GetComponent<BehaviorGraphAgent>();
        spumC = controller.spumController;

        deadTargetTimer = 0f;
        isWaitingForDeadTarget = false;

        Target.Value = GetTarget();
        if (Target.Value != null)
        {
            mController = Target.Value.GetComponent<MonsterController>();
        }

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
            MonsterController monsterController = monster.GetComponent<MonsterController>();

            if (monsterController.IsDead) continue;

            if (distance < minDistance)
            {
                minDistance = distance;
                closest = monster;
            }
        }

        return closest;
    }

    private void FlipTowardsTarget()
    {
        if (Target.Value == null) return;

        Vector3 scale = Self.Value.transform.localScale;

        if (controller.movedRight)
        {
            scale.x = Target.Value.transform.position.x < Self.Value.transform.position.x ? -1 : 1;
        }
        else
        {
            scale.x = Target.Value.transform.position.x < Self.Value.transform.position.x ? 1 : -1;
        }

        Debug.Log($"방향전환 전: {Self.Value.transform.localScale}");
        Self.Value.transform.localScale = scale;
        Debug.Log($"방향전환 후: {Self.Value.transform.localScale}");
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
            return Status.Running;
        }

        // 타겟 유효성 재확인
        if (Target?.Value == null)
        {
            Target.Value = GetTarget();
            if (Target.Value == null)
            {
                BGagent?.SetVariableValue<bool>("isTargetDetected", false);
                return Status.Failure;
            }
            mController = Target.Value.GetComponent<MonsterController>();
        }

        // 타겟이 몬스터 컨트롤러를 가지고 있고 죽어있다면 0.1초 대기 후 다음 행동으로
        if (mController != null && mController.IsDead)
        {
            if (!isWaitingForDeadTarget)
            {
                // 대기 시작
                isWaitingForDeadTarget = true;
                deadTargetTimer = deadTargetDelay;
            }

            // 대기 중 타이머 감소
            deadTargetTimer -= Time.deltaTime;
            if (deadTargetTimer <= 0f)
            {
                // 대기 완료 -> 타겟 재탐지 후 행동 종료(성공으로 처리하여 다음 노드로 진행)
                Target.Value = GetTarget();
                isWaitingForDeadTarget = false;
                deadTargetTimer = 0f;

                // 새 타겟이 없으면 targetDetected=false
                if (Target.Value == null)
                    BGagent?.SetVariableValue<bool>("isTargetDetected", false);

                return Status.Failure;
            }

            // 아직 대기 중이면 계속 Running
            return Status.Running;
        }

        // 타겟이 유효하고 살아있다면 공격 시도
        if (Target.Value != null && attackDelay <= 0f)
        {
            IDamagable dam = Target.Value.GetComponent<IDamagable>();
            mController = Target.Value.GetComponent<MonsterController>();
            if (dam != null && mController != null && !mController.IsDead)
            {
                // 공격 실행
                FlipTowardsTarget();
                spumC.PlayAnimation(PlayerState.ATTACK, 0);

                float rawDamage = (float)(model.ExtAtk + model.InnAtk
                                 - (mController.Model.BaseModel.finalInnerDefense + mController.Model.BaseModel.finalOuterDefense));
                float damage = Math.Clamp(rawDamage, 1f, float.MaxValue);

                bool isCritical = UnityEngine.Random.value < controller.model.CritRate;
                if (isCritical) damage *= controller.model.CritDamage;

                dam.TakeDamage(damage);
                mController.isAttackedByNormalAttack = true;
                controller.damageDealt += damage;
                controller.synergyUI?.UpdateDamageUI();

                attackDelay = 1f / Mathf.Max(0.0001f, model.AttackSpeed);
                controller.skill2Count = Math.Max(0, controller.skill2Count - 1);

                return Status.Success;
            }
            else
            {
                // 공격 불가대상인 경우 재탐색 또는 대기
                Target.Value = GetTarget();
                return Status.Running;
            }
        }

        Target.Value = GetTarget();
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


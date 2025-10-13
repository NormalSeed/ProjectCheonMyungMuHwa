using System;
using Unity.Behavior;
using Unity.Properties;
using UnityEngine;
using UnityEngine.AI;
using Action = Unity.Behavior.Action;

[Serializable, GeneratePropertyBag]
[NodeDescription(name: "PlayerUseSkill", story: "[Self] Use Skill to [Target] if [isSkillReady] and [isInSkillRange]", category: "Action", id: "e6f708863bb84ccd760ad1f7e1b6bf1f")]
public partial class PlayerUseSkillAction : Action
{
    [SerializeReference] public BlackboardVariable<GameObject> Self;
    [SerializeReference] public BlackboardVariable<GameObject> Target;
    [SerializeReference] public BlackboardVariable<bool> IsSkillReady;
    [SerializeReference] public BlackboardVariable<bool> IsInSkillRange;

    private PlayerController controller;
    private BehaviorGraphAgent BGagent;
    private NavMeshAgent NMagent;
    private SkillSet skillSet;
    private MonsterController mController;

    private bool skillExecuted = false;

    // 선택: 스킬 탐색 비용이 크면 외부에서 관리하는 Monster 리스트 사용 권장
    private static GameObject[] cachedMonsters;

    protected override Status OnStart()
    {
        if (Self?.Value == null) return Status.Failure;

        controller = Self.Value.GetComponent<PlayerController>();
        if (controller == null) return Status.Failure;

        BGagent = Self.Value.GetComponent<BehaviorGraphAgent>();
        NMagent = Self.Value.GetComponent<NavMeshAgent>();

        if (controller.skillSet != null)
            skillSet = controller.skillSet.GetComponent<SkillSet>();
        else
            skillSet = null;

        skillExecuted = false;

        // 초기 타겟 안전하게 설정
        if (Target?.Value == null)
            Target.Value = GetTargetSafe();
        if (Target.Value != null)
            mController = Target.Value.GetComponent<MonsterController>();
        else
            mController = null;

        return Status.Running;
    }

    private GameObject GetTargetSafe()
    {
        // 가능하면 MonsterManager 같은 중앙 리스트를 사용하도록 변경하세요.
        // 일단은 FindGameObjectsWithTag을 사용하되 null/빈 배열 처리를 포함.
        GameObject[] monsters = GameObject.FindGameObjectsWithTag("Monster");
        if (monsters == null || monsters.Length == 0) return null;

        GameObject closest = null;
        float minDistSqr = float.MaxValue;
        Vector3 pos = Self.Value.transform.position;

        for (int i = 0; i < monsters.Length; i++)
        {
            MonsterController monsterController = monsters[i].GetComponent<MonsterController>();
            var m = monsters[i];
            if (m == null) continue;
            var mc = m.GetComponent<MonsterController>();
            if (mc != null && mc.IsDead) continue; // 이미 죽은 몬스터 건너뛰기

            float dSqr = (m.transform.position - pos).sqrMagnitude;
            if (dSqr < minDistSqr)
            {
                minDistSqr = dSqr;
                closest = m;
            }
        }

        return closest;
    }

    private void FlipTowardsTarget()
    {
        if (Target?.Value == null || Self?.Value == null) return;

        Vector3 scale = Self.Value.transform.localScale;

        if (controller.movedRight)
            scale.x = Target.Value.transform.position.x < Self.Value.transform.position.x ? -1 : 1;
        else
            scale.x = Target.Value.transform.position.x < Self.Value.transform.position.x ? 1 : -1;

        Self.Value.transform.localScale = scale;
    }

    protected override Status OnUpdate()
    {
        // 필수 전제 체크
        if (Self?.Value == null) return Status.Failure;
        if (IsSkillReady == null || IsInSkillRange == null) return Status.Failure;

        // 스킬 준비/사거리 조건이 만족되지 않으면 실패로 처리(흐름 설계에 따라 Running으로 바꿀 수 있음)
        if (!IsSkillReady.Value || !IsInSkillRange.Value)
            return Status.Failure;

        // 스킬셋이 아직 로드되지 않았다면 시도해서 가져오기
        if (skillSet == null && controller.skillSet != null)
            skillSet = controller.skillSet.GetComponent<SkillSet>();

        // 타겟 유효성 재확인
        if (Target?.Value == null)
        {
            Target.Value = GetTargetSafe();
            mController = Target.Value?.GetComponent<MonsterController>();
            if (Target.Value == null)
            {
                BGagent?.SetVariableValue<bool>("isTargetDetected", false);
                return Status.Failure;
            }
        }
        else
        {
            // 이미 타겟이 있고 MonsterController를 캐시하지 않았다면 시도
            if (mController == null)
                mController = Target.Value.GetComponent<MonsterController>();
        }

        // 만약 타겟이 죽어있다면 공격하지 않음 -> 재탐색하거나 Failure 처리
        if (mController != null && mController.IsDead)
        {
            // 타겟이 죽었으므로 즉시 타겟 재탐지하고 행동 종료(Success로 다음 노드 진행)
            Target.Value = GetTargetSafe();
            if (Target.Value == null)
                BGagent?.SetVariableValue<bool>("isTargetDetected", false);

            skillExecuted = false;
            return Status.Success;
        }

        // 이미 스킬을 실행했으면 스킬 재생 완료를 기다리기
        if (skillExecuted)
        {
            // 스킬이 재생 중이면 계속 Running, 재생이 끝났으면 Success
            if (skillSet != null && skillSet.isSkillPlaying)
                return Status.Running;

            // 스킬 재생이 끝난 상황: 상태 정리하고 Success 반환
            skillExecuted = false;
            return Status.Success;
        }

        // 스킬 실행 시도
        if (Target.Value != null && skillSet != null)
        {
            IDamagable dam = Target.Value.GetComponent<IDamagable>();
            if (dam == null)
            {
                // 공격 불가 대상이면 재탐색
                Target.Value = GetTargetSafe();
                return Status.Running;
            }

            // Skill1 우선, Skill2 다음 (원래 로직 유지)
            if (controller.isSkill1Ready)
            {
                NMagent?.ResetPath();
                FlipTowardsTarget();
                try
                {
                    skillSet.Skill1(Target.Value.transform);
                }
                catch (Exception ex)
                {
                    Debug.LogError("[PlayerUseSkillAction] Skill1 호출 중 예외: " + ex);
                    return Status.Failure;
                }

                controller.curCool = skillSet.skills != null && skillSet.skills.Count > 0
                    ? skillSet.skills[0].CoolTime
                    : controller.curCool;
                skillExecuted = true;
                return Status.Running;
            }
            else if (controller.isSkill2Ready)
            {
                NMagent?.ResetPath();
                FlipTowardsTarget();
                try
                {
                    skillSet.Skill2(Target.Value.transform);
                }
                catch (Exception ex)
                {
                    Debug.LogError("[PlayerUseSkillAction] Skill2 호출 중 예외: " + ex);
                    return Status.Failure;
                }

                controller.skill2Count = 5;
                skillExecuted = true;
                return Status.Running;
            }
        }

        // 이 지점에 도달하면 실행 조건은 만족했으나 실행 불가: 재탐색 시도
        Target.Value = GetTargetSafe();
        return Status.Running;
    }

    protected override void OnEnd()
    {
        skillExecuted = false;

        // 타겟 재탐지
        Target.Value = GetTargetSafe();
        if (Target.Value == null)
        {
            BGagent?.SetVariableValue<bool>("isTargetDetected", false);
        }
    }
}
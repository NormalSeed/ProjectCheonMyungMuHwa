using System;
using Unity.Behavior;
using Unity.Properties;
using UnityEngine;
using UnityEngine.AI;
using UnityEngine.InputSystem.XR;
using Action = Unity.Behavior.Action;

[Serializable, GeneratePropertyBag]
[NodeDescription(name: "PlayerChase", story: "[Self] Chases [Target] if [isTargetDetected] and [isInAttackRange]", category: "Action", id: "ee0e126e269beb9725b29f54f7c198ef")]
public partial class PlayerChaseAction : Action
{
    [SerializeReference] public BlackboardVariable<GameObject> Self;
    [SerializeReference] public BlackboardVariable<GameObject> Target;
    [SerializeReference] public BlackboardVariable<bool> IsTargetDetected;
    [SerializeReference] public BlackboardVariable<bool> IsInAttackRange;

    private NavMeshAgent agent;
    private PlayerModel model;
    private PlayerController controller;
    private float distance;
    private SPUM_Prefabs spumC;
    private MonsterController mController;

    // 목적지 재설정 최소 임계값 (너무 자주 SetDestination 호출을 막기 위함)
    private const float DEST_CHANGE_THRESHOLD_SQR = 0.01f;

    // 이전 목적지 캐시
    private Vector3 lastDestination = Vector3.positiveInfinity;

    protected override Status OnStart()
    {
        if (Self?.Value == null) return Status.Failure;

        agent = Self.Value.GetComponent<NavMeshAgent>();
        model = Self.Value.GetComponent<PlayerModel>();
        controller = Self.Value.GetComponent<PlayerController>();
        spumC = controller?.spumController;

        if (agent == null || model == null || controller == null)
            return Status.Failure;

        Self.Value.transform.localScale = Vector3.one;

        // 이동 애니메이션 재생
        spumC?.PlayAnimation(PlayerState.MOVE, 0);

        // 타겟 지정(가장 가까운 상대)
        Target.Value = GetTarget();
        mController = Target.Value.GetComponent<MonsterController>();
        // agent 속도 설정
        agent.speed = model.modelSO != null ? model.modelSO.MoveSpeed : agent.speed;

        // lastDestination 초기화
        lastDestination = Vector3.positiveInfinity;

        return Status.Running;
    }

    /// <summary>
    /// Monster 태그를 가진 오브젝트 중 거리가 가장 가까운 오브젝트를 반환하는 메서드
    /// </summary>
    /// <returns></returns>
    private GameObject GetTarget()
    {
        GameObject[] monsters = GameObject.FindGameObjectsWithTag("Monster");
        if (monsters == null || monsters.Length == 0) return null;

        GameObject closest = null;
        float minDistanceSqr = float.MaxValue;
        Vector3 selfPosition = Self.Value.transform.position;

        foreach (GameObject monster in monsters)
        {
            if (monster == null || !monster.activeInHierarchy) continue;
            var mCtrl = monster.GetComponent<MonsterController>();
            if (mCtrl != null && mCtrl.IsDead) continue;

            float dSqr = (monster.transform.position - selfPosition).sqrMagnitude;
            if (dSqr < minDistanceSqr)
            {
                minDistanceSqr = dSqr;
                closest = monster;
            }
        }

        if (closest != null)
        {
            controller.movedRight = Self.Value.transform.position.x < closest.transform.position.x;
        }

        return closest;
    }

    protected override Status OnUpdate()
    {
        // 기본 안전성 검사
        if (Self?.Value == null) return Status.Failure;
        if (agent == null || model == null || controller == null) return Status.Failure;

        // 타겟 유효성 확인
        if (Target?.Value == null || !Target.Value.activeInHierarchy || mController.IsDead == true)
        {
            // 재탐색 시도
            Target.Value = GetTarget();
            if (Target.Value == null)
            {
                IsTargetDetected.Value = false;
                IsInAttackRange.Value = false;
                if (agent.hasPath) agent.ResetPath();
                return Status.Failure;
            }
        }

        // 거리 계산: 3D로 정확하게, 필드에 저장(원형 유지)
        Vector3 toTarget = Target.Value.transform.position - Self.Value.transform.position;
        distance = Mathf.Sqrt(toTarget.sqrMagnitude);

        // 사정거리 안으로 들어오면 성공 반환
        float atkRange = model.modelSO != null ? model.modelSO.AtkRange : 0f;
        if (distance <= atkRange)
        {
            if (agent.hasPath) agent.ResetPath();
            IsInAttackRange.Value = true;
            return Status.Success;
        }

        // 사정거리 밖이면 계속 NavMesh를 이용해 경로 설정
        IsInAttackRange.Value = false;

        Vector3 dest = Target.Value.transform.position;

        // 목적지가 의미 있게 변경된 경우에만 SetDestination 호출 (빈번한 호출 방지)
        if (lastDestination == Vector3.positiveInfinity || (lastDestination - dest).sqrMagnitude > DEST_CHANGE_THRESHOLD_SQR)
        {
            // NavMesh 위에서 경로 계산이 가능하면 목적지 설정, 아니면 still try SetDestination (CalculatePath 실패할 때도 시도해 볼 수 있음)
            try
            {
                NavMeshPath path = new NavMeshPath();
                bool calc = agent.CalculatePath(dest, path);
                if (calc && path.status == NavMeshPathStatus.PathComplete)
                {
                    agent.SetDestination(dest);
                    lastDestination = dest;
                }
                else
                {
                    // CalculatePath가 실패하거나 경로가 불완전할 경우에도 SetDestination을 시도
                    agent.SetDestination(dest);
                    lastDestination = dest;
                }
            }
            catch
            {
                // SetDestination/CalculatePath에서 예외가 발생하면 안전하게 실패 처리 없이 Running 유지
                try { agent.SetDestination(dest); lastDestination = dest; } catch { /* swallow */ }
            }
        }

        // 정상적으로 추격 중이면 계속 Running 반환
        return Status.Running;
    }
}
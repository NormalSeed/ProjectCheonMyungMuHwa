using System;
using System.Collections.Generic;
using Unity.Behavior;
using Unity.Properties;
using UnityEngine;
using UnityEngine.AI;
using Action = Unity.Behavior.Action;

[Serializable, GeneratePropertyBag]
[NodeDescription(name: "PlayerMove", story: "[Self] move to [AlignPoint]", category: "Action", id: "ac6be75cade22ceed3bfae9a3e9af1c2")]
public partial class PlayerMoveAction : Action
{
    [SerializeReference] public BlackboardVariable<GameObject> Self;
    [SerializeReference] public BlackboardVariable<GameObject> AlignPoint;

    private PlayerController controller;
    private NavMeshAgent NMagent;
    private SPUM_Prefabs spumC;

    private AlignPoint alignPoint;
    private Transform targetPoint;

    private PartyManager partyManager;
    private InGameManager inGameManager;

    private BehaviorGraphAgent BGagent;
    private float detectInterval = 0.15f; // 탐색 간격(튜닝 가능)
    private float detectTimer = 0f;

    protected override Status OnStart()
    {
        controller = Self.Value.GetComponent<PlayerController>();
        NMagent = Self.Value.GetComponent<NavMeshAgent>();
        spumC = controller.spumController;
        BGagent = Self.Value.GetComponent<BehaviorGraphAgent>(); // 추가

        partyManager = PartyManager.Instance;
        inGameManager = InGameManager.Instance;

        Self.Value.transform.localScale = Vector3.one;

        spumC.PlayAnimation(PlayerState.MOVE, 0);

        controller.hasAligned = false;
        AlignPoint.Value = GameObject.FindGameObjectWithTag("AlignPoint");
        alignPoint = AlignPoint.Value.GetComponent<AlignPoint>();

        // 자신의 배치 번호에 따라 이동 포인트로 이동하는 기능 구현 필요
        // 이동 포인트의 position을 NavMeshAgent의 목적지로 설정
        targetPoint = GetMovePoint(alignPoint.alignPoints);
        NMagent.SetDestination(targetPoint.position); // 배치 번호를 바꾸면 이동할 포인트도 변경됨

        return Status.Running;
    }

    private Transform GetMovePoint(List<GameObject> points)
    {
        // 배치 번호에 따라 이동 포인트로 이동하는 로직 구현 필요
        Transform point = points[controller.partyNum].transform;
        if (Self.Value.transform.position.x < point.position.x)
        {
            controller.movedRight = true;
        }
        else
        {
            controller.movedRight = false;
        }

            Debug.Log($"이동 포인트 : {point.gameObject.name}");
        return point;
    }

    private GameObject GetClosestAliveMonsterWithinSearchRange()
    {
        // 필요하면 searchRange를 필드로 빼서 조절 가능
        float searchRange = 20f;
        GameObject[] monsters = GameObject.FindGameObjectsWithTag("Monster");
        if (monsters == null || monsters.Length == 0) return null;

        Vector3 pos = Self.Value.transform.position;
        float minSqr = float.MaxValue;
        GameObject closest = null;
        foreach (var m in monsters)
        {
            if (m == null || !m.activeInHierarchy) continue;
            var mc = m.GetComponent<MonsterController>();
            if (mc == null || mc.IsDead) continue;
            float dSqr = (m.transform.position - pos).sqrMagnitude;
            if (dSqr < minSqr && dSqr <= searchRange * searchRange)
            {
                minSqr = dSqr;
                closest = m;
            }
        }
        return closest;
    }

    protected override Status OnUpdate()
    {
        detectTimer -= Time.deltaTime;

        if (!NMagent.pathPending && NMagent.remainingDistance <= NMagent.stoppingDistance)
        {
            if (!NMagent.hasPath || NMagent.velocity.sqrMagnitude == 0f)
            {
                if (!controller.hasAligned)
                {
                    InGameManager.Instance.alignedNum.Value++;
                    controller.hasAligned = true;
                }

                int activeMemberCount = 0;
                var members = partyManager.MembersID;
                for (int i = 0; i < members.Count; i++)
                {
                    if (members[i] != null) activeMemberCount++;
                }

                // 모든 캐릭터가 정렬됐는지 확인
                if (InGameManager.Instance.alignedNum.Value >= activeMemberCount)//PartyManager.Instance.partyMembers.Count
                {
                    NMagent.ResetPath();
                    return Status.Success; // 모두 정렬 완료 -> Idle로 전환
                }

                return Status.Running; // 나만 도착했음 -> 대기
            }
        }

        if (detectTimer <= 0f)
        {
            detectTimer = detectInterval;
            // 간단한 근접 몬스터 탐색
            GameObject found = GetClosestAliveMonsterWithinSearchRange();
            if (found != null)
            {
                // Behavior Graph 변수로 타겟 존재 알림 또는 직접 상태 변경
                BGagent?.SetVariableValue<bool>("isTargetDetected", true);
                BGagent?.SetVariableValue<string>("CurState", "Attack"); // CurState 타입이 string일 때
                return Status.Failure; // 또는 Success: 트리 구조에 맞게 선택
            }
        }

        return Status.Running;
    }

    protected override void OnEnd()
    {
        Debug.Log($"현재 정렬된 플레이어 수 : {inGameManager.alignedNum.Value}");
    }
}


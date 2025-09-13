using System;
using System.Collections.Generic;
using System.Linq;
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
    private bool hasAligned = false;

    protected override Status OnStart()
    {
        hasAligned = false;

        controller = Self.Value.GetComponent<PlayerController>();
        NMagent = Self.Value.GetComponent<NavMeshAgent>();
        spumC = controller.spumController;

        spumC.PlayAnimation(PlayerState.MOVE, 0);

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

        Debug.Log($"이동 포인트 : {point.gameObject.name}");
        return point;
    }

    protected override Status OnUpdate()
    {
        if (!NMagent.pathPending && NMagent.remainingDistance <= NMagent.stoppingDistance)
        {
            if (!NMagent.hasPath || NMagent.velocity.sqrMagnitude == 0f)
            {
                if (!hasAligned)
                {
                    InGameManager.Instance.alignedNum.Value++;
                    hasAligned = true;
                }

                int activeMemberCount = PartyManager.Instance.MembersID.Count(member => member != null);

                // 모든 캐릭터가 정렬됐는지 확인
                if (InGameManager.Instance.alignedNum.Value >= activeMemberCount)//PartyManager.Instance.partyMembers.Count
                {
                    NMagent.ResetPath();
                    return Status.Success; // 모두 정렬 완료 -> Idle로 전환
                }

                return Status.Running; // 나만 도착했음 -> 대기
            }
        }

        return Status.Running;
    }

    protected override void OnEnd()
    {
        Debug.Log($"현재 정렬된 플레이어 수 : {InGameManager.Instance.alignedNum.Value}");
    }
}


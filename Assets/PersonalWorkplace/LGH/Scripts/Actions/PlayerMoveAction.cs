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
    private AlignPoint alignPoint;

    protected override Status OnStart()
    {
        controller = Self.Value.GetComponent<PlayerController>();
        NMagent = Self.Value.GetComponent<NavMeshAgent>();

        AlignPoint.Value = GameObject.FindGameObjectWithTag("AlignPoint");
        alignPoint = AlignPoint.Value.GetComponent<AlignPoint>();

        // 자신의 배치 번호에 따라 이동 포인트로 이동하는 기능 구현 필요
        // 이동 포인트의 position을 NavMeshAgent의 목적지로 설정
        NMagent.SetDestination(GetMovePoint(alignPoint.alignPoints).position); // 배치 번호를 바꾸면 이동할 포인트도 변경됨

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
        if (Vector2.Distance(Self.Value.transform.position, GetMovePoint(alignPoint.alignPoints).position) < 0.01f)
        {
            NMagent.ResetPath();
            return Status.Success;
        }
        return Status.Running;
    }

    protected override void OnEnd()
    {
        InGameManager.Instance.alignedNum.Value++;
    }
}


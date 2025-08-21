using System;
using System.Collections.Generic;
using Unity.Behavior;
using UnityEngine;
using Action = Unity.Behavior.Action;
using Unity.Properties;
using System.Linq;
using UnityEngine.AI;

[Serializable, GeneratePropertyBag]
[NodeDescription(name: "PlayerMove", story: "[Self] move to [AlignPoints]", category: "Action", id: "ac6be75cade22ceed3bfae9a3e9af1c2")]
public partial class PlayerMoveAction : Action
{
    [SerializeReference] public BlackboardVariable<GameObject> Self;
    [SerializeReference] public BlackboardVariable<List<GameObject>> AlignPoints;

    private NavMeshAgent NMagent;

    protected override Status OnStart()
    {
        NMagent = Self.Value.GetComponent<NavMeshAgent>();

        // AlignPoint 태그를 가진 이동 포인트를 검색해서
        GameObject[] foundPoints = GameObject.FindGameObjectsWithTag("AlignPoint");
        // AlignPoints 리스트에 넣어줌
        AlignPoints.Value = foundPoints.ToList();

        // 자신의 배치 번호에 따라 이동 포인트로 이동하는 기능 구현 필요
        // 이동 포인트의 position을 NavMeshAgent의 목적지로 설정
        NMagent.SetDestination(GetMovePoint(AlignPoints.Value).position); // 배치 번호를 바꾸면 이동할 포인트도 변경됨

        return Status.Running;
    }

    private Transform GetMovePoint(List<GameObject> points)
    {
        // 배치 번호에 따라 이동 포인트로 이동하는 로직 구현 필요
        return points[0].transform;
    }

    protected override Status OnUpdate()
    {
        if (Vector2.Distance(Self.Value.transform.position, GetMovePoint(AlignPoints.Value).position) < 0.1f)
        {
            NMagent.ResetPath();
            return Status.Success;
        }
        return Status.Running;
    }
}


using System;
using Unity.Behavior;
using UnityEngine;
using Action = Unity.Behavior.Action;
using Unity.Properties;

[Serializable, GeneratePropertyBag]
[NodeDescription(name: "PlayerChase", story: "[Self] Chases [Target]", category: "Action", id: "ee0e126e269beb9725b29f54f7c198ef")]
public partial class PlayerChaseAction : Action
{
    [SerializeReference] public BlackboardVariable<GameObject> Self;
    [SerializeReference] public BlackboardVariable<GameObject> Target;
    protected override Status OnStart()
    {
        // 타겟 지정(가장 가까운 상대)
        // Astar 알고리즘을 이용해 타겟을 목표 지점으로 설정
        return Status.Running;
    }

    protected override Status OnUpdate()
    {
        // 공격 사정거리 내로 들어오면 성공 반환
        //if ()
        //{
        //    return Status.Success;
        //}

        // 사정거리 밖이면 계속 실행
        return Status.Running;
    }
}


using System;
using Unity.Behavior;
using UnityEngine;
using Action = Unity.Behavior.Action;
using Unity.Properties;
using UnityEngine.AI;

[Serializable, GeneratePropertyBag]
[NodeDescription(name: "PlayerChase", story: "[Self] Chases [Target]", category: "Action", id: "ee0e126e269beb9725b29f54f7c198ef")]
public partial class PlayerChaseAction : Action
{
    [SerializeReference] public BlackboardVariable<GameObject> Self;
    [SerializeReference] public BlackboardVariable<GameObject> Target;

    private NavMeshAgent agent;
    private PlayerModel model;
    private float distance;
    
    protected override Status OnStart()
    {
        agent = Self.Value.GetComponent<NavMeshAgent>();
        model = Self.Value.GetComponent<PlayerModel>();
        // 타겟 지정(가장 가까운 상대)
        Target.Value = GetTarget();

        agent.speed = model.MoveSpeed;
        
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
        distance = Vector2.Distance(Self.Value.transform.position, Target.Value.transform.position);

        // 사정거리 안으로 들어오면 성공 반환
        if (distance <= model.AtkRange)
        {
            agent.ResetPath();
            return Status.Success;
        }
            

        // 사정거리 밖이면 계속 NavMesh를 이용해 경로 설정
        if (Target.Value != null)
        {
            agent.SetDestination(Target.Value.transform.position);
        }

        return Status.Running;
    }
}


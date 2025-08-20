using System;
using Unity.Behavior;
using UnityEngine;
using Action = Unity.Behavior.Action;
using Unity.Properties;

[Serializable, GeneratePropertyBag]
[NodeDescription(name: "PlayerUseSkill", story: "[Self] Use Skill to [Target]", category: "Action", id: "e6f708863bb84ccd760ad1f7e1b6bf1f")]
public partial class PlayerUseSkillAction : Action
{
    [SerializeReference] public BlackboardVariable<GameObject> Self;
    [SerializeReference] public BlackboardVariable<GameObject> Target;

    private PlayerController controller;

    protected override Status OnStart()
    {
        controller = Self.Value.GetComponent<PlayerController>();

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
        if (Target.Value != null)
        {
            Debug.Log("스킬 공격 실행");
        }

        return Status.Success;
    }

    protected override void OnEnd()
    {
    }
}


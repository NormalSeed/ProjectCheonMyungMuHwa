using System;
using Unity.Behavior;
using UnityEngine;
using Action = Unity.Behavior.Action;
using Unity.Properties;
using GooglePlayGames.BasicApi;
using System.Linq;
using System.Collections.Generic;

[Serializable, GeneratePropertyBag]
[NodeDescription(name: "DetectTargetPlayerAction", story: "[Self] detect nearby [Target] and set bool [IsTargetDetected] & [Controller] & [TargetCon]", category: "Action", id: "6ca15d30e1052d68a137bbfc3f902935")]
public partial class DetectTargetPlayerAction : Action
{
    [SerializeReference] public BlackboardVariable<GameObject> Self;
    [SerializeReference] public BlackboardVariable<GameObject> Target;
    [SerializeReference] public BlackboardVariable<bool> IsTargetDetected;
    [SerializeReference] public BlackboardVariable<MonsterController> Controller;
    [SerializeReference] public BlackboardVariable<PlayerController> TargetCon;
    private Vector3 selfPosition => Self.Value.transform.position;

    private float detectDelay = 0.5f;
    private float timer = 0f;

    protected override Status OnStart()
    {
        Controller.Value.OnIdle();

        return Status.Running;
    }

    /// <summary>
    /// Monster 태그를 가진 오브젝트 중 거리가 가장 가까운 오브젝트를 반환하는 메서드
    /// </summary>
    /// <returns></returns>
    private void GetTarget(ref BlackboardVariable<PlayerController> target)
    {
        List<PlayerController> list = new();
        foreach (GameObject go in GameObject.FindGameObjectsWithTag("Player"))
        {
            PlayerController c = go.GetComponent<PlayerController>();
            if (!c.isDead.Value) list.Add(c);
        }
        if (list.Count == 0)
        {
            return;
        }
        float minDistance = Mathf.Infinity;

        foreach (PlayerController monster in list)
        {
            float distance = Vector3.Distance(selfPosition, monster.transform.position);
            if (distance < minDistance)
            {
                minDistance = distance;
                target.Value = monster;
            }
        }
        IsTargetDetected.Value = true;
        return;
    }

    protected override Status OnUpdate()
    {
        if (timer <= 0f)
        {
            GetTarget(ref TargetCon);
            timer = detectDelay;
        }
        timer -= Time.deltaTime;
        return Status.Running;
    }
}


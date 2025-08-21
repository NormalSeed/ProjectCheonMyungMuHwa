using System;
using Unity.Behavior;
using UnityEngine;
using UnityEngine.AI;

public class Monster : MonoBehaviour
{

    private BehaviorGraphAgent agent;
    private NavMeshAgent nav;
    void Awake()
    {
        agent = GetComponent<BehaviorGraphAgent>();
        nav = GetComponent<NavMeshAgent>();
        nav.updateRotation = false;
        nav.updateUpAxis = false;
    }
}

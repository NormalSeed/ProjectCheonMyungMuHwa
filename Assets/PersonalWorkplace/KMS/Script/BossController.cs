using System;
using System.Collections;
using Unity.Behavior;
using UnityEngine;
using UnityEngine.AI;

public class BossController : MonsterController
{
    public override void OnAttack(GameObject me, IDamagable target)
    {

    }
    protected override void InitComponent()
    {
        hurtWfs = new WaitForSeconds(0.15f);
        navAgent = GetComponent<NavMeshAgent>();
        treeAgent = GetComponent<BehaviorGraphAgent>();
        Model = GetComponent<MonsterModel>();
        navAgent.updateRotation = false;
        navAgent.updateUpAxis = false;
        Model.CurHealth = new ObservableProperty<double>(10f);
    }
    public override void OnIdle()
    {
        
    }
    public override void OnMove()
    {
        
    }

    public override void OnDeath()
    {
        OnLifeEnded?.Invoke(this);
    }


}

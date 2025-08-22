using System;
using Unity.Behavior;
using UnityEngine;
using UnityEngine.AI;

public class MonsterController : MonoBehaviour, IDamagable
{

    [SerializeField] public SPUM_Prefabs Spum;
    [SerializeField] public MonsterModel Model;
    private BehaviorGraphAgent agent;
    private NavMeshAgent nav;
    [SerializeField] Coin[] coins;
    void Awake()
    {

        Spum = GetComponent<SPUM_Prefabs>();
        nav = GetComponent<NavMeshAgent>();
        agent = GetComponent<BehaviorGraphAgent>();
        nav.updateRotation = false;
        nav.updateUpAxis = false;
        Spum.OverrideControllerInit();
        Model.CurHealth = new ObservableProperty<double>(0f);


    }
    void OnEnable()
    {
        Model.CurHealth.Value = Model.HealthPoint;
        nav.speed = Model.MoveSpeed;
        agent.SetVariableValue<float>("AttackDistance", Model.AttackDistance);
        agent.SetVariableValue<float>("AttackDistanceWithClearance", Model.AttackDistanceWithClearance);
        agent.SetVariableValue<float>("AttackDelay", Model.AttackDelay);
        agent.SetVariableValue<float>("CurrentDistance", float.MaxValue);
        agent.SetVariableValue<MonsterController>("Controller", this);
    }

    public void BlastCoins()
    {
        foreach (Coin c in coins)
        {
            c.gameObject.SetActive(true);
        }
    }

    void Update()
    {
        if (Input.GetKeyDown(KeyCode.F5))
        {
            TakeDamage(10);
            Debug.Log(Model.CurHealth.Value);
        }
    }

    public void TakeDamage(double amount)
    {
        Model.CurHealth.Value -= amount;
    }
}

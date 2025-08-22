using System;
using Unity.Behavior;
using UnityEngine;

[Serializable, Unity.Properties.GeneratePropertyBag]
[Condition(name: "MonsterHealthCondition", story: "Check [Self] helath/ [Controller]", category: "Conditions", id: "bb9dcfd8ce30e641014da6515cf18654")]
public partial class MonsterHealthCondition : Condition
{
    [SerializeReference] public BlackboardVariable<GameObject> Self;
    [SerializeReference] public BlackboardVariable<MonsterController> Controller;

    public override bool IsTrue()
    {

        if (Controller.Value.Model.CurHealth.Value <= 0)
        {
            return true;
        }
        else
        {
            return false;
        }
    }
}

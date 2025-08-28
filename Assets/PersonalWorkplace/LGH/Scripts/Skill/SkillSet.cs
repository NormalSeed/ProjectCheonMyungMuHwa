using System.Collections.Generic;
using UnityEngine;

public class SkillSet : MonoBehaviour
{
    [SerializeField] public string SkillSetID;
    [SerializeField] public List<PlayerSkillSO> skills;

    protected ProjectileController pController;

    protected PlayerController controller;
    protected Transform parentTransform;

    public void Init(PlayerController controller)
    {
        pController = GetComponent<ProjectileController>();
        if (pController != null)
        {
            pController.Init();
        }
        
        this.controller = controller;
        parentTransform = this.controller.gameObject.transform;
    }

    public virtual void Skill1(Transform target)
    {

    }

    public virtual void Skill2(Transform target)
    {

    }
}

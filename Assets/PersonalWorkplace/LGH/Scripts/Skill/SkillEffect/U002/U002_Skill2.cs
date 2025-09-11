using System.Collections;
using UnityEngine;

public class U002_Skill2 : SkillEffect
{
    private Transform originalParent;
    protected override void Awake()
    {
        base.Awake();
        duration = 5f;
    }
    public void SetParent(Transform parent)
    {
        originalParent = parent;
    }

    private void OnEnable()
    {
        StartCoroutine(AutoDisable());
    }

    private void OnDisable()
    {
        transform.SetParent(originalParent);
        gameObject.SetActive(false);
    }

    private IEnumerator AutoDisable()
    {
        yield return new WaitForSeconds(duration);
        transform.SetParent(originalParent);
        gameObject.SetActive(false);
    }
}

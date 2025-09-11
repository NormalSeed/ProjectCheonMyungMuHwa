using System.Collections;
using UnityEngine;

public class NanGong_DestroyFist : SkillEffect
{
    private Transform originalParent;

    protected override void Awake()
    {
        base.Awake();
        duration = 1f;
    }

    private void OnEnable()
    {
        StartCoroutine(AutoDisable());
    }

    public void SetParent(Transform parent)
    {
        originalParent = parent;
    }

    private IEnumerator AutoDisable()
    {
        yield return new WaitForSeconds(duration);
        transform.SetParent(originalParent);
        gameObject.SetActive(false);
    }
}

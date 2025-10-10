using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class UIBase : MonoBehaviour
{
    public virtual void SetShow()
    {
        gameObject.SetActive(true);
    }

    public virtual void SetHide()
    {
        gameObject.SetActive(false);
    }

    public virtual void RefreshUI() { /* Do Nothing */}
}

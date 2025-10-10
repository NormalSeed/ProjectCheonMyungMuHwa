using System;
using UnityEngine;
using UnityEngine.UI;

public enum ShopUIType
{
  Currency,
  Special,
  Package,
  SeasonPass
}

[Serializable]
public struct ShopUIAndButton
{
  public UIBase UI;
  public ShopUISelectButton btn;
}

public class ShopUI : UIBase
{
  [SerializeField] ShopUIAndButton[] uiAndBtn;
  private ShopUIAndButton current;

  void Awake()
  {
    foreach (ShopUIAndButton a in uiAndBtn)
    {
      a.btn.OnClick.AddListener(() => ActiveUI(a));
    }
  }

  public override void SetShow()
  {
    base.SetShow();
    if (current.Equals(default(ShopUIAndButton)))
    {
      ShopUIAndButton unb = uiAndBtn[0];
      unb.UI.SetShow();
      unb.btn.ActiveButton();
      current = unb;
    }
  }

  private void ActiveUI(ShopUIAndButton unb)
  {
    if (unb.UI == current.UI)
    {
      unb.UI.RefreshUI();
      return;
    }
    DisableCurrenctUI();
    unb.UI.SetShow();
    unb.btn.ActiveButton();
    current = unb;
  }
  private void DisableCurrenctUI()
  {
    current.UI.SetHide();
    current.btn.InactiveButton();
  }

  void OnDestroy()
  {
    foreach (ShopUIAndButton a in uiAndBtn)
    {
      a.btn.OnClick.RemoveListener(() => ActiveUI(a));
    }
  }
}

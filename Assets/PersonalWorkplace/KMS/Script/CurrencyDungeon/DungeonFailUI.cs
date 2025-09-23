using System;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.UI;

public class DungeonFailUI : UIBase
{
  [SerializeField] Button[] buttons;
  [SerializeField] Button screen;

  [SerializeField] FadeCanvas fade;

  public void SetActionToButton(int index, Action act)
  {
    UnityAction action = new UnityAction(act);
    buttons[index].onClick.AddListener(action);
  }
  public void SetActionToScreen(Action act)
  {
    UnityAction action = new UnityAction(act);
    screen.onClick.AddListener(action);
  }

  public void LoadScene(string id)
  {
    fade.FadeOutAndLoadScene(id, 1.5f);
  }

  void OnEnable()
  {
    foreach (Button b in buttons)
    {
      b.onClick.RemoveAllListeners();
    }
    screen.onClick.RemoveAllListeners(); 
  }
}

public partial class PopupManager
{
  public DungeonFailUI ShowDungeonFailPopup()
  {
    if (!_popupDict.TryGetValue(PopupType.DungeonFail, out var uiBase) || uiBase == null)
    {
      Debug.LogWarning("[PopupManager] 팝업이 등록되지 않았습니다.");
      return null;
    }
    if (uiBase is DungeonFailUI dungeon)
    {
      dungeon.SetShow();
      return dungeon;
    }
    return null;
  }
}

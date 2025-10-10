using System;
using TMPro;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.UI;

public class DungeonFailUI : UIBase
{
  [SerializeField] Button[] buttons;
  [SerializeField] Button screen;
  [SerializeField] UnityAction OnTimeOver;
  [SerializeField] TMP_Text message;
  private bool isStopped;
  private float currentTime;

  private float initTime = 5f;
  public void SetActionToButton(int index, UnityAction act)
  {
    buttons[index].onClick.AddListener(act);
  }
  public void SetActionToScreen(UnityAction act)
  {
    screen.onClick.AddListener(act);
  }
  public void SetActionToTimeOut(UnityAction act)
  {
    OnTimeOver += act;
  }

  void OnEnable()
  {
    foreach (Button b in buttons)
    {
      b.onClick.RemoveAllListeners();
    }
    screen.onClick.RemoveAllListeners();
    OnTimeOver = null;
    isStopped = false;
    currentTime = initTime;
  }
  void Update()
  {
    if (isStopped) return;
    if (currentTime <= 0)
    {
      OnTimeOver?.Invoke();
      isStopped = true;
      return;
    }
    currentTime -= Time.deltaTime;
    message.text = $"{(int)currentTime + 1} 초 후 자동으로 다시 시작...";
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

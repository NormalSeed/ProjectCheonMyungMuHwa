using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class HeroParty : UIBase
{
    [Header("Button")]
    [SerializeField] private Button saveSettingButton;
    [SerializeField] private Button autoSetButton;

    public List<GameObject> partyMembers = new List<GameObject>();

    #region Unity LifeCycle
    private void OnEnable()
    {
        saveSettingButton.onClick.AddListener(SaveHeroSetting);
        autoSetButton.onClick.AddListener(AutoSet);
    }

    private void OnDisable()
    {
        saveSettingButton.onClick.RemoveListener(SaveHeroSetting);
        autoSetButton.onClick.RemoveListener(AutoSet);
    }
    #endregion

    #region OnClick
    private void SaveHeroSetting()
    {

        SetHide();
    }
    private void AutoSet()
    {

    }
    #endregion

    #region private
    public void AddMember(GameObject member)
    {
        if (partyMembers.Count < 5)
        {
            partyMembers.Add(member);
        }
        else
        {
            Debug.Log("파티가 가득 찼습니다!");
        }
    }
    #endregion
}

using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class HeroPartySettingUI : UIBase
{
    [Header("Button")]
    [SerializeField] private Button saveSettingButton;
    [SerializeField] private Button autoSetButton;


    public bool IsPartySettingNow = false;

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
        /*  선택된 멤버를 추가하는 기능  */
        SetHide();
    }
    private void AutoSet()
    {
        /*  멤버를 자동으로 선택하여 추가하는 기능ㄴ  */
    }
    #endregion
}

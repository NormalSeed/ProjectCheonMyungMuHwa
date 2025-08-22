using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class HeroPartySetting : UIBase
{
    [Header("Button")]
    [SerializeField] private Button saveSettingButton;
    [SerializeField] private Button autoSetButton;

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

    }
    #endregion
}

using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using TMPro;
using UnityEngine;
using UnityEngine.AddressableAssets;
using UnityEngine.ResourceManagement.AsyncOperations;
using UnityEngine.UI;

public class SummonResultUI : UIBase
{
    [Header("Button")]
    [SerializeField] private Button resultButton;
    
    [Header("Pool")]
    [SerializeField] private GachaCardPoolManager poolManager;

    [Header("Panel")]
    [SerializeField] private SummonEquipUI SummonEquipUI;
    [SerializeField] private SummonHeroUI SummonHeroUI;
    [SerializeField] private SummonPetUI SummonPetUI;

    #region Unity LifeCycle
    private void OnEnable()
    {
        Init();
    }

    private void OnDisable()
    {
        resultButton.onClick.RemoveListener(OnClickResult);
    }
    #endregion

    #region Init
    private void Init()
    {
        resultButton.onClick.AddListener(OnClickResult);
    }

    private void OnClickResult()
    {
        this.gameObject.SetActive(false);
    }
    #endregion

    #region Private
    /// <summary>
    /// 소환 결과를 보여줍니다.
    /// </summary>
    /// <param name="times"></param>
    public async void ShowSummonResult(List<CardInfo> results)
    {
        gameObject.SetActive(true);
        poolManager.ReturnAll();

        for (int i = 0; i < results.Count; i++)
        {
            var info = results[i];
            var card = poolManager.GetCard();
            var setting = card.GetComponent<CardSetting>();

            setting.chardata = info;
            card.transform.SetAsLastSibling();
            card.SetActive(true);

            // 연출 간격 조정
            await Task.Delay(100);
        }
        SummonHeroUI.HandleGachaCompleted();
    }
    /// <summary>
    /// 장비 소환 결과를 보여줍니다.
    /// </summary>
    /// <param name="results">소환된 장비 리스트</param>
    public async void ShowSummonResult(List<EquipmentInstance> results)
    {
        gameObject.SetActive(true);
        poolManager.ReturnAll();

        for (int i = 0; i < results.Count; i++)
        {
            var equip = results[i];
            var card = poolManager.GetCard();
            //var setting = card.GetComponent<EquipmentCardSetting>();

            //setting.SetData(equip); // 장비 데이터 설정
            card.transform.SetAsLastSibling();
            card.SetActive(true);

            await Task.Delay(100); // 연출 간격
        }

        SummonEquipUI.HandleGachaCompleted();
    }
    #endregion
}

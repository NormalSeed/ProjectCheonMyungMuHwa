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
        Debug.Log($"[ShowSummonResult] 장비 결과 표시 시작 - 총 {results.Count}개");
        gameObject.SetActive(true);
        poolManager.ReturnAll();
        Debug.Log("[ShowSummonResult] 카드 풀 초기화 완료");

        for (int i = 0; i < results.Count; i++)
        {
            var equip = results[i];
            Debug.Log($"[ShowSummonResult] 카드 생성 시작 - {i + 1}/{results.Count}, 장비: {equip.templateID}, 레어도: {equip.rarity}, Lv.{equip.level}");

            var card = poolManager.GetCard();
            var display = card.GetComponent<EquipmentCardDisplay>();

            if (display != null)
            {
                display.SetData(equip);
                Debug.Log($"[ShowSummonResult] 카드 데이터 설정 완료 - {equip.templateID}");
            }
            else
            {
                Debug.LogWarning($"[ShowSummonResult] 카드에 EquipmentCardDisplay가 없습니다 - 카드 인덱스: {i}");
            }

            card.transform.SetAsLastSibling();
            card.SetActive(true);
            Debug.Log($"[ShowSummonResult] 카드 활성화 완료 - {card.name}");

            await Task.Delay(100);
        }
        SummonEquipUI.HandleGachaCompleted();
        Debug.Log("[ShowSummonResult] 모든 카드 표시 완료 - Gacha 처리 종료");
    }
    #endregion
}

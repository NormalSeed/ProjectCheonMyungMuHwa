using System;
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
    private CardInfo cardInfo;

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
    public async void ShowSummonResult(int times)
    {
        // (선택) 이전 결과 모두 리셋
        poolManager.ReturnAll();

        for (int i = 0; i < times; i++)
        {
            CardInfo cardInfo = await LoadCardInfo();
            GameObject card = poolManager.GetCard();

            var setting = card.GetComponent<CardSetting>();
            setting.chardata = cardInfo;
            card.transform.SetAsLastSibling();
            card.SetActive(true);

            await Task.Delay(TimeSpan.FromSeconds(0.01f));
        }
    }

    private async Task<CardInfo> LoadCardInfo()
    {
        var handle = Addressables.LoadAssetAsync<CardInfo>("C001CardInfo");
        await handle.Task;
        if (handle.Status == AsyncOperationStatus.Succeeded)
            return handle.Result;

        throw new InvalidOperationException("[LoadCardInfo] : 카드정보 불러오기 실패");
    }
    #endregion
}

using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class SummonResultUI : UIBase
{
    [Header("Button")]
    [SerializeField] private Button resultButton;

    [Header("SummonResult")]
    [SerializeField] private Transform resultContents;


    private void OnEnable()
    {
        Init();
    }

    private void OnDisable()
    {
        resultButton.onClick.RemoveListener(OnClickResult);
    }
    private void Init()
    {
        resultButton.onClick.AddListener(OnClickResult);
    }

    private void OnClickResult()
    {
        SetHide();
    }


    /// <summary>
    /// 소환 결과를 보여줍니다.
    /// </summary>
    /// <param name="times"></param>
    private void ShowSummonResult(int times)
    {
        for (int i = 0; i > times; i++)
        {
            /* TODO : 소환 결과 출력하기 */
        }
    }
}

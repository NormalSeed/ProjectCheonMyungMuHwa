using System.Collections.Generic;
using UnityEngine;

public class RouletteUI : UIBase
{

    public Dictionary<CurrencyType, BigCurrency> Rewards { get; set; }
    [SerializeField] Roulette roulette;

    private float multiplier;

    private Dictionary<float, float> table = new Dictionary<float, float>()
    {
        {0.75f, 1.2f},
        {0.2f, 1.5f},
        {0.05f, 2.0f}
    };



    private void SetRandomMultiplier()
    {
        float r = Random.value;
        float sum = 0;
        foreach (var v in table)
        {
            sum += v.Key;
            if (r <= sum)
            {
                multiplier = v.Value;
                break;
            }
        }
    }

    private void SetRandomTargetDegree()
    {
        
    }
}
public partial class PopupManager
{
    public RouletteUI ShowRoulettePopup()
    {
        if (!_popupDict.TryGetValue(PopupType.Roulette, out var uiBase) || uiBase == null)
        {
            Debug.LogWarning("[PopupManager] 팝업이 등록되지 않았습니다.");
            return null;
        }
        if (uiBase is RouletteUI ui)
        {
            ui.SetShow();
            return ui;
        }
        Debug.LogWarning("오류");
        return null;
    }
}

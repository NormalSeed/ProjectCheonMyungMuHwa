using System.Collections.Generic;
using Unity.VisualScripting;
using UnityEngine;
using UnityEngine.UI;

public class RouletteUI : UIBase
{

    public Dictionary<CurrencyType, BigCurrency> Rewards { get; set; }
    [SerializeField] Roulette roulette;

    [SerializeField] Button screen;

    private float multiplier;

    private float targetDegree;

    private List<(float deg, float mult)> table = new List<(float deg, float mult)>()
    {
        (18f, 2.0f),
        (90f, 1.2f),
        (36f, 1.5f),
        (90f, 1.2f),
        (36f, 1.5f),
        (90f, 1.2f),
    };

    void Awake()
    {
        screen.onClick.RemoveAllListeners();
        screen.onClick.AddListener(StartRoulette);
        roulette.OnEnd = null;
        roulette.OnEnd = OnRouletteEnd;
    }



    private void GetRandomValues()
    {
        targetDegree = Random.Range(0, 360f);
        float sum = 0;
        foreach ((float deg, float mult) v in table)
        {
            sum += v.deg;
            if (targetDegree <= sum)
            {
                multiplier = v.mult;
                break;
            }
        }
    }

    public void StartRoulette()
    {
        GetRandomValues();
        roulette.StartRoulette(targetDegree);
        screen.onClick.RemoveAllListeners();
        Debug.Log($"<color=yellow> {multiplier}");
    }
    public void OnRouletteEnd()
    {
        screen.onClick.RemoveAllListeners();
        screen.onClick.AddListener(OpenRewardPopup);
    }
    public void OpenRewardPopup()
    {
        screen.onClick.RemoveAllListeners();
        List<ItemData> datas = new();
        List<BigCurrency> curs = new();
        foreach (var r in Rewards)
        {
            ItemData item = null;
            BigCurrency c = r.Value;
            c *= multiplier;
            switch (r.Key)
            {
                case CurrencyType.Gold:
                    item = new ItemData(11002, "", "", "GoldImage", true, ItemType.Currency); break;
                case CurrencyType.Soul:
                    item = new ItemData(11003, "", "", "SoulImage", true, ItemType.Currency); break;
                case CurrencyType.SpiritStone:
                    item = new ItemData(11004, "", "", "SpiritImage", true, ItemType.Currency); break;
                case CurrencyType.SummonTicket:
                    item = new ItemData(11006, "", "", "HeroTicketImage", true, ItemType.Currency); break; //TODO 해당하는 어드렛서블 이미지등록
                case CurrencyType.EquipmentSummonTicket:
                    item = new ItemData(11011, "", "", "EquipTicketImage", true, ItemType.Currency); break; //TODO 해당하는 어드렛서블 이미지등록
            }
            datas.Add(item);
            curs.Add(c);
        }
        PopupManager.Instance.ShowRewardPopup(datas, curs, true, multiplier);
        SetHide();
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

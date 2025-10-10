using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.UI;
using VContainer;

public class DecompositionEquipment : MonoBehaviour
{
    [Inject] private EquipmentService equipmentService;
    [Inject] private EquipmentManager equipmentManager;

    [Header("Button")]
    [SerializeField] private Button normalButton;       // 노말 일괄
    [SerializeField] private Button rareButton;         // 희귀 일괄
    [SerializeField] private Button epicButton;         // 특급 일괄
    [SerializeField] private Button exitButton;         // 나가기
    [SerializeField] private Button submitButton;       // 분해하기

    [Header("Text")]
    [SerializeField] private TextMeshProUGUI equipCount;    // 장비 개수
    [SerializeField] private TextMeshProUGUI resultGrind;    // 획득 연마석

    [Header("Pool")]
    [SerializeField] private GachaCardPoolManager cardPoolManager;  // 풀메니저

    [Header("Panel")]
    [SerializeField] private EquipmentItemList equipmentItemList; // 아이템 인벤토리 창

    public List<DecompositionButton> activeEquipButtons = new();    // 분해 선택 버튼
    public List<DecompositionButton> selectedEquipButtons = new();  // 선택된 장비버튼 리스트
    public List<EquipmentInstance> selectedEquip = new();  // 선택된 장비 리스트

    private BigCurrency currency;

    #region Unity
    private void OnEnable()
    {
        Init();

    }

    private void OnDisable()
    {
        normalButton.onClick.RemoveListener(OnClickNormal);
        rareButton.onClick.RemoveListener(OnClickRare);
        epicButton.onClick.RemoveListener(OnClickEpic);
        exitButton.onClick.RemoveListener(OnClickExit);
        submitButton.onClick.RemoveListener(OnClickSubmit);
        this.gameObject.SetActive(false);
    }

    #endregion

    private void Init()
    {
        normalButton.onClick.AddListener(OnClickNormal);
        rareButton.onClick.AddListener(OnClickRare);
        epicButton.onClick.AddListener(OnClickEpic);
        exitButton.onClick.AddListener(OnClickExit);
        submitButton.onClick.AddListener(OnClickSubmit);
    }

    #region OnClick
    private void OnClickNormal()
    {
        SelectEquipmentsByRarity(RarityType.Normal);
    }

    private void OnClickRare()
    {
        SelectEquipmentsByRarity(RarityType.Rare);
    }

    private void OnClickEpic()
    {
        SelectEquipmentsByRarity(RarityType.Epic);
    }

    private void OnClickExit()
    {
        this.gameObject.SetActive(false);
        AudioManager.Instance.PlaySound("5. 팝업 닫을 때 사운드");
    }
    private void OnClickSubmit()
    {
        //  연마석 지급
        CurrencyManager.Instance.Add(CurrencyType.GrindingStone, currency);

        // 1. 실제 장비 데이터에서 제거
        equipmentManager.DelectEquipmentsByList(selectedEquip);

        // 2. 선택 리스트 초기화
        selectedEquipButtons.Clear();
        selectedEquip.Clear();

        // 3. UI 갱신
        ShowEquipmentListByEquip();
        equipmentItemList.ShowEquipmentListByTemplateID(equipmentItemList.thisTemplateID);
        currency = BigCurrency.FromBaseAmount(CalculateTotalGrindingStone(selectedEquip));
        resultGrind.text = currency.ToString();
    }


    #endregion

    #region Private

    #region events
    private void HandleSelectChanged(DecompositionButton button, bool isSelected)
    {
        if (isSelected)
        {
            if (!selectedEquipButtons.Contains(button))
            {
                selectedEquipButtons.Add(button);
                selectedEquip.Add(button.equipmentCardDisplay.GetEquipment());
            }
        }
        else
        {
            if (selectedEquipButtons.Contains(button))
            {
                selectedEquipButtons.Remove(button);
                selectedEquip.Remove(button.equipmentCardDisplay.GetEquipment());
            }
        }
        // UI 업데이트 예시
        currency = BigCurrency.FromBaseAmount(CalculateTotalGrindingStone(selectedEquip));
        resultGrind.text = $"분해 시 획득 연마석 : {currency.ToString()} 개"; // 선택된 장비에 따라 계산 필요
    }
    #endregion
    private void SelectEquipmentsByRarity(RarityType maxRarity)
    {
        foreach (var button in activeEquipButtons)
        {
            var equip = button.equipmentCardDisplay.GetEquipment();
            if (equip != null && equip.rarity <= maxRarity)
            {
                if (!button.IsSelected)
                {
                    button.ToggleSelect(); // 선택
                }
            }
            else
            {
                if (button.IsSelected)
                {
                    button.ToggleSelect(); // 선택 해제
                }
            }
        }
    }
    // 분해 시 연마석 계산
    private int GetDecompositionGrindingStone(EquipmentInstance equip)
    {
        int baseValue = 0;

        switch (equip.rarity)
        {
            case RarityType.Normal:
                baseValue = 20;
                break;
            case RarityType.Rare:
                baseValue = 40;
                break;
            case RarityType.Epic:
                baseValue = 60;
                break;
            case RarityType.Unique:
                baseValue = 80;
                break;
        }

        return baseValue * equip.level;
    }
    public int CalculateTotalGrindingStone(List<EquipmentInstance> selectedEquip)
    {
        int total = 0;
        foreach (var equip in selectedEquip)
        {
            total += GetDecompositionGrindingStone(equip);
        }
        return total;
    }
    #endregion

    #region Public
    public void ShowEquipmentListByEquip()
    {
        cardPoolManager.ReturnAll(); // 기존 카드 초기화
        activeEquipButtons.Clear();

        // 착용되지 않은 장비만 필터링
        var filtered = equipmentManager.allEquipments.FindAll(e => !e.isEquipped);

        Debug.Log($"[ShowEquipmentListByEquip] 미착용 장비 수: {filtered.Count}");
        foreach (var equip in filtered)
        {
            var card = cardPoolManager.GetCard();
            var display = card.GetComponentInChildren<EquipmentCardDisplay>();
            var button = card.GetComponent<DecompositionButton>();

            if (display != null) display.SetData(equip);
            if (button != null)
            {
                button.Init(equip);
                button.onSelectChanged = HandleSelectChanged; // 이벤트 연결
                activeEquipButtons.Add(button);
            }

            card.transform.SetAsLastSibling();
            card.SetActive(true);
        }

        // UI 텍스트 업데이트
        equipCount.text = $"{filtered.Count.ToString()} / 300 개";
        resultGrind.text = $"분해 시 획득 연마석 : {currency.ToString()} 개"; // 선택된 장비에 따라 계산 필요
    }
    #endregion
}

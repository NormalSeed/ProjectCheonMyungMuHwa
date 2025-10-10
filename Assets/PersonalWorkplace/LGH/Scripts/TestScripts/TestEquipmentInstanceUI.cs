using TMPro;
using UnityEngine;
using UnityEngine.UI;
using VContainer;

public class TestEquipmentInstanceUI : MonoBehaviour
{
    [Inject]
    private EquipmentService service;

    public Button equipButton;

    public TextMeshProUGUI nameText;
    public TextMeshProUGUI levelText;

    private EquipmentInstance instance;
    private string charID;

    private void Start()
    {
        equipButton = GetComponent<Button>();
        equipButton.onClick.AddListener(OnEquipButtonClicked);

        // 임시로 C001 캐릭터에 장착
        charID = "C001";
    }

    public void Bind(EquipmentInstance data)
    {
        instance = data;
        nameText.text = data.template.name;
        levelText.text = $"Lv.{data.level}";
    }

    public EquipmentInstance GetInstance() => instance;

    public void OnEquipButtonClicked()
    {
        var character = service.GetCharacter(charID);
        if (character == null)
        {
            Debug.LogWarning($"캐릭터 {charID}를 찾을 수 없습니다.");
            return;
        }

        var slot = character.slots[instance.equipmentType];

        // 이미 해당 슬롯에 이 장비가 장착되어 있다면 해제
        if (slot.equippedItem == instance && instance.isEquipped)
        {
            service.UnequipFromCharacter(charID, instance.equipmentType);
            Debug.Log($"장비 {instance.templateID} 해제됨");
        }
        else
        {
            // 다른 장비가 장착되어 있거나 비어 있다면 장착
            service.EquipToCharacter(charID, instance);
            Debug.Log($"장비 {instance.templateID} 장착됨");
        }
    }
}

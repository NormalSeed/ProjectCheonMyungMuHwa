using UnityEngine;
using UnityEngine.UI;
using VContainer;

public class EquipmentChange : MonoBehaviour
{
    [Inject] private EquipmentService equipmentService;
    [Inject] private EquipmentManager equipmentManager;

    [Header("EquipmentCardDisplay")]
    [SerializeField] private EquipmentCardDisplay equipmentImage;
    [SerializeField] private EquipmentInfoPanel infoPanel;

    [Header("Button")]
    [SerializeField] private Button submitButton;
    [SerializeField] private Button cancelButton;


    private EquipmentInstance instance;     // 판넬의 장비

    private string thisCharId;              // 새로 장착할 대상 영웅 ID
    private string oldCharID;               // 기존 장착중인 대상 영웅 ID
    private string instanceID;              // 장비 ID

    private void OnEnable()
    {
        Init();   
    }

    private void OnDisable()
    {
        submitButton.onClick.RemoveListener(OnClickSubmit);
        cancelButton.onClick.RemoveListener(OnClickCancle);
    }

    private void Init()
    {
        instance = infoPanel.instance;
        instanceID = infoPanel.instanceID;
        thisCharId = infoPanel.charId;
        oldCharID = instance.charID;
        equipmentImage.SetData(instance);
        submitButton.onClick.AddListener(OnClickSubmit);
        cancelButton.onClick.AddListener(OnClickCancle);
    }

    private void OnClickSubmit()
    {
        ChangeHeroEquipment();
        this.gameObject.SetActive(false);
    }
    private void OnClickCancle()
    {
        this.gameObject.SetActive(false);
    }

    private void ChangeHeroEquipment()
    {
        // 3. 기존 캐릭터에서 장비 해제
        if (!string.IsNullOrEmpty(oldCharID) && oldCharID != thisCharId)
        {
            Debug.LogWarning($"장비 ID {instance.instanceID}");
            Debug.LogWarning($"장비 장착 여부 : {instance.isEquipped}");
            Debug.LogWarning($"장비중 {oldCharID}, 신규 장착 대상 {thisCharId}");
            instance.isEquipped = false;
            instance.charID = null;
            if(equipmentService == null)
            {
                Debug.LogWarning("equipmentService 등록안됨");
            }
            equipmentService.UnequipFromUnactivatedCharacter(oldCharID, instance);
            equipmentService.UnequipFromCharacter(oldCharID, instance.equipmentType);

            Debug.Log($"[ChangeHeroEquipment] 기존 캐릭터 {oldCharID}에서 장비 {instanceID} 해제됨");
        }

        // 4. 새 캐릭터에게 장비 장착
        instance.isEquipped = true;
        instance.charID = thisCharId;
        equipmentService.EquipToUnactivatedCharacter(thisCharId, instance);
        equipmentService.EquipToCharacter(thisCharId, instance);

        Debug.Log($"[ChangeHeroEquipment] 새 캐릭터 {thisCharId}에게 장비 {instanceID} 장착됨");

        // 5. 장비 매니저 및 UI 갱신
        HeroDataManager.Instance.ApplyHeorStats(instance, thisCharId);
        HeroDataManager.Instance.SaveHeroData(thisCharId);
        StatModifierManager.ApplyToCard(infoPanel.HeroInfo.heroData.cardInfo);
        infoPanel.HeroInfo.Init();
        infoPanel.HeroInfo.RefreshHeroUI();
        infoPanel.SetPanelText();
        infoPanel.RefreshEquipCardUI();
    }
}

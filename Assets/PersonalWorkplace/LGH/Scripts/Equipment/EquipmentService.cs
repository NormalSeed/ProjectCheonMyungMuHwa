using System.Collections.Generic;
using System.Linq;
using Unity.VisualScripting;
using UnityEngine;

public class EquipmentService
{
    private readonly EquipmentManager equipmentManager;
    private readonly List<CharacterEquipment> activeCharacters = new();

    public EquipmentService(EquipmentManager equipmentManager)
    {
        this.equipmentManager = equipmentManager;
    }

    /// <summary>
    /// 캐릭터가 팀에 편성돼 활성화 됐을 때 activeCharacters 리스트에 추가하는 메서드
    /// </summary>
    /// <param name="character"></param>
    public void RegisterCharacter(CharacterEquipment character)
    {
        if (!activeCharacters.Contains(character))
            activeCharacters.Add(character);
    }

    /// <summary>
    /// charID를 기준으로 해당 캐릭터의 CharacterEquipment를 불러오는 메서드
    /// </summary>
    /// <param name="charID"></param>
    /// <returns></returns>
    public CharacterEquipment GetCharacter(string charID)
    {
        return activeCharacters.FirstOrDefault(c => c.charID == charID);
    }

    /// <summary>
    /// 장비 획득 메서드. templateID를 기준으로 EquipmentSO 템플릿을 받아오고 희귀도와 레벨을 받아와 획득 처리.
    /// </summary>
    /// <param name="templateID"></param>
    /// <param name="rarity"></param>
    /// <param name="level"></param>
    /// <returns></returns>
    public EquipmentInstance AcquireEquipment(string templateID, RarityType rarity, int level = 1)
    {
        var template = equipmentManager.allTemplates.Find(t => t.templateID == templateID);
        if (template == null)
        {
            Debug.LogWarning($"템플릿을 찾을 수 없습니다.");
            return null;
        }

        var instance = new EquipmentInstance
        {
            instanceID = System.Guid.NewGuid().ToString(),
            templateID = templateID,
            equipmentType = template.equipmentType,
            rarity = rarity,
            level = level,
            isEquipped = false,
            template = template
        };

        instance.InitializeStats();
        equipmentManager.allEquipments.Add(instance);
        equipmentManager.SaveToJson();

        return instance;
    }

    /// <summary>
    /// CharacterEquipment의 슬롯에 장비를 장착시키는 메서드
    /// </summary>
    /// <param name="charID"></param>
    /// <param name="item"></param>
    /// <returns></returns>
    public bool EquipToCharacter(string charID, EquipmentInstance item)
    {
        var character = GetCharacter(charID);
        if (character == null)
        {
            Debug.LogWarning("캐릭터가 존재하지 않습니다.");
            return false;
        }

        if (item.template == null || item.template.equipClass != character.equipClass)
        {
            Debug.LogWarning("캐릭터가 장비할 수 없는 장비입니다.");
            return false;
        }

        var slot = character.slots[item.equipmentType];

        if (slot.equippedItem != null)
        {
            slot.equippedItem.isEquipped = false;
        }

        slot.equippedItem = item;
        item.isEquipped = true;

        equipmentManager.SaveToJson();
        return true;
    }
}

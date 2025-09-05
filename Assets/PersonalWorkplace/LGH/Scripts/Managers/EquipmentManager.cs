using System;
using System.Collections.Generic;
using System.IO;
using UnityEngine;
using VContainer.Unity;

[System.Serializable]
public class EquipmentSaveData
{
    public string instanceID;
    public string templateID;
    public string equipmentType; // enum → string
    public string rarity;        // enum → string
    public int level;
    public bool isEquipped;
    //public bool isLocked;
}

[System.Serializable]
public class EquipmentSaveList
{
    public List<EquipmentSaveData> equipments = new();
}

public class EquipmentManager : IInitializable
{
    public List<EquipmentInstance> allEquipments = new();
    public List<EquipmentSO> allTemplates = new();

    private readonly string savePath = Path.Combine(Application.persistentDataPath, "equipment_save.json");

    public void Initialize()
    {
        LoadFromJson(); // 게임 시작 시 자동 로딩
    }

    public void SaveToJson()
    {
        var saveList = new EquipmentSaveList();

        foreach (var instance in allEquipments)
        {
            saveList.equipments.Add(new EquipmentSaveData
            {
                instanceID = instance.instanceID,
                templateID = instance.templateID,
                equipmentType = instance.equipmentType.ToString(),
                rarity = instance.rarity.ToString(),
                level = instance.level,
                isEquipped = instance.isEquipped,
            });
        }

        string json = JsonUtility.ToJson(saveList, true);
        File.WriteAllText(savePath, json);
        Debug.Log($"장비 데이터 저장 완료: {savePath}");
    }

    private void LoadFromJson()
    {
        if (!File.Exists(savePath))
        {
            Debug.Log("장비 저장 파일이 존재하지 않습니다.");
            return;
        }

        string json = File.ReadAllText(savePath);
        var saveList = JsonUtility.FromJson<EquipmentSaveList>(json);

        foreach (var data in saveList.equipments)
        {
            var template = allTemplates.Find(t => t.templateID == data.templateID);
            if (template == null)
            {
                Debug.LogWarning($"템플릿 '{data.templateID}'을 찾을 수 없습니다.");
                continue;
            }

            if (!Enum.TryParse(data.equipmentType, out EquipmentType type) ||
                !Enum.TryParse(data.rarity, out RarityType rarity))
            {
                Debug.LogWarning($"장비 타입 또는 희귀도 파싱 실패: {data.equipmentType}, {data.rarity}");
                continue;
            }

            var instance = new EquipmentInstance
            {
                instanceID = data.instanceID,
                templateID = data.templateID,
                equipmentType = type,
                rarity = rarity,
                level = data.level,
                isEquipped = data.isEquipped,
                template = template
            };

            instance.InitializeStats(); // 능력치 계산

            allEquipments.Add(instance);
        }

        Debug.Log($"장비 데이터 로딩 완료: {allEquipments.Count}개");
    }
}


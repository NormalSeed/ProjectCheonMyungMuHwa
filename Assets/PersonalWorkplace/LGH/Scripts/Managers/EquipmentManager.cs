using Firebase.Auth;
using Firebase.Extensions;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
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

// Firebase 저장용 클래스
[Serializable]
public class EquipmentFirebaseData
{
    public string templateID;
    public string rarity;
    public int level;
    public bool isEquipped;
}


public class EquipmentManager : IStartable
{
    public bool IsInitialized { get; private set; }

    public List<EquipmentInstance> allEquipments = new();
    public List<EquipmentSO> allTemplates = new();

    private readonly string savePath = Path.Combine(Application.persistentDataPath, "equipment_save.json");

    public EquipmentManager(List<EquipmentSO> allTemplates)
    {
        this.allTemplates = allTemplates;
    }

    public void Start()
    {
        LoadFromJson(); // 게임 시작 시 자동 로딩
        IsInitialized = true;
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

        SaveToFirebase();
    }

    private void SaveToFirebase()
    {
        var db = Firebase.Database.FirebaseDatabase.DefaultInstance;
        string uid = FirebaseAuth.DefaultInstance.CurrentUser?.UserId;
        if (string.IsNullOrEmpty(uid))
        {
            Debug.LogWarning("Firebase 사용자 인증이 되어 있지 않습니다.");
            return;
        }
        var equipmentRef = db.GetReference($"users/{uid}/equipments");

        Dictionary<string, object> equipmentData = new();

        // 파이어베이스에 저장할 때는 Dictionary 등으로 저장 가능한 형식으로 지정해야 함
        foreach (var instance in allEquipments)
        {
            equipmentData[instance.instanceID] = new Dictionary<string, object>
            {
                { "templateID", instance.templateID },
                { "level", instance.level },
                { "rarity", instance.rarity.ToString() },
                { "isEquipped", instance.isEquipped }
            };
        }

        equipmentRef.SetValueAsync(equipmentData).ContinueWithOnMainThread(task =>
        {
            if (task.IsCompleted)
                Debug.Log("Firebase에 장비 데이터 업로드 완료");
            else
                Debug.LogWarning("Firebase 업로드 실패: " + task.Exception?.Message);
        });
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


using System.Linq;
using UnityEngine;
using UnityEngine.UI;
using VContainer;

public class TestEquipmentCreator : MonoBehaviour
{
    [Inject] private EquipmentService equipmentService;
    [Inject] private EquipmentManager equipmentManager;

    [SerializeField] private Button generateButton;

    private void Start()
    {
        generateButton.onClick.AddListener(GenerateRandomEquipment);
    }

    private void GenerateRandomEquipment()
    {
        string randomTemplateID = GetRandomTemplateID();
        RarityType randomRarity = GetRandomRarity();
        int randomLevel = Random.Range(1, 6); // 예: 1~5레벨

        var equipment = equipmentService.AcquireEquipment(randomTemplateID, randomRarity, randomLevel);

        if (equipment != null)
        {
            Debug.Log($"장비 생성 완료: {equipment.templateID} / {equipment.rarity} / Lv.{equipment.level} / Stat: {equipment.GetStat()}");
        }
        else
        {
            Debug.LogWarning("장비 생성 실패");
        }
    }

    private string GetRandomTemplateID()
    {
        var templateIDs = equipmentManager.allTemplates
            .Select(t => t.templateID)
            .ToList();

        if (templateIDs.Count == 0) return "";

        int index = Random.Range(0, templateIDs.Count);
        return templateIDs[index];
    }

    private RarityType GetRandomRarity()
    {
        var values = System.Enum.GetValues(typeof(RarityType));
        return (RarityType)values.GetValue(Random.Range(0, values.Length));
    }
}

using UnityEngine;
using UnityEngine.UI;
using UnityEngine.AddressableAssets;
using UnityEngine.ResourceManagement.AsyncOperations;

public class EquipmentCardDisplay : MonoBehaviour
{
    [Header("UI")]
    [SerializeField] private Image iconImage;

    private EquipmentInstance equipment;

    public void SetData(EquipmentInstance equip)
    {
        equipment = equip;

        if (equipment == null || string.IsNullOrEmpty(equipment.templateID))
        {
            Debug.LogWarning("장비 데이터가 유효하지 않습니다.");
            return;
        }

        string addressKey = $"{equipment.templateID}_{equipment.rarity}";
        LoadIcon(addressKey);
    }

    private void LoadIcon(string key)
    {
        Addressables.LoadAssetAsync<Sprite>(key).Completed += handle =>
        {
            if (handle.Status == AsyncOperationStatus.Succeeded)
            {
                iconImage.sprite = handle.Result;
            }
            else
            {
                Debug.LogError($"장비 아이콘 로드 실패: {key}");
            }
        };
    }
}

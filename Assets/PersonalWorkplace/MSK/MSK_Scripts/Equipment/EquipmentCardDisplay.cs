using UnityEngine;
using UnityEngine.AddressableAssets;
using UnityEngine.ResourceManagement.AsyncOperations;
using UnityEngine.UI;

public class EquipmentCardDisplay : MonoBehaviour
{
    [Header("UI")]
    [SerializeField] private Image iconImage;

    private EquipmentInstance equipment;

    #region Private
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
    #endregion

    #region Public 
    public void SetData(EquipmentInstance equip)
    {
        equipment = equip;

        if (equipment == null || string.IsNullOrEmpty(equipment.templateID))
        {
            Debug.LogWarning("장비 데이터가 유효하지 않습니다.");
            LoadIcon("Exception_Sprite");
            return;
        }

        string addressKey = $"{equipment.templateID}_{equipment.rarity}";
        LoadIcon(addressKey);
    }
    #endregion
}

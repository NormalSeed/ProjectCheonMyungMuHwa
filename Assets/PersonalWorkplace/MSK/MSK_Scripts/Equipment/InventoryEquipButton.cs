using UnityEngine;
using UnityEngine.AddressableAssets;
using UnityEngine.ResourceManagement.AsyncOperations;
using UnityEngine.UI;

public class InventoryEquipButton : MonoBehaviour
{
    [SerializeField] Button button;
    [SerializeField] Image isEquipImg;
    [SerializeField] Image EquipHeroFace;

    private EquipmentInfoPanel panel;
    private EquipmentInstance equipmentInstance;
    #region Unity
    private void OnEnable()
    {
        button.onClick.AddListener(OnClickEquipButton);
    }
    private void OnDisable()
    {
        button.onClick.RemoveListener(OnClickEquipButton);
    }
    #endregion

    #region OnClick
    private void OnClickEquipButton()
    {
        panel.GetEquipmentInstance(equipmentInstance);
        panel.gameObject.SetActive(true);
        panel.Init();
    }
    #endregion

    #region Public 
    public void Init(EquipmentInfoPanel input, EquipmentInstance equip)
    {
        panel = input;
        equipmentInstance = equip;
        if (equip.isEquipped == true)
        {
            isEquipImg.gameObject.SetActive(true);
            LoadAddressableSprite(equipmentInstance.charID + "_face");
        }
        else
        {
            isEquipImg.gameObject.SetActive(false);
        }
    }
    public bool IsSameInstance(EquipmentInstance target)
    {
        return equipmentInstance != null && equipmentInstance.instanceID == target.instanceID;
    }
    #endregion
    #region Private
    private void LoadAddressableSprite(string key)
    {
        var handle = Addressables.LoadAssetAsync<Sprite>(key);

        handle.Completed += OnSpriteLoaded;
    }
    private void OnSpriteLoaded(AsyncOperationHandle<Sprite> handle)
    {
        if (handle.Status == AsyncOperationStatus.Succeeded)
        {
            EquipHeroFace.sprite = handle.Result;
            EquipHeroFace.enabled = true;
        }
        else
        {
            Debug.LogWarning($"Failed to load sprite for key: {handle.DebugName}");
            EquipHeroFace.enabled = false;
        }
    }
    #endregion
}

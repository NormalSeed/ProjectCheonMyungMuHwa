using UnityEngine;
using UnityEngine.AddressableAssets;
using UnityEngine.ResourceManagement.AsyncOperations;
using UnityEngine.EventSystems;
using UnityEngine.UI;
using System.Threading.Tasks;

public class HeroSlotUI : MonoBehaviour, IBeginDragHandler, IDragHandler, IEndDragHandler
{
    public int slotIndex; // MembersID 리스트의 인덱스
    public Image icon;
    public CardInfo cardInfo;

    private GameObject dragVisual;


    public void SetCard(CardInfo info, int index)
    {
        cardInfo = info;
        slotIndex = index;

        if (info != null)
        {
            icon.enabled = false; // 로딩 중 잠시 숨김
            LoadAddressableSprite(info.HeroID + "_sprite");
        }
        else
        {
            LoadAddressableSprite("Exception_Sprite");
            icon.enabled = false;
        }
    }


    public void OnBeginDrag(PointerEventData eventData)
    {
        dragVisual.GetComponent<CanvasGroup>().blocksRaycasts = false;
        dragVisual.transform.SetAsLastSibling();
    }

    public void OnDrag(PointerEventData eventData)
    {
        if (dragVisual != null)
            dragVisual.transform.position = eventData.position;
    }

    public void OnEndDrag(PointerEventData eventData)
    {
        Destroy(dragVisual);

        if (eventData.pointerEnter != null)
        {
            HeroSlotUI targetSlot = eventData.pointerEnter.GetComponent<HeroSlotUI>();
            if (targetSlot != null && targetSlot != this)
            {
                SwapPartyMembers(slotIndex, targetSlot.slotIndex);
            }
        }
    }
    private void LoadAddressableSprite(string key)
    {
        var handle = Addressables.LoadAssetAsync<Sprite>(key);

        handle.Completed += OnSpriteLoaded;
    }

    private void SwapPartyMembers(int from, int to)
    {
        var members = PartyManager.Instance.MembersID;
        (members[from], members[to]) = (members[to], members[from]);

        HeroUI ui = FindFirstObjectByType<HeroUI>();
        ui.RefreshPartySlots();
    }

    private void OnSpriteLoaded(AsyncOperationHandle<Sprite> handle)
    {
        if (handle.Status == AsyncOperationStatus.Succeeded)
        {
            icon.sprite = handle.Result;
            icon.enabled = true;
        }
        else
        {
            Debug.LogWarning($"Failed to load sprite for key: {handle.DebugName}");
            icon.enabled = false;
        }
    }
}
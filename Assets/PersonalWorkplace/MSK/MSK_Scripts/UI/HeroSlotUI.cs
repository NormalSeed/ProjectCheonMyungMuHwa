using UnityEngine;
using UnityEngine.AddressableAssets;
using UnityEngine.ResourceManagement.AsyncOperations;
using UnityEngine.EventSystems;
using UnityEngine.UI;

public class HeroSlotUI : MonoBehaviour, IBeginDragHandler, IDragHandler, IEndDragHandler
{
    public int slotIndex; // MembersID 리스트의 인덱스
    public Image icon;
    public CardInfo cardInfo;

    private GameObject dragVisual;
    private Canvas canvas;

    private void Start()
    {
        canvas = GetComponentInParent<Canvas>();
    }

    public void SetCard(CardInfo info, int index)
    {
        cardInfo = info;
        slotIndex = index;
        LoadAddressableSprite(info.HeroID);
    }

    public void OnBeginDrag(PointerEventData eventData)
    {
        dragVisual = Instantiate(gameObject, canvas.transform);
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

    private void SwapPartyMembers(int from, int to)
    {
        var members = PartyManager.Instance.MembersID;
        (members[from], members[to]) = (members[to], members[from]);

        HeroUI ui = FindFirstObjectByType<HeroUI>();
        ui.RefreshPartySlots();
    }

    public void LoadAddressableSprite(string key)
    {
        Addressables.LoadAssetAsync<Sprite>(key).Completed += handle =>
        {
            if (handle.Status == AsyncOperationStatus.Succeeded)
            {
                icon.sprite = handle.Result;
            }
        };
    }
}
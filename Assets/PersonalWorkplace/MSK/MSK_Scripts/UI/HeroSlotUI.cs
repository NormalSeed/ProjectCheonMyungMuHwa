using UnityEngine;
using UnityEngine.AddressableAssets;
using UnityEngine.EventSystems;
using UnityEngine.ResourceManagement.AsyncOperations;
using UnityEngine.UI;

public class HeroSlotUI : MonoBehaviour, IBeginDragHandler, IDragHandler, IEndDragHandler
{
    [SerializeField] private HeroSlotUI heroSlotPrefab; // 슬롯 프리팹
    [SerializeField] private Sprite circles;
    private GameObject dragVisual;
    private Canvas canvas;
    public int slotIndex; // MembersID 리스트의 인덱스
    public Image icon;
    public CardInfo cardInfo;

    private void Start()
    {
        canvas = GetComponentInParent<Canvas>();
    }

    public void SetCard(CardInfo info, int index)
    {
        cardInfo = info;
        slotIndex = index;

        if (info != null)
        {
            icon.enabled = false;

            // HeroSprites에서 얼굴 이미지 가져오기
            if (HeroSprites.Instance != null && HeroSprites.Instance.IsInitialized)
            {
                var faceSprite = HeroSprites.Instance.GetCharaterFaceSprite(info.HeroID);
                if (faceSprite != null)
                {
                    icon.sprite = faceSprite;
                    icon.enabled = true;
                }
                else
                {
                    Debug.LogWarning($"[HeroSlotUI] 얼굴 이미지 없음: {info.HeroID}");
                    icon.enabled = false;
                }
            }
            else
            {
                Debug.LogWarning("[HeroSlotUI] HeroSprites 초기화 안됨 → 얼굴 이미지 로딩 실패");
                icon.enabled = false;
            }
        }
        else
        {
            icon.sprite = circles;
        }
    }



    public void OnBeginDrag(PointerEventData eventData)
    {
        if (!PartyManager.Instance.IsHeroSetNow)
            return;

        dragVisual = Instantiate(heroSlotPrefab.gameObject, canvas.transform);
        dragVisual.transform.SetAsLastSibling();

        // CanvasGroup 설정
        CanvasGroup cg = dragVisual.GetComponent<CanvasGroup>();
        if (cg == null)
            cg = dragVisual.AddComponent<CanvasGroup>();
        cg.blocksRaycasts = false;
        icon.raycastTarget = false;
        // CardInfo 전달 및 시각적 설정
        HeroSlotUI visualSlot = dragVisual.GetComponent<HeroSlotUI>();
        if (visualSlot != null)
            visualSlot.SetCard(cardInfo, slotIndex); // 현재 슬롯의 카드 정보 복사
    }

    public void OnDrag(PointerEventData eventData)
    {
        if (!PartyManager.Instance.IsHeroSetNow)
            return;

        if (dragVisual != null)
            dragVisual.transform.position = eventData.position;
    }

    public void OnEndDrag(PointerEventData eventData)
    {
        if (!PartyManager.Instance.IsHeroSetNow)
            return;

        Destroy(dragVisual);
        icon.raycastTarget = true;
        if (eventData.pointerEnter != null)
        {
            Debug.LogWarning("[OnEndDrag] 실행됨");
            HeroSlotUI targetSlot = eventData.pointerEnter.GetComponent<HeroSlotUI>();
            if (targetSlot == null)
                Debug.LogWarning("targetSlot == null");
            else
            {
                Debug.LogWarning($"[OnEndDrag] {targetSlot.cardInfo.HeroID}");
            }
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
        if (!PartyManager.Instance.IsHeroSetNow)
            return;

        var members = PartyManager.Instance.MembersID;
        (members[from], members[to]) = (members[to], members[from]);

        HeroUI ui = FindFirstObjectByType<HeroUI>();
        Debug.LogWarning($"[SwapPartyMembers] 실행됨{members[from].HeroID}, {members[to].HeroID}");
        ui.RefreshSlot(members[from]);
        ui.RefreshSlot(members[to]);
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
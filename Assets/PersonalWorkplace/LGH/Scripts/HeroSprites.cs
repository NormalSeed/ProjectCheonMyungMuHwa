using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.AddressableAssets;
using UnityEngine.ResourceManagement.AsyncOperations;

public class HeroSprites : MonoBehaviour
{
    public static HeroSprites Instance { get; private set; }

    public List<Sprite> Sprites = new();
    public List<Sprite> Standing = new();

    private Dictionary<string, Sprite> spriteLookup = new();
    private Dictionary<string, Sprite> standingSpriteLookup = new();
    private Dictionary<string, Sprite> faceSpriteLookup = new();

    private List<string> charIDs = new List<string>
    {
        "C001",
        "C002",
        "C003",
        "C004",
        "N002",
        "N003",
        "R002",
        "R003",
        "U002",
        "U003",
        "L002",
        "L003",
    };

    public bool IsInitialized { get; private set; } = false;

    private void Awake()
    {
        if (Instance != null)
        {
            Destroy(gameObject); // 중복 제거
            return;
        }

        Instance = this;
        DontDestroyOnLoad(gameObject);
    }

    public void Init()
    {
        StartCoroutine(LoadRoutine(charIDs));
    }

    private IEnumerator LoadRoutine(List<string> ids)
    {
        foreach (var id in ids)
        {
            string spriteId = id + "_sprite";
            string faceId = id + "_face";
            string standingId = id + "_standing";

            var faceHandle = Addressables.LoadAssetAsync<Sprite>(faceId);
            var handle = Addressables.LoadAssetAsync<Sprite>(spriteId);
            var Standnghandle = Addressables.LoadAssetAsync<Sprite>(standingId);
            
            yield return handle;
            yield return faceHandle;
            yield return Standnghandle;

            if (handle.Status == AsyncOperationStatus.Succeeded)
            {
                var sprite = handle.Result;
                Sprites.Add(sprite);
                spriteLookup[id] = sprite;
                Debug.Log($"[HeroSprites] 스프라이트 로드 완료: {id}");
            }

            if (Standnghandle.Status == AsyncOperationStatus.Succeeded)
            {
                var sprite = Standnghandle.Result;
                standingSpriteLookup[id] = sprite;
                Debug.Log($"[HeroSprites] 전신 스프라이트 로드 완료: {id}");
            }
            else
            {
                Debug.LogError($"전신 스프라이트 로딩 실패: {id}");
            }

            if (faceHandle.Status == AsyncOperationStatus.Succeeded)
            {
                var faceSprite = faceHandle.Result;
                faceSpriteLookup[id] = faceSprite;
                Debug.Log($"[HeroSprites] 얼굴 이미지 로드 완료: {id}");
            }
            else
            {
                Debug.LogError($"얼굴 이미지 로딩 실패: {id}");
            }
        }

        IsInitialized = true;
        Debug.Log("[HeroSprites] 모든 스프라이트 로딩 완료!");
    }

    public Sprite GetCharacterSprite(string charID)
    {
        spriteLookup.TryGetValue(charID, out var sprite);
        return sprite;
    }

    public Sprite GetCharacterStandingSprite(string charID)
    {
        standingSpriteLookup.TryGetValue(charID, out var sprite);
        return sprite;
    }

    public Sprite GetCharaterFaceSprite(string charID)
    {
        faceSpriteLookup.TryGetValue(charID, out var sprite);
        return sprite;
    }
}

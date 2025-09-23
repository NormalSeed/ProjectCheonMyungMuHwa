using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.AddressableAssets;
using UnityEngine.ResourceManagement.AsyncOperations;

public class HeroSprites : MonoBehaviour
{
    public static HeroSprites Instance { get; private set; }

    public List<Sprite> Sprites = new();

    private Dictionary<string, Sprite> spriteLookup = new();

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
            var handle = Addressables.LoadAssetAsync<Sprite>(spriteId);
            yield return handle;

            if (handle.Status == AsyncOperationStatus.Succeeded)
            {
                var sprite = handle.Result;
                Sprites.Add(sprite);
                spriteLookup[id] = sprite;
                Debug.Log($"[HeroSprites] 스프라이트 로드 완료: {id}");
            }
            else
            {
                Debug.LogError($"스프라이트 로딩 실패: {id}");
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
}

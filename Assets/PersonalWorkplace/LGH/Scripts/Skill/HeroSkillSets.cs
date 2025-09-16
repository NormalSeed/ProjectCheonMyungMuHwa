using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.AddressableAssets;
using UnityEngine.ResourceManagement.AsyncOperations;

public class HeroSkillSets : MonoBehaviour
{
    public static HeroSkillSets Instance;

    public List<GameObject> SkillSets = new();
    private List<string> skillSetIDs = new List<string>
    {
        "SS_Blade1",
        "SS_Sword1",
        "SS_Wand1",
        "SS_Bow1",
        "NS002",
        "NS003",
        "RS002",
        "RS003",
        "US002",
        "US003",
        "LS002",
        "LS003",
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
        StartCoroutine(LoadRoutine(skillSetIDs));
    }

    private IEnumerator LoadRoutine(List<string> ids)
    {
        foreach (var id in ids)
        {
            var handle = Addressables.LoadAssetAsync<GameObject>(id);
            yield return handle;

            if (handle.Status == AsyncOperationStatus.Succeeded)
            {
                GameObject instance = Instantiate(handle.Result, transform);
                instance.name = id; // 이름 지정
                instance.SetActive(false); // 초기엔 비활성화

                SkillSets.Add(instance);
            }
            else
            {
                Debug.LogError($"SkillSet 로딩 실패: {id}");
            }
        }

        IsInitialized = true;
        Debug.Log("[HeroSkillSets] 모든 스킬셋 로딩 완료!");
    }
}

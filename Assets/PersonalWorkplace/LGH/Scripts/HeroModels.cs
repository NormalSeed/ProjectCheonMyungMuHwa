using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.AddressableAssets;
using UnityEngine.ResourceManagement.AsyncOperations;

public class HeroModels : MonoBehaviour
{
    public static HeroModels Instance;

    public List<PlayerModelSO> ModelSOs = new();

    private Dictionary<string, PlayerModelSO> modelLookup = new();

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
            string modelSOId = id + "_model";
            var handle = Addressables.LoadAssetAsync<PlayerModelSO>(modelSOId);
            yield return handle;

            if (handle.Status == AsyncOperationStatus.Succeeded)
            {
                var modelSO = handle.Result;
                ModelSOs.Add(modelSO);
                modelLookup[id] = modelSO;
                Debug.Log($"[HeroModels] 모델SO 로드 완료: {id}");
            }
            else
            {
                Debug.LogError($"ModelSO 로딩 실패: {id}");
            }
        }

        IsInitialized = true;
        Debug.Log("[HeroModels] 모든 모델SO 로딩 완료!");
    }

    /// <summary>
    /// HeroModels에 등록되어있는 ModelSO를 charID를 기준으로 반환하는 메서드
    /// </summary>
    /// <param name="charID"></param>
    /// <returns></returns>
    public PlayerModelSO GetModelSO(string charID)
    {
        modelLookup.TryGetValue(charID, out var model);
        return model;
    }
}

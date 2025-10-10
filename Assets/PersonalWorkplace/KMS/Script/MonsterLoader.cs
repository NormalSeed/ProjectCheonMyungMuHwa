using UnityEngine;
using UnityEngine.AddressableAssets;
using System.Threading.Tasks;
using System.Collections.Generic;
using UnityEngine.ResourceManagement.AsyncOperations;


// 몬스터, 몬스터와 관련된 것들 로드
public class MonsterLoader : MonoBehaviour
{

    public bool IsInitialized;

    private static bool initialized;

    [SerializeField] LoadedMonsterSO monsterSO;

    public void Awake()
    {
        if (initialized)
        {
            Destroy(gameObject);
        }
        else
        {
            DontDestroyOnLoad(gameObject);
        }
    }

    public void Init()
    {
        LoadAll();
    }
    private async void LoadAll()
    {
        await LoadAssetAsync();
        IsInitialized = true;
    }
    private async Task LoadAssetAsync()
    {
        AsyncOperationHandle<IList<GameObject>> handle = Addressables.LoadAssetsAsync<GameObject>("monster");
        IList<GameObject> loadedList = await handle.Task;
        foreach (GameObject obj in loadedList)
        {
            monsterSO.Objects.Add(obj.name, obj);
        }
    }
}

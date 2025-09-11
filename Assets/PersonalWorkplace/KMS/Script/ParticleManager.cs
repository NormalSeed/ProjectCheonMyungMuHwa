using System.Collections.Generic;
using UnityEngine;
using System.Collections;
using UnityEngine.Pool;
using UnityEngine.AddressableAssets;
using Unity.VisualScripting;

public class ParticleManager : MonoBehaviour
{

    public static ParticleManager Instance;

    private Dictionary<string, ObjectPool<ParticleSystem>> poolDict;

    private Dictionary<string, WaitForSeconds> particleDelays;

    private Dictionary<ParticleSystem, string> particleName;

    private void Awake()
    {
        if (Instance == null)
        {
            Instance = this;
        }
        else
        {
            Destroy(gameObject);
        }
        DontDestroyOnLoad(gameObject);
        poolDict = new Dictionary<string, ObjectPool<ParticleSystem>>();
        particleDelays = new Dictionary<string, WaitForSeconds>();
        particleName = new Dictionary<ParticleSystem, string>();
        LoadAssetAsync();
    }

    private async void LoadAssetAsync()
    {
        var handle = Addressables.LoadAssetsAsync<GameObject>("particle");
        IList<GameObject> loadedParticlePrefabs = await handle.Task;
        foreach (GameObject prefab in loadedParticlePrefabs)
        {
            ObjectPool<ParticleSystem> Pool = new ObjectPool<ParticleSystem>(
            createFunc: () =>
            {
                ParticleSystem ps = UnityEngine.Object.Instantiate(prefab).GetComponent<ParticleSystem>();
                ps.transform.parent = gameObject.transform;
                return ps;
            },
            actionOnGet: obj => obj.gameObject.SetActive(true),
            actionOnRelease: obj => obj.gameObject.SetActive(false),
            actionOnDestroy: obj => UnityEngine.Object.Destroy(obj.gameObject),
            defaultCapacity: 20,
            maxSize: 20
            );
            ParticleSystem part = prefab.GetComponent<ParticleSystem>();
            particleDelays.Add(prefab.name, new WaitForSeconds(part.main.duration));
            poolDict.Add(prefab.name, Pool);
            particleName.Add(part, prefab.name);
        }
        Debug.Log($"<color=green> 파티클 로드 완료 </color>");

    }
    public ParticleSystem GetParticle(string name, Vector2 pos, Transform parent = null, float scale = 1)
    {
        ParticleSystem part = poolDict[name].Get();
        part.transform.position = pos;
        part.transform.localScale = scale * Vector3.one;
        if (parent != null) part.transform.SetParent(parent);
        if (!part.main.loop) //만약 루프되는 파티클이 아니라면 자동 반환 처리
        {
            StartCoroutine(WaitForParticleEnd(name, part, particleDelays[name]));
        }
        return part;
    }

    public void ReleaseParticle(ParticleSystem part, string name)
    {
        part.transform.SetParent(gameObject.transform);
        poolDict[name].Release(part);
    }
    IEnumerator WaitForParticleEnd(string name, ParticleSystem pt, WaitForSeconds wfs)
    {
        pt.Play();
        yield return wfs;
        pt.transform.SetParent(gameObject.transform);
        poolDict[name].Release(pt);
    }
}

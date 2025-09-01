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
        Instance = this;
        poolDict = new Dictionary<string, ObjectPool<ParticleSystem>>();
        particleDelays = new Dictionary<string, WaitForSeconds>();
        particleName = new Dictionary<ParticleSystem, string>();
        IList<GameObject> loadedParticlePrefabs = Addressables.LoadAssetsAsync<GameObject>("Particle").WaitForCompletion();
        foreach (GameObject prefab in loadedParticlePrefabs)
        {
            ObjectPool<ParticleSystem> Pool = new ObjectPool<ParticleSystem>(
            createFunc: () => UnityEngine.Object.Instantiate(prefab).GetComponent<ParticleSystem>(),
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
    }
    public ParticleSystem GetParticle(string name, Vector2 pos, Transform parent = null, float scale = 1)
    {
        ParticleSystem part = poolDict[name].Get();
        part.transform.position = pos;
        part.transform.localScale = new Vector3(scale, scale, scale);
        part.transform.SetParent(parent);
        if (!part.main.loop)
        {
            StartCoroutine(WaitForParticleEnd(name, part, particleDelays[name]));
        }
        return part;
    }

    public void ReleaseParticle(ParticleSystem part, string name)
    {
        part.transform.SetParent(null);
        poolDict[name].Release(part);
    }
    IEnumerator WaitForParticleEnd(string name, ParticleSystem pt, WaitForSeconds wfs)
    {
        pt.Play();
        yield return wfs;
        pt.transform.SetParent(null);
        poolDict[name].Release(pt);
    }
}

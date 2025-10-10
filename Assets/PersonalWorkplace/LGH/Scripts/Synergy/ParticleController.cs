using System.Collections.Generic;
using UnityEngine;

public class ParticleController : MonoBehaviour
{
    [SerializeField] private GameObject[] particlePrefabs;
    public Dictionary<string, LGH_ObjectPool> particlePools;

    public void OnEnable()
    {
        particlePools = new Dictionary<string, LGH_ObjectPool>();

        foreach (var prefab in particlePrefabs)
        {
            var pooled = prefab.GetComponent<PooledParticle>();
            var pool = new LGH_ObjectPool(transform, pooled, 15);
            particlePools.Add(prefab.name, pool);
        }
    }

    public void PlayParticle(string particleID, Vector3 position)
    {
        if (!particlePools.ContainsKey(particleID))
        {
            Debug.LogWarning($"Particle ID '{particleID}' not found.");
            return;
        }

        var pooledObj = particlePools[particleID].PopPool();
        var particle = pooledObj as PooledParticle;

        if (particle == null)
        {
            Debug.LogError("PooledParticle 캐스팅 실패");
            return;
        }

        particle.PlayAt(position);
    }
}

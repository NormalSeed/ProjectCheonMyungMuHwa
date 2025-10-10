using System.Collections.Generic;
using UnityEngine;

public class ProjectileController : MonoBehaviour
{
    [SerializeField] private GameObject[] projectilePrefabs;
    public Dictionary<string, LGH_ObjectPool> projectilePools;

    private PlayerController controller;
    private SkillSet skillSet;

    public void Init()
    {
        skillSet = GetComponent<SkillSet>();
        controller = skillSet.controller;

        projectilePools = new Dictionary<string, LGH_ObjectPool>();

        foreach (var prefab in projectilePrefabs)
        {
            PooledObject po = prefab.GetComponent<PooledObject>();
            var pool = new LGH_ObjectPool(transform, po, 5);
            projectilePools.Add(prefab.name, pool);
        }
    }

    public Projectile SpawnProjectile(string projectileID, Vector3 spawnPos, Transform target)
    {
        if (!projectilePools.ContainsKey(projectileID))
        {
            Debug.LogWarning($"Projectile ID '{projectileID}' not found.");
            return null;
        }

        var pooledObj = projectilePools[projectileID].PopPool();
        pooledObj.transform.localScale = Vector3.one;
        pooledObj.transform.position = spawnPos;

        var projectile = pooledObj as Projectile;
        if (projectile == null)
        {
            Debug.LogError("Projectile 캐스팅 실패");
            return null;
        }

        return projectile;
    }
}

using System.Collections.Generic;
using UnityEngine;

public class ProjectileController : MonoBehaviour
{
    [SerializeField] private PooledObject[] projectilePrefabs;
    public Dictionary<string, LGH_ObjectPool> projectilePools;

    private SkillSet skillSet;

    public void Init()
    {
        skillSet = GetComponentInChildren<SkillSet>();

        projectilePools = new Dictionary<string, LGH_ObjectPool>();

        foreach (var prefab in projectilePrefabs)
        {
            var pool = new LGH_ObjectPool(transform, prefab, 5);
            projectilePools.Add(prefab.name, pool);
        }
        Debug.Log($"Skill1의 이펙트 이름 : {projectilePools["Slashwave"]}");

    }

    public Projectile SpawnProjectile(string projectileID, Vector3 spawnPos, Transform target)
    {
        if (!projectilePools.ContainsKey(projectileID))
        {
            Debug.LogWarning($"Projectile ID '{projectileID}' not found.");
            return null;
        }

        var pooledObj = projectilePools[projectileID].PopPool();
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

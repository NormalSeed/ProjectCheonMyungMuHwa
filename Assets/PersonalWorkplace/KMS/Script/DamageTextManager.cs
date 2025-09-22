using UnityEngine;

public class DamageTextManager : MonoBehaviour
{
    public static DamageTextManager Instance;

    public DefaultPool<DamageText> DamagePool;

    [SerializeField] GameObject damageText;

    void Awake()
    {
        if (Instance == null)
        {
            Instance = this;
            DamagePool = new DefaultPool<DamageText>(damageText, 30, exceed: true, warmup: false, parent: gameObject.transform);
            DontDestroyOnLoad(gameObject);
        }
        else
        {
            Destroy(gameObject);
        }
    }

    public DamageText Get(Vector2 pos)
    {
        return DamagePool.GetItem(pos);
    }

}

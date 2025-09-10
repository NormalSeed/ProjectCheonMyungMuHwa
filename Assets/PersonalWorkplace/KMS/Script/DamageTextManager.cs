using UnityEngine;

public class DamageTextManager : MonoBehaviour
{
    public static DamageTextManager Instance;

    public DefaultPool<DamageText> DamagePool;

    void Awake()
    {
        Instance = this;
        DamagePool = new DefaultPool<DamageText>("DamageText", 30, exceed:true, warmup:false, parent:gameObject.transform);
    }

    public DamageText Get(Vector2 pos)
    {
        return DamagePool.GetItem(pos);
    }

}

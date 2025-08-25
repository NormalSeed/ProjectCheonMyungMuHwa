using UnityEngine;

public abstract class PooledObject : MonoBehaviour
{
    public LGH_ObjectPool ObjPool { get; private set; }

    public void PooledInit(LGH_ObjectPool objPool)
    {
        ObjPool = objPool;
    }

    public void ReturnPool()
    {
        ObjPool.PushPool(this);
    }
}


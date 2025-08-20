using UnityEngine;

public class PoolManager : MonoBehaviour
{
    [SerializeField] public Pool PunchPool;
    [SerializeField] public Pool StickPool;
    [SerializeField] public Pool CainPool;
    [SerializeField] public Pool BowPool;

    public void ActiveAll()
    {
        PunchPool.ActiveAllItems();
        StickPool.ActiveAllItems();
        CainPool.ActiveAllItems();
        BowPool.ActiveAllItems();
    }

}

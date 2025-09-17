using UnityEngine;

public class CurrencyManagerTestBootstrap : MonoBehaviour
{
    void Awake()
    {
        if (CurrencyManager.Instance == null)
        {
            var model = new CurrencyModel(); // ICurrencyModel 구현체
            new CurrencyManager(model).Start();
            Debug.Log("[테스트] CurrencyManager 강제 초기화 완료");
        }
    }
}
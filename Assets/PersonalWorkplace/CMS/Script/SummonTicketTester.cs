using UnityEngine;

public class SummonTicketTester : MonoBehaviour
{
    [SerializeField] private CurrencyModel _model; // 인스펙터에 드래그해서 연결
    [SerializeField] private KeyCode addKey = KeyCode.T; // T키 누르면 추가

    private void Update()
    {
        if (Input.GetKeyDown(addKey))
        {
            var model = _model ?? CurrencyManager.Instance?.Model;
            if (model == null)
            {
                Debug.LogWarning("[SummonTicketTester] CurrencyModel이 null입니다.");
                return;
            }

            var current = model.Get(CurrencyType.SummonTicket);
            var newAmount = current + new BigCurrency(1000, 0);
            model.Set(CurrencyType.SummonTicket, newAmount);

            Debug.Log($"[테스트] 소환권 1000개 추가됨 → 현재: {newAmount}");
        }
    }

}

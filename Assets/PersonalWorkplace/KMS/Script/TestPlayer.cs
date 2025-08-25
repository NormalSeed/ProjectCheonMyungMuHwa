using UnityEngine;

public class TestPlayer : MonoBehaviour, IDamagable
{

    [SerializeField] double HP;
    void IDamagable.TakeDamage(double amount)
    {
        HP -= amount;

    }

    void Update()
    {
        if (Input.GetMouseButtonDown(0)) // 마우스 왼쪽 클릭
        {
            Vector2 mousePos = Camera.main.ScreenToWorldPoint(Input.mousePosition);
            RaycastHit2D hit = Physics2D.Raycast(mousePos, Vector2.up);
            // 예: Collider가 붙은 오브젝트의 컴포넌트 가져오기
            if (hit.collider == null) return;
            IDamagable comp = hit.collider.GetComponent<IDamagable>();
            if (comp != null)
            {
                Debug.Log("hit");
                comp.TakeDamage(50000);
            }
        }
    }
}

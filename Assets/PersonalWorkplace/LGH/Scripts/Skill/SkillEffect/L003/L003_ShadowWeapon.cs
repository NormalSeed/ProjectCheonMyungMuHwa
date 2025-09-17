using UnityEngine;

public class L003_ShadowWeapon : MonoBehaviour
{
    private Transform target;
    public float speed = 10f;
    private bool isLaunched = false;
    private Vector2 moveDirection;

    private void OnEnable()
    {
        isLaunched = false;
        moveDirection = Vector2.zero;
    }

    public void SetTarget(Transform target)
    {
        this.target = target;
        Launch();
    }

    public void Launch()
    {
        if (target == null) return;

        // 방향 설정
        Vector2 currentPos = transform.position;
        Vector2 targetPos = target.position;
        moveDirection = (targetPos - currentPos).normalized;

        // 2D 회전 설정 (Z축 기준)
        float angle = Mathf.Atan2(moveDirection.y, moveDirection.x) * Mathf.Rad2Deg;
        transform.rotation = Quaternion.Euler(0, 0, angle);

        isLaunched = true;
    }

    private void Update()
    {
        if (!isLaunched || target == null || !target.gameObject.activeInHierarchy)
        {
            Debug.LogWarning("ShadowWeapon 비활성화됨: isLaunched=" + isLaunched + ", target=" + target);

            gameObject.SetActive(false);
            return;
        }

        // 이동 처리
        transform.position += (Vector3)(moveDirection * speed * Time.deltaTime);

        // 충돌 감지 (간단한 거리 기반)
        float distanceToTarget = Vector2.Distance(transform.position, target.position);

        if (distanceToTarget < 0.1f) // 충돌 거리 기준은 상황에 따라 조정
        {
            isLaunched = false;
            gameObject.SetActive(false);
        }
    }
}

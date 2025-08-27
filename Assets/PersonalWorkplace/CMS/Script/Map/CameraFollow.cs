using UnityEngine;


public class CameraFollow : MonoBehaviour
{
    [SerializeField] private Transform target;  // 따라갈 플레이어
    [SerializeField] private float smoothSpeed = 5f; // 부드럽게 따라가는 속도
    [SerializeField] private Vector3 offset;   // 카메라 위치 오프셋

    private void LateUpdate()
    {
        if (target == null) return;

        Vector3 desiredPosition = target.position + offset;
        Vector3 smoothedPosition = Vector3.Lerp(transform.position, desiredPosition, smoothSpeed * Time.deltaTime);
        transform.position = smoothedPosition;
    }
}
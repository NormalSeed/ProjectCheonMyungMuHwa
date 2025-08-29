using UnityEngine;

public class GroupCamera2D : MonoBehaviour
{
    public Transform[] units;        // 5명 유닛 Transform
    public Vector3 offset;           // 카메라 오프셋 
    public float smoothTime = 0.3f;  // 따라가는 부드러움
    public float minZoom = 5f;       // 최소 줌
    public float maxZoom = 15f;      // 최대 줌
    public float zoomLimiter = 10f;  // 유닛 간 거리 대비 줌 조정 정도

    private Vector3 velocity;
    private Camera cam;

    void Start()
    {
        cam = Camera.main;
    }

    void LateUpdate()
    {
        if (units.Length == 0) return;

        Move();
        Zoom();
    }

    void Move()
    {
        Vector3 centerPoint = GetCenterPoint();
        Vector3 newPosition = centerPoint + offset;

        transform.position = Vector3.SmoothDamp(transform.position, newPosition, ref velocity, smoothTime);
    }

    void Zoom()
    {
        float greatestDistance = GetGreatestDistance();
        float newZoom = Mathf.Lerp(minZoom, maxZoom, greatestDistance / zoomLimiter);
        cam.orthographicSize = Mathf.Lerp(cam.orthographicSize, newZoom, Time.deltaTime);
    }


    float GetGreatestDistance()
    {
        var bounds = new Bounds(units[0].position, Vector3.zero);
        foreach (var unit in units)
        {
            bounds.Encapsulate(unit.position);
        }
        return Mathf.Max(bounds.size.x, bounds.size.y); // 2D니까 x,y 기준
    }

    Vector3 GetCenterPoint()
    {
        if (units.Length == 1)
            return units[0].position;

        var bounds = new Bounds(units[0].position, Vector3.zero);
        foreach (var unit in units)
        {
            bounds.Encapsulate(unit.position);
        }
        return bounds.center;
    }
}

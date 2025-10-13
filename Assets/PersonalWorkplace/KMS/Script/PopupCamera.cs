using UnityEngine;

public class PopupCamera : MonoBehaviour
{
    public static Camera InstanceCamera;
    [SerializeField] Camera cam;
    void Awake()
    {
        if (InstanceCamera == null)
        {
            DontDestroyOnLoad(gameObject);
            InstanceCamera = cam;
        }
        else
        {
            Destroy(gameObject);
        }
    }
}

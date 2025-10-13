using UnityEngine;
using UnityEngine.Rendering.Universal;

public class StackPopupCam : MonoBehaviour
{
    [SerializeField] Camera main;
    void Start()
    {
        var data = main.GetUniversalAdditionalCameraData();
        data.cameraStack.Add(PopupCamera.InstanceCamera);
    }
}

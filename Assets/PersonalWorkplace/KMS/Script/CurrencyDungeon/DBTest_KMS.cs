using UnityEngine;

public class DBTest_KMS : MonoBehaviour
{

    void Start()
    {

    }
    void Update()
    {
        if (Input.GetKeyDown(KeyCode.A))
        {
            string id = BackendManager.Auth.CurrentUser.UserId;
            Debug.Log($"<color=yellow>{id}</color>");

        }
    }
}

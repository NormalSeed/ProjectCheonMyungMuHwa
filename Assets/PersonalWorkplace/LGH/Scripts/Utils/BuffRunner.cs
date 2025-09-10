using UnityEngine;

public class BuffRunner : MonoBehaviour
{
    private static BuffRunner _instance;
    public static BuffRunner Instance
    {
        get
        {
            if (_instance == null)
            {
                var go = new GameObject("BuffRunner");
                DontDestroyOnLoad(go);
                _instance = go.AddComponent<BuffRunner>();
            }
            return _instance;
        }
    }
}

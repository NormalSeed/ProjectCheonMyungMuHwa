using UnityEngine;

public class CanvasTest : MonoBehaviour
{
  void Awake()
  {
    DontDestroyOnLoad(gameObject);
  }
}

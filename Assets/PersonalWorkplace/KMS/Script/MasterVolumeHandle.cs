using UnityEngine;
using UnityEngine.UI;

public class MasterVolumeHandle : MonoBehaviour
{
    private Scrollbar bar;

    public float Value { get => bar.value; set => bar.value = value; }

  void Awake()
  {
        bar = GetComponent<Scrollbar>();
        bar.onValueChanged.AddListener(OnScrollValueChanged);
  }

  public void OnScrollValueChanged(float val)
    {
        AudioListener.volume = val;
    }
}

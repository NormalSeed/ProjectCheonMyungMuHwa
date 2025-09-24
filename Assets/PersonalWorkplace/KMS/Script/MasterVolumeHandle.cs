using UnityEngine;
using UnityEngine.UI;

public class MasterVolumeHandle : MonoBehaviour
{
  private Scrollbar bar;

  [SerializeField] SettingValueSO settingData;

  public float Value { get => bar.value; set => bar.value = value; }

  void Awake()
  {
    bar = GetComponent<Scrollbar>();
    bar.onValueChanged.AddListener(OnScrollValueChanged);
    Value = settingData.SoundVolumeValue;
  }

  public void OnScrollValueChanged(float val)
  {
    AudioListener.volume = val;
    settingData.SoundVolumeValue = val;
  }
}

using UnityEngine;

[CreateAssetMenu(fileName = "SettingValueSO", menuName = "Scriptable Objects/SettingValueSO")]
public class SettingValueSO : ScriptableObject
{
  public float SoundVolumeValue;

  void OnEnable()
  {
    AudioListener.volume = SoundVolumeValue;
  }
}

using System.IO;
using System.Threading.Tasks;
using Unity.VisualScripting;
using UnityEngine;
using UnityEngine.Audio;
public class SettingValueSO : ScriptableObject
{
  private SettingScruct settings;
  [SerializeField] AudioMixer mixer;

  string filePath;

  public float dB_BGM => settings.dB_BGM;
  public float dB_SFX => settings.dB_SFX;

  void OnEnable()
  {
    filePath = Path.Combine(Application.persistentDataPath, "setting.json");
    if (File.Exists(filePath))
    {
      string loadedJson = File.ReadAllText(filePath);
      settings = JsonUtility.FromJson<SettingScruct>(loadedJson);
    }
    else
    {
      settings = new SettingScruct()
      {
        dB_BGM = 0,
        dB_SFX = 0,
      };
    }
    InitialSettingAysnc();
  }

  private async void InitialSettingAysnc()
  {
    await Task.Delay(1000);
    SetAudioMixerBGM(settings.dB_BGM);
    SetAudioMixerSFX(settings.dB_SFX);
  }

  public void SetAudioMixerBGM(float volume)
  {
    mixer.SetFloat("VolumeBGM", volume);
    settings.dB_BGM = volume;

  }
  public void SetAudioMixerSFX(float volume)
  {
    mixer.SetFloat("VolumeSFX", volume);
    settings.dB_SFX = volume;
  }

  public void SaveSettings()
  {
    string json = JsonUtility.ToJson(settings);
    File.WriteAllText(filePath, json);
  }
}

public struct SettingScruct
{
  public float dB_BGM;
  public float dB_SFX;

}

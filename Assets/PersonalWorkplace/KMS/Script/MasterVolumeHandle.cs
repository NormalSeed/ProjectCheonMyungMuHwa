using TMPro;
using UnityEngine;
using UnityEngine.Audio;
using UnityEngine.UI;

public class MasterVolumeHandle : MonoBehaviour
{
  [SerializeField] Slider sliderBGM;
  [SerializeField] Slider sliderSFX;

  [SerializeField] TMP_Text textBGM;
  [SerializeField] TMP_Text textSFX;
  [SerializeField] SettingValueSO settingData;

  void Start()
  {
    sliderBGM.onValueChanged.AddListener(OnBGMValueChange);
    sliderSFX.onValueChanged.AddListener(OnSFXValueChange);
    sliderBGM.value = Mathf.Pow(10, settingData.dB_BGM / 20);
    sliderSFX.value = Mathf.Pow(10, settingData.dB_SFX / 20);

  }

  public void OnBGMValueChange(float val)//원래는 20이 아닌 10
  {
    float dB = Mathf.Log10(val) * 20;
    settingData.SetAudioMixerBGM(dB);
    textBGM.text = ((int)(val * 100)).ToString();
  }
  public void OnSFXValueChange(float val)
  {
    float dB = Mathf.Log10(val) * 20;
    settingData.SetAudioMixerSFX(dB);
    textSFX.text = ((int)(val * 100)).ToString();
  }

  void OnDisable()
  {
    settingData.SaveSettings();
  }

  void OnDestroy()
  {
    sliderBGM.onValueChanged.RemoveAllListeners();
    sliderSFX.onValueChanged.RemoveAllListeners();
  }
}

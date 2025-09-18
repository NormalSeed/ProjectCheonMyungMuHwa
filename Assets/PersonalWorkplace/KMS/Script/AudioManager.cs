using UnityEngine;
using System.Collections.Generic;
using UnityEngine.AddressableAssets;
using UnityEngine.InputSystem;

public class AudioManager : MonoBehaviour
{
    public static AudioManager Instance;
    private DefaultPool<AudioSourceController> soundPool;
    [SerializeField] private GameObject AudioSourcePrefab;
    private Dictionary<string, AudioClip> clips;

    public bool IsInitialized;

    private void Awake()
    {
        if (Instance == null)
        {
            Instance = this;
            DontDestroyOnLoad(gameObject);
        }
        else
        {
            Destroy(gameObject);
        }
    }
    public void Init()
    {
        soundPool = new DefaultPool<AudioSourceController>(obj: AudioSourcePrefab, maxCount: 30, active: true, exceed: true, warmup: false, parent: gameObject.transform);
        clips = new();
        LoadAssetAsync();
    }
    private async void LoadAssetAsync()
    {
        var handle = Addressables.LoadAssetsAsync<AudioClip>("sound");
        IList<AudioClip> loadedSound = await handle.Task;
        foreach (AudioClip clip in loadedSound)
        {
            clips.Add(clip.name, clip);
        }
        IsInitialized = true;
        Debug.Log($"<color=yellow> 사운드 로드 완료 </color>");
    }

    public AudioSourceController PlaySound(string id, float volume = 1, SoundMode mode = SoundMode.Once)
    {
        AudioClip clip = clips[id];
        AudioSourceController asc = soundPool.GetItem();
        asc.Init(clip, volume, mode);
        return asc;
    }
    public void StopSound(AudioSourceController asc)
    {
        soundPool.ReleaseItem(asc);
    }
    public void StopAllSounds()
    {
        soundPool.ReleaseAllItes();
    }

}

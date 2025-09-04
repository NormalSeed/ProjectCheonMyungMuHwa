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

    private void Awake()
    {
        Instance = this;
        soundPool = new DefaultPool<AudioSourceController>(AudioSourcePrefab, 100, true, true, false);
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

}

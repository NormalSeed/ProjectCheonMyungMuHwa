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

    public List<AudioClip> loadedSoundList; // D

    private void Awake()
    {
        Instance = this;
        soundPool = new DefaultPool<AudioSourceController>(AudioSourcePrefab, 100, true, true, false);
        clips = new();
        //IList<AudioClip> loadedSoundList = Addressables.LoadAssetsAsync<AudioClip>("Sound").WaitForCompletion();
        foreach (AudioClip clip in loadedSoundList)
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

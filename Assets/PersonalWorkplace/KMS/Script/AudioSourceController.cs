using UnityEngine;
using System;

public enum SoundMode { Once, Permanent }
public class AudioSourceController : MonoBehaviour, IPooled<AudioSourceController>
{
    public Action<IPooled<AudioSourceController>> OnLifeEnded { get; set; }

    private AudioSource source;

    private float timer;
    private float soundTime;

    private SoundMode currentmode;

    void Awake()
    {
        source = GetComponent<AudioSource>();
    }

    void OnEnable()
    {
        timer = 0;
    }

    public void Init(AudioClip clip, float volume, SoundMode mode)
    {
        source.clip = clip;
        source.volume = volume;
        soundTime = clip.length;
        currentmode = mode;
        source.Play();
    }

    void Update()
    {
        if (timer >= soundTime)
        {
            if (currentmode == SoundMode.Once)
            {
                OnLifeEnded?.Invoke(this);
            }
            else
            {
                source.Play();
                timer = 0;
            }
        }
        timer += Time.deltaTime;
    }


}

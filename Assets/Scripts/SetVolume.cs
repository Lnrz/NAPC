using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class SetVolume : MonoBehaviour
{
    private AudioSource[] audioSrcs;

    void Start()
    {
        float volume;

        audioSrcs = GetComponents<AudioSource>();
        volume = PlayerPrefs.GetFloat("volume", 0.8f);

        SetSourceVolume(volume);
    }

    public void SetSourceVolume(float volume)
    {
        foreach (AudioSource audioSrc in audioSrcs)
        {
            audioSrc.volume = volume;
        }
    }
}

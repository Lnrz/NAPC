using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class AudioManager : MonoBehaviour
{
    [SerializeField] private AudioSource BGM;
    [SerializeField] private AudioSource coinSound;
    [SerializeField] private AudioSource ghostDeathSound;
    [SerializeField] private AudioSource gameOverSound;
    [SerializeField] private AudioSource buttonClickSound;
    [SerializeField] private AudioSource victorySound;
    [SerializeField] private AudioSource fireSound;
    [SerializeField] private AudioSource freezeSound;
    [SerializeField] private AudioSource iceCrackOneSound;
    [SerializeField] private AudioSource iceCrackTwoSound;
    [SerializeField] private AudioSource iceBreakSound;

    public void PlayCoinSound()
    {
        coinSound.Play();
    }

    public void PlayGhostDeathSound()
    {
        ghostDeathSound.Play();
    }

    public void IncreaseBGMPitch()
    {
        BGM.pitch = 1.25f;
    }

    public void RestoreBGMPitch()
    {
        BGM.pitch = 1f;
    }

    public void PlayGameOverSound()
    {
        BGM.Stop();
        gameOverSound.Play();
    }

    public void PlayButtonClickSound()
    {
        buttonClickSound.Play();
    }

    public void PlayVictorySound()
    {
        BGM.Stop();
        victorySound.Play();
    }

    public void PlayFireSound()
    {
        fireSound.Play();
    }

    public void PlayFreezeSound()
    {
        freezeSound.Play();
    }

    public void PlayIceCrackOneSound()
    {
        iceCrackOneSound.Play();
    }

    public void PlayIceCrackTwoSound()
    {
        iceCrackTwoSound.Play();
    }

    public void PlayIceBreakSound()
    {
        iceBreakSound.Play();
    }
}

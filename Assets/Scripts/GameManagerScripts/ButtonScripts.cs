using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;
using TMPro;

public class ButtonScripts : MonoBehaviour
{
    [SerializeField] private Image[] images;
    [SerializeField] private Sprite[] openMouthSkins;
    [SerializeField] private Slider volumeSlider;

    public void Replay()
    {
        Scene current;

        current = SceneManager.GetActiveScene();
        SceneManager.LoadScene(current.buildIndex);
    }

    public void Menu()
    {
        SceneManager.LoadScene(0);
    }

    public void PlayHardBickMap()
    {
        SceneManager.LoadScene(1);
    }

    public void PlayColdIceMap()
    {
        SceneManager.LoadScene(2);
    }

    public void Exit()
    {
        Application.Quit();
    }

    public void ResetScore()
    {
        PlayerPrefs.SetInt("bestscore", 0);
    }

    public void SetPlayerSkin(int id)
    {
        PlayerPrefs.SetInt("skin", id);
    }

    public void SetVolume(float val)
    {
        PlayerPrefs.SetFloat("volume", val);
    }

    public void SettingMenuStart()
    {
        int chosenSkin = PlayerPrefs.GetInt("skin", 0);
        float volume = PlayerPrefs.GetFloat("volume", 0.8f);

        images[chosenSkin].sprite = openMouthSkins[chosenSkin];
        volumeSlider.value = volume;
    }
}
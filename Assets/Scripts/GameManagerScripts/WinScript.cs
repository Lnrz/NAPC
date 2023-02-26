using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class WinScript : MonoBehaviour
{
    private int coinsNum;
    private int nacmanCoinNum = 0;
    private AudioManager audioManager;

    void Start()
    {
        GameObject audioManagerObj;

        audioManagerObj = GameObject.Find("AudioManager");
        audioManager = audioManagerObj.GetComponent<AudioManager>();

        coinsNum = GameObject.FindGameObjectsWithTag("Coin").Length;
    }

    public void AddCoin()
    {
        nacmanCoinNum++;
        if (nacmanCoinNum == coinsNum)
        {
            DisableEnemies();
            DisableNacman();
            audioManager.PlayVictorySound();
            DisplayGameVictory();
        }
    }

    private void DisableEnemies()
    {
        ManageEnemies enemyManager = gameObject.GetComponent<ManageEnemies>();

        enemyManager.DisableEnemies();
    }

    private void DisableNacman()
    {
        PlayerController player = GameObject.FindGameObjectWithTag("Player").GetComponent<PlayerController>();

        player.Deactivate();
    }

    private void DisplayGameVictory()
    {
        GameUI gameUI = gameObject.GetComponent<GameUI>();

        gameUI.DisplayGameEnd("Victory");
    }
}

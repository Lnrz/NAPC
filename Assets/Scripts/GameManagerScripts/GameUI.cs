using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using TMPro;

public class GameUI : MonoBehaviour
{
    private int points = 0;
    [SerializeField] private GameObject startGameUI;
    [SerializeField] private TextMeshProUGUI pointsText;
    [SerializeField] private GameObject scoreDisplay;
    [SerializeField] private GameObject gameEndDisplay;
    [SerializeField] private TextMeshProUGUI endGameText;
    [SerializeField] private TextMeshProUGUI bestScoreText;

    public void AddPoints(int amount)
    {
        points += amount;
        UpdateDisplay();
    }

    private void UpdateDisplay()
    {
        pointsText.text = points.ToString();
    }

    public void DisplayGameEnd(string msg)
    {
        scoreDisplay.SetActive(false);
        endGameText.text = msg;
        gameEndDisplay.SetActive(true);
        UpdateBestScore();
    }

    public void UpdateBestScore()
    {
        int storedValue;

        storedValue = PlayerPrefs.GetInt("bestscore", 0);
        points = (points >= storedValue) ? points : storedValue; 
        bestScoreText.text = points.ToString();
        PlayerPrefs.SetInt("bestscore", points);
    }

    public void StartGameUI()
    {
        startGameUI.SetActive(false);
        scoreDisplay.SetActive(true);
    }
}
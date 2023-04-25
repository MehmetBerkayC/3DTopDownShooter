using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;
using TMPro;

public class GameUI : MonoBehaviour
{
    [SerializeField] Image fadePlane;
    [SerializeField] GameObject gameoverUI;
    [SerializeField] GameObject inGameUI;

    [SerializeField] RectTransform waveBanner;
    [SerializeField] TMP_Text waveTitle;
    [SerializeField] TMP_Text waveEnemyCount;
    
    [SerializeField] TMP_Text scoreText;
    [SerializeField] RectTransform healthBar;

    [SerializeField] TMP_Text gameoverScoreText;

    Spawner spawner;
    Player player;

    private void Start()
    {
        player = FindObjectOfType<Player>();
        player.OnDeath += OnGameOver;
    }

    void Awake()
    {
        spawner = FindObjectOfType<Spawner>();
        spawner.OnNewWave += OnNewWave;
    }

    private void Update()
    {
        scoreText.text = ScoreKeeper.score.ToString("D6");
        
        // health calculation
        float healthPercentage = 0;
        if(player != null)
        {
            healthPercentage = player.Health / player.startingHealth;
        }
        healthBar.localScale = new Vector3(healthPercentage, 1, 1);

    }

    void OnNewWave(int waveNumber)
    {
        String[] numbers = { "One", "Two", "Three", "Four", "Five", "Six" };
        waveTitle.text = "- Wave " + numbers[waveNumber - 1] + " -";

        string enemyCountString = ((spawner.waves[waveNumber - 1].infinite) ? "Infinite" : spawner.waves[waveNumber - 1].enemyCount + "");
        waveEnemyCount.text = "Enemies: " + enemyCountString;

        StopCoroutine("AnimateWaveBanner");
        StartCoroutine("AnimateWaveBanner");
    }

    IEnumerator AnimateWaveBanner()
    {
        waveBanner.gameObject.SetActive(true);
        float delayTime = 1.5f;
        float speed = 2.5f;
        float animatePercentage = 0;
        int direction = 1;

        float endDelayTime = Time.time + 1 / speed + delayTime;

        while (animatePercentage >= 0)
        {
            animatePercentage += Time.deltaTime * speed * direction;

            if (animatePercentage > 1)
            {
                animatePercentage = 1;
                if(Time.time > endDelayTime)
                {
                    direction = -1;
                }
            }

            waveBanner.anchoredPosition = Vector2.up * Mathf.Lerp(-630, -356, animatePercentage);
            yield return null;
        }

        waveBanner.gameObject.SetActive(false);
    }
 
    IEnumerator Fade(Color from, Color to, float time)
    {
        float speed = 1f / time;
        float percent = 0f;

        while (percent < 1)
        {
            percent += Time.deltaTime * speed;
            fadePlane.color = Color.Lerp(from, to, percent);
            yield return null; 
        }
    }

    // UI Input
    public void StartNewGame()
    {
        SceneManager.LoadScene("Game");

        inGameUI.SetActive(true);
        gameoverUI.SetActive(false);
        
        Cursor.visible = false;
        Cursor.lockState = CursorLockMode.Confined;
    }
    
    void OnGameOver()
    {
        StartCoroutine(Fade(Color.clear, fadePlane.color, 1f));

        gameoverScoreText.text = scoreText.text;

        inGameUI.SetActive(false);
        gameoverUI.SetActive(true);
     
        Cursor.visible = true;
    }

    public void ReturnMainMenu()
    {
        SceneManager.LoadScene("Menu");
    }
}

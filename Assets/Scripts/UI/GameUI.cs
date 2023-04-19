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

    [SerializeField] RectTransform waveBanner;
    [SerializeField] TMP_Text waveTitle;
    [SerializeField] TMP_Text waveEnemyCount;

    Spawner spawner;

    private void Start()
    {
        FindObjectOfType<Player>().OnDeath += OnGameOver;
    }

    void Awake()
    {
        spawner = FindObjectOfType<Spawner>();
        spawner.OnNewWave += OnNewWave;
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

    // UI Inout
    public void StartNewGame()
    {
        SceneManager.LoadScene("Game");
    }
    
    void OnGameOver()
    {
        Cursor.visible = true;
        StartCoroutine(Fade(Color.clear, Color.black, 1f));
        gameoverUI.SetActive(true);
    }
}

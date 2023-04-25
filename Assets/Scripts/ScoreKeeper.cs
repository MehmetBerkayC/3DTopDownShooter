using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ScoreKeeper : MonoBehaviour
{
    public static int score { get; private set; }
    float lastEnemyKillTime;
    int streakCount;
    float streakExpireTime = 2f;

    void Start()
    {
        Enemy.OnDeathStatic += OnEnemyKilled;
        FindObjectOfType<Player>().OnDeath += OnPlayerDeath;
    }

    void OnEnemyKilled()
    {
        if(Time.time < lastEnemyKillTime + streakExpireTime)
        {
            if(streakCount <= 7)
            {
                streakCount++;
            }
        }
        else
        {
            streakCount = 0;
        }

        lastEnemyKillTime = Time.time;
        score += 5 * (int)Mathf.Pow(2, streakCount);

    }

    void OnPlayerDeath()
    {
        Enemy.OnDeathStatic -= OnEnemyKilled;
        score = 0;
    }
}

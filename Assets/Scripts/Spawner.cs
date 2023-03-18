using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Spawner : MonoBehaviour
{
    [SerializeField] Wave[] waves;
    [SerializeField] Enemy enemy;

    Wave currentWave;
    int currentWaveNumber;

    int enemiesRemainingToSpawn;
    int enemiesRemainingAlive;
    float nextSpawnTime;

    private void Start()
    {
        NextWave();
    }

    private void NextWave()
    {
        currentWaveNumber++;
        print("Wave " + currentWaveNumber);
        if(currentWaveNumber <= waves.Length)
        {
            currentWave = waves[currentWaveNumber - 1];

            enemiesRemainingToSpawn = currentWave.enemyCount;
            enemiesRemainingAlive = enemiesRemainingToSpawn;
        }
    }

    private void Update()
    {
       if(enemiesRemainingToSpawn > 0 && Time.time > nextSpawnTime)
        {
            enemiesRemainingToSpawn--;
            nextSpawnTime = Time.time + currentWave.timeBetweenSpawns;

            Enemy spawnedEnemy = Instantiate(enemy, Vector3.zero, Quaternion.identity) as Enemy;
            spawnedEnemy.OnDeath += OnEnemyDeath; 
       }
    }

    void OnEnemyDeath()
    {
        enemiesRemainingAlive--;
        
        if(enemiesRemainingAlive == 0)
        {
            NextWave();
        }
    }
}

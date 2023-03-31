using UnityEngine;
using System.Collections;

[System.Serializable]
public class Wave
{
    public bool infinite;
    public int enemyCount;
    public float timeBetweenSpawns;
    public float moveSpeed;
    public int hitsToKillPlayer;
    public float enemyHealth;
    public Color skinColor;
}
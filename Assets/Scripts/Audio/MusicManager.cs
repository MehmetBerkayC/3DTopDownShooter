using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class MusicManager : MonoBehaviour
{
    [SerializeField] AudioClip mainTheme;
    [SerializeField] AudioClip menuTheme;

    private void Start()
    {
        AudioManager.instance.PlayMusic(menuTheme, 2);
    }

    private void Update()
    {
        if (Input.GetKeyDown(KeyCode.K))
        {
            AudioManager.instance.PlayMusic(mainTheme, 3);
        }
    }
}

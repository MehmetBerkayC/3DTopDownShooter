using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class AudioManager : MonoBehaviour
{
    float masterVolumePercentage = 1f;
    float sfxVolumePercentage = 1;
    float musicVolumePercentage = 1;

    AudioSource[] musicSources;
    int activeMusicSourceIndex;

    public static AudioManager instance;

    Transform audioListener;
    Transform playerTransform;

    private void Awake()
    {
        instance = this;

        musicSources = new AudioSource[2];
        for (int i = 0; i < 2; i++)
        {
            GameObject newMusicSource = new GameObject("Music Source " + (i + 1));
            musicSources[i] = newMusicSource.AddComponent<AudioSource>();
            newMusicSource.transform.parent = transform;
        }

        audioListener = FindObjectOfType<AudioListener>().transform;
        playerTransform = FindObjectOfType<AudioListener>().transform;
    }

    private void Update()
    {
        if(playerTransform != null)
        {
            audioListener.position = playerTransform.position;
        }
    }

    public void PlayMusic(AudioClip clip, float fadeDuration = 1)
    {
        activeMusicSourceIndex = 1 - activeMusicSourceIndex;
        musicSources[activeMusicSourceIndex].clip = clip;
        musicSources[activeMusicSourceIndex].Play();

        StartCoroutine(AnimateMusicCrossfade(fadeDuration));
    }

    public void PlaySound(AudioClip clip, Vector3 position)
    {
        // ideal for short clips
        AudioSource.PlayClipAtPoint(clip, position, sfxVolumePercentage * masterVolumePercentage);
    }

    public void PlaySound(string soundName, Vector3 position)
    {

    }

    IEnumerator AnimateMusicCrossfade(float duration)
    {
        float percentage = 0;

        while (percentage < 1)
        {
            percentage += Time.deltaTime * 1 / duration;

            musicSources[activeMusicSourceIndex].volume = Mathf.Lerp(0, musicVolumePercentage * masterVolumePercentage, percentage);
            musicSources[1 - activeMusicSourceIndex].volume = Mathf.Lerp(musicVolumePercentage * masterVolumePercentage, 0, percentage);
            yield return null;
        }
    }
}

using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class AudioManager : MonoBehaviour
{
    public enum AudioChannel { Master, Sfx, Music};

    public float masterVolumePercentage { get; private set; }
    public float sfxVolumePercentage { get; private set; }
    public float musicVolumePercentage { get; private set; }

    AudioSource sfx2DSource;
    AudioSource[] musicSources;
    int activeMusicSourceIndex;

    public static AudioManager instance;

    Transform audioListener;
    Transform playerTransform;

    SoundLibrary library;

    private void Awake()
    {
        if(instance != null)
        {
            Destroy(gameObject);
        }
        else
        {
            instance = this;

            DontDestroyOnLoad(gameObject);

            library = GetComponent<SoundLibrary>();

            musicSources = new AudioSource[2];
            for (int i = 0; i < 2; i++)
            {
                GameObject newMusicSource = new GameObject("Music Source " + (i + 1));
                musicSources[i] = newMusicSource.AddComponent<AudioSource>();
                newMusicSource.transform.parent = transform;
            }

            GameObject newSfx2DSource = new GameObject("2D Sfx Source");
            sfx2DSource = newSfx2DSource.AddComponent<AudioSource>();
            newSfx2DSource.transform.parent = transform;

            audioListener = FindObjectOfType<AudioListener>().transform;
            
            if (FindObjectOfType<Player>() != null)
            {
                playerTransform = FindObjectOfType<AudioListener>().transform;
            }

            masterVolumePercentage = PlayerPrefs.GetFloat("Master Volume", 1);
            sfxVolumePercentage = PlayerPrefs.GetFloat("Sfx Volume", 1);
            musicVolumePercentage = PlayerPrefs.GetFloat("Music Volume", 1);
        }

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
        PlaySound(library.GetClipFromName(soundName), position);
    }
    public void PlaySound2D(string soundName)
    {
        sfx2DSource.PlayOneShot(library.GetClipFromName(soundName), sfxVolumePercentage * masterVolumePercentage);
    }

    public void SetVolume(float volumePercentage, AudioChannel channel)
    {
        switch (channel)
        {
            case AudioChannel.Master:
                masterVolumePercentage = volumePercentage;
                break;
                
            case AudioChannel.Sfx:
                sfxVolumePercentage = volumePercentage;
                break;
                
            case AudioChannel.Music:
                musicVolumePercentage = volumePercentage;
                break;
        }

        musicSources[0].volume = musicVolumePercentage * masterVolumePercentage;
        musicSources[1].volume = musicVolumePercentage * masterVolumePercentage;

        PlayerPrefs.SetFloat("Master Volume", masterVolumePercentage);
        PlayerPrefs.SetFloat("Sfx Volume", masterVolumePercentage);
        PlayerPrefs.SetFloat("Music Volume", masterVolumePercentage);
        PlayerPrefs.Save();
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

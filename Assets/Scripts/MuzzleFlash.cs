using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class MuzzleFlash : MonoBehaviour
{
    [SerializeField] GameObject flashHolder;
    [SerializeField] float flashTime;

    [SerializeField] Sprite[] flashSprites;
    [SerializeField] SpriteRenderer[] spriteRenderers;

    void Start()
    {
        Deactivate();
    }

    public void Activate()
    {
        flashHolder.SetActive(true);

        int spriteIndex = Random.Range(0, flashSprites.Length);
        for (int i = 0; i < spriteRenderers.Length; i++)
        {
            spriteRenderers[i].sprite = flashSprites[spriteIndex];
        }

        Invoke("Deactivate", flashTime);
    }

    private void Deactivate()
    {
        flashHolder.SetActive(false);
    }

}

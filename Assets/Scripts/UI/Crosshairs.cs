using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Crosshairs : MonoBehaviour
{
    [SerializeField] LayerMask targetMask;
    [SerializeField] Color highlightColor;
    [SerializeField] SpriteRenderer dot;
    Color originalColor;


    private void Start()
    {
        Cursor.visible = false;

        originalColor = dot.color;   
    }

    void Update()
    {
        transform.Rotate(Vector3.forward * 40 * Time.deltaTime);        
    }

    public void DetectTargets(Ray ray, float maxDistance)
    {
        if(Physics.Raycast(ray, maxDistance, targetMask))
        {
            dot.color = highlightColor;
        }
        else
        {
            dot.color = originalColor;
        }
    }
}

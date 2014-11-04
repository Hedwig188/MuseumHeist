using UnityEngine;
using System;
using System.Collections;

public class SineWave_VLS : MonoBehaviour
{
    public bool useRandomStart = true;
    public Vector3 axis = Vector3.up;
    public float speed = 5.0f;
    public float magnitude = 1.0f;

    private float startTime = 0;
    private Vector3 startPos;
    private Light2D isLight = null;

    void Start()
    {
        if (useRandomStart)
        {
            startTime = UnityEngine.Random.Range(-1500f, 1500f);
        }

        startPos = transform.localPosition;

        isLight = gameObject.GetComponent<Light2D>();
    }


	void FixedUpdate() 
    {
        if (light != null)
        {
            if (isLight.IsVisible)
            {
                transform.localPosition = startPos + (axis * Mathf.Sin((startTime + Time.time) * speed) * magnitude);
            }
        }
        else
        {
            transform.localPosition = startPos + (axis * Mathf.Sin((startTime + Time.time) * speed) * magnitude);
        }
	}
}

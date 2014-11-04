/*
 *  Used to test the mesh leakage
 */

using UnityEngine;
using System.Collections;
using System.Collections.Generic;

public class RapidLightGenerator : MonoBehaviour
{
    public int lightsPerIteration = 5;

    private Light2D[] lightObjs = new Light2D[0];
    private static int lightsGenerated = 0;

    /*
    void OnGUI()
    {
        GUILayout.BeginArea(new Rect(20, 20, 200, 400));
        {
#if UNITY_EDITOR
            GUILayout.Label("Heap: " + (Profiler.usedHeapSize * 0.000001f).ToString());
#endif
            //GUILayout.Label("Time: " + Time.time.ToString("0"));
            GUILayout.Label("Lights Generated: " + lightsGenerated.ToString());
            GUILayout.Label("FPS: " + fps.ToString("0.0"));
        }
        GUILayout.EndArea();
    }
    */

    void Update()
    {
        if (lightObjs.Length > 0)
        {
            for (int i = 0; i < lightsPerIteration; i++)
                Destroy(lightObjs[i].gameObject);
        }

        lightObjs = new Light2D[lightsPerIteration];
        lightsGenerated += lightsPerIteration;

        for (int i = 0; i < lightsPerIteration; i++)
        {
            lightObjs[i] = Light2DRadial.Create(new Vector3(Random.Range(-5, 5), Random.Range(-4, 4), 0), GetRandColor(), Random.Range(1f, 2f));
        }

        TickFPSCounter();
    }

    Color GetRandColor()
    {
        int r = Random.Range(0, 4);
        switch (r)
        {
            case 0:
                return Color.red;
            case 1:
                return Color.green;
            case 2:
                return Color.blue;
            case 3:
                return Color.yellow;
        }

        return Color.red;
    }

    int frameCount = 0;
    float nextUpdate = 0f;
    float fps = 0f;
    float updateRate = 3.0f;
    void TickFPSCounter()
    {
        frameCount++;
        if (Time.time > nextUpdate)
        {
            nextUpdate = Time.time + (1f / updateRate);
            fps = frameCount * updateRate;
            frameCount = 0;
        }
    }
}

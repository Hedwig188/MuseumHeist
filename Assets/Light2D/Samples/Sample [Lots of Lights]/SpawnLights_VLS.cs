using UnityEngine;
using System.Collections;
using System.Collections.Generic;

public class SpawnLights_VLS : MonoBehaviour 
{
    static int lightCount = 0;

    public float spawnSpeed = 0.05f;
    public int XLightCount = 10;
    public int YLightCount = 10;
    public float lightMinScale = 15;
    public float lightMaxScale = 35;

    private int x = 0;
    private int y = 0;
    private bool spawnLights = true;

    void Start()
    {
        StartCoroutine("AddLight");
        lightCount = 0;
        x = 0;
        y = 0;
        spawnLights = true;
    }

    IEnumerator AddLight()
    {
        while (spawnLights)
        {

            Light2DRadial.Create(new Vector3((x * lightMinScale), (y * lightMinScale), 0), new Color((float)Random.Range(0, 1f), 0, (float)Random.Range(0, 1f), 1f), Random.Range(lightMinScale, lightMaxScale), 360, Light2D.LightDetailSetting.Rays_300);
            lightCount++;

            //l2d.gameObject.AddComponent<SineWave_VLS>();


            if (++x >= XLightCount)
            {
                x = 0;

                if (++y >= YLightCount)
                    spawnLights = false;
            }

            yield return 0;
        }
    }
}

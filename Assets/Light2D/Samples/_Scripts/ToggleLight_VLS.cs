using UnityEngine;
using System.Collections;

public class ToggleLight_VLS : MonoBehaviour 
{
    Light2DRadial l2D;

    void Start()
    {
        Random.seed = gameObject.GetInstanceID();
        InvokeRepeating("ToggleLight", Random.Range(0.2f, 2f), Random.Range(0.2f, 2f));
        l2D = gameObject.GetComponent<Light2DRadial>();
    }

    void ToggleLight()
    {
            l2D.LightColor = new Color(Random.Range(0f, 1f), Random.Range(0f, 1f), Random.Range(0f, 1f), 0.2f);

        l2D.ToggleLight(true);
    }
}

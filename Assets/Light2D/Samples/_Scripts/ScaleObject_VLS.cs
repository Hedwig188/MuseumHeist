using UnityEngine;
using System.Collections;

public class ScaleObject_VLS : MonoBehaviour 
{
    void FixedUpdate()
    {
        ScaleObject();
    }

    void ScaleObject()
    {
        float s = Random.Range(0.5f, 3f);
        transform.localScale = new Vector3(s,s,s);
    }
}

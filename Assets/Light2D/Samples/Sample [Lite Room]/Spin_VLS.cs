using UnityEngine;
using System.Collections;

public class Spin_VLS : MonoBehaviour 
{
    public float speed = 5;
    public Vector3 axis = new Vector3(0, 1, 0);
    void Update()
    {
        transform.Rotate(axis * speed * Time.deltaTime);
    }
}

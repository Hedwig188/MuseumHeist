using UnityEngine;
using System.Collections;

public class Orbit_VLS : MonoBehaviour 
{
    public Vector3 pointToOrbit = Vector3.zero;
    public float orbitSpeed = 25f;

	void Update () 
    {
        transform.RotateAround(pointToOrbit, Vector3.forward, orbitSpeed * Time.deltaTime);
	}
}

using UnityEngine;
using System.Collections;

public class CameraPlanerMovement_VLS : MonoBehaviour
{
    public float speed = 25;

    private float s = 0;

    void OnGUI()
    {
        GUI.Box(new Rect(Screen.width/2 - 5, Screen.height/2 - 5, 10, 10), "");
    }

	// Update is called once per frame
	void Update () 
    {
        s = speed;
        if (Input.GetKey(KeyCode.LeftShift) || Input.GetKey(KeyCode.RightShift))
            s *= 2f;

        transform.Translate(Input.GetAxis("Horizontal") * Time.deltaTime * s, Input.GetAxis("Vertical") * Time.deltaTime * s, 0);
	}
}

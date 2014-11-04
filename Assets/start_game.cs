using UnityEngine;
using System.Collections;

public class start_game : MonoBehaviour {

	// Update is called once per frame
	void Update () {
		if (Input.touchCount == 1) {
			Application.LoadLevel("ChooseRole");
		}
	}
}

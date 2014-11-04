using UnityEngine;
using System.Collections;

public class FogOfWar : MonoBehaviour {

	// Use this for initialization
	void Start () {
		isLight = false;
		isGuard = false;
		isLightOn = false;
	}
	
	// Update is called once per frame
	void Update () {
		if (isLight) {
			lightObj = GameObject.Find (lightName);
			//print (lightName);
						script1 = (lightScript)lightObj.transform.gameObject.GetComponent ("lightScript");
//			script1 = lightObj.GetComponent<"lightScirpt">();
			//print ("isOn"+script1.isOn);
						if (script1.isOn) {  //light is on
								isLightOn = true;
						} else {
								isLightOn = false;
								isLight = false;
						}
		} else {isLightOn = false;
				}
		if (isLightOn || isGuard) {
			this.renderer.enabled = false;
		} 
		else {
			this.renderer.enabled = true;
		}
	}

	void OnTriggerEnter2D (Collider2D col) {
		if (col.name.StartsWith("guard_light")) {
			lightName = col.name;
			isLight = true;
			//print ("trigger"+lightName);
		}

	
		if (string.Equals (col.tag, "guard")) {
			//this.renderer.enabled = false;
			isGuard = true;
		}
	}
	void OnTriggerExit2D (Collider2D col) {
		if (string.Equals (col.tag, "guard")) {
			//this.renderer.enabled = true;
			isGuard = false;

		}
	}

	public bool isLight;
	public bool isLightOn;
	public bool isGuard;
	public string lightName; 
	private GameObject lightObj; //= GameObject.Find(guardNumber + "");
	private lightScript script1; //= (lightScript)lightObj.transform.gameObject.GetComponent("lightScript");



	
}

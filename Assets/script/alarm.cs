﻿using UnityEngine;
using System.Collections;

public class alarm : MonoBehaviour {

	public float fadeSpeed = 2f;
	public float highIntensity = 2f;
	public float lowIntensity = 0.5f;
	public float changeMargin = 0.2f;
	public bool alarmOn;

	private float targetIntensity;

	void Awake(){
		light.intensity = 0f;
		targetIntensity = highIntensity;
	}

	void Update(){
		if (alarmOn) {
			light.intensity = Mathf.Lerp(light.intensity,targetIntensity,fadeSpeed*Time.deltaTime);
			CheckTargetIntensity ();
		}else{
			light.intensity = Mathf.Lerp(light.intensity,0f,fadeSpeed*Time.deltaTime);
		}

	}

	void CheckTargetIntensity(){
		if (Mathf.Abs (light.intensity - targetIntensity) < changeMargin) {
			if(targetIntensity == highIntensity){
				targetIntensity = lowIntensity;
			}else{
				targetIntensity = highIntensity;
			}
		}
	}
}

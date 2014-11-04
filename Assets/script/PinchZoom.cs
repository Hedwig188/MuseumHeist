using UnityEngine;
using System.Collections;

public class PinchZoom : MonoBehaviour {

	public float orthoZoomSpeed = 0.2f;
	public float maxSize = 4.0f;
	public float minSize = 2.0f;
	void Update(){
		if (Input.touchCount == 2) {
			Touch touchZero = Input.GetTouch(0);
			Touch touchOne = Input.GetTouch(1);

			Vector2 touchZeroPrevPos = touchZero.position-touchZero.deltaPosition;
			Vector2 touchOnePrevPos = touchOne.position-touchOne.deltaPosition;

			float prevTouchDeltaMag = (touchZeroPrevPos-touchOnePrevPos).magnitude;
			float touchDeltaMag = (touchZero.position-touchOne.position).magnitude;

			float deltaMagnitudeDiff = prevTouchDeltaMag-touchDeltaMag;
			if(camera.isOrthoGraphic){
				camera.orthographicSize +=deltaMagnitudeDiff*orthoZoomSpeed;
				camera.orthographicSize = Mathf.Max(Mathf.Min(camera.orthographicSize,maxSize),minSize);
			}

		}
	}
}

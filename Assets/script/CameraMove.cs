using UnityEngine;
using System.Collections;

public class CameraMove : MonoBehaviour {
	public float xMargin = 1f;
	public float yMargin = 1f;
	public float xSmooth = 4f;
	public float ySmooth = 4f;
//	public Vector2 maxXAndY;
//	public Vector2 minXAndY;
	private Transform player;

	void Start()
	{

	}

	bool CheckXMargin(){
		return Mathf.Abs (transform.position.x - player.position.x) > xMargin;
	}

	bool CheckYMargin(){
		return Mathf.Abs (transform.position.y - player.position.y) > yMargin;
	}

	void Update(){
		Vector3 tempPos = new Vector3 ();
		tempPos = transform.position;
		if (tempPos.x > 11)
						tempPos.x = 11;
		if (tempPos.x < 1)
			tempPos.x = 1;
		if (tempPos.y > 5)
						tempPos.y = 5;
		if (tempPos.y < 1)
						tempPos.y = 1;
		transform.position = tempPos;
		if(Network.isClient)
		TrackPlayer ();
	}

	void TrackPlayer(){
		player = GameObject.Find("ThiefChild").transform;
		float targetX = transform.position.x;
		float targetY = transform.position.y;
//		if (CheckXMargin()) {
			targetX = Mathf.Lerp(transform.position.x,player.position.x,xSmooth*Time.deltaTime);
//		}

//		if (CheckYMargin()) {
			targetY = Mathf.Lerp(transform.position.y,player.position.y,ySmooth*Time.deltaTime);
//		}
//		targetX = Mathf.Clamp (targetX, minXAndY.x, maxXAndY.x);
//		targetY = Mathf.Clamp (targetY, minXAndY.y, maxXAndY.y);

		transform.position = new Vector3 (targetX, targetY, transform.position.z);

	}
}

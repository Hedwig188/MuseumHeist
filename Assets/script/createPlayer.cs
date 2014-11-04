using UnityEngine;
using System.Collections;

public class createPlayer : MonoBehaviour {
	private GameObject player;
	// Use this for initialization
	void Awake()
	{
		if(Network.isClient)
		{
			Debug.Log("client player create");
			player = Resources.Load ("Thief") as GameObject;
			Vector3 playerPosition = new Vector3 (4.8f, 1.5f, 0f);
			Network.Instantiate (player, playerPosition, Quaternion.identity, 0);
			Debug.Log("15");
			//			yield WaitForSeconds(3);
			networkView.RPC("hidePlayer",RPCMode.Server,null);
		}
	}
	void Start () 
	{
		
	}
	
	// Update is called once per frame
	void Update () {
	
	}
[RPC]
	void hidePlayer()
	{
		Debug.Log ("hideplayer");
		player = GameObject.FindGameObjectWithTag ("Player");
		player.renderer.enabled = false;
	}
}

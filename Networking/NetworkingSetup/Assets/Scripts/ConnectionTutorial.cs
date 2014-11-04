using UnityEngine;
using System.Collections;

public class ConnectionTutorial : MonoBehaviour {
	public int portNum = 25001;
	public string ipAdd = "127.0.0.1";
	public GameObject laser,prefab,playerClient;
	void OnGUI()
	{
		if(Network.peerType == NetworkPeerType.Disconnected)
		{
			GUI.Label(new Rect(100,100,100,100),"Status:Disconnected");
			
			if(GUI.Button(new Rect(100,240,100,100),"Connect client"))
			Network.Connect(ipAdd,portNum);
			
			if(GUI.Button(new Rect(100,350,100,100),"Initialize server"))
			Network.InitializeServer(8,portNum,false);
		}
		else if(Network.peerType == NetworkPeerType.Client)
		{
			GUI.Label(new Rect(100,100,100,100),"Connected as client");
			if(GUI.Button(new Rect(100,240,100,100),"Disconnect"))
			Network.Disconnect(200);
		}
		else if(Network.peerType == NetworkPeerType.Server)
		{
			GUI.Label(new Rect(100,100,100,100),"Server ready");
			if(GUI.Button(new Rect(100,240,100,100),"Disconnect"))
				Network.Disconnect(200);
		}
		if(Network.isServer)
		{
			laser = Resources.Load("laser") as GameObject;
			Quaternion rotation = Quaternion.identity;
			rotation.eulerAngles = new Vector3(0,0,270);
			Vector3 pos = new Vector3(-2f,0.5f,2f);
			if(GUI.Button(new Rect(50,50,50,50),"Place object"))
			{
				Network.Instantiate(laser,pos,rotation,0);
				
			}
		}
	}

	// Use this for initialization
	void Start () {
	
	}
	
	// Update is called once per frame
	void Update () {
	
	}
}

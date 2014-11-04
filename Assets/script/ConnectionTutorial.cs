using UnityEngine;
using System.Collections;
	
	public class ConnectionTutorial : MonoBehaviour {
		public int portNum = 25001;
		public string ipAdd = "127.0.0.1";
		private string role;
		private GameObject player;
		public int fontSize = 30;
		
		void OnGUI()
		{
			GUIStyle myButtonStyle = new GUIStyle(GUI.skin.button);
			GUIStyle myButtonStyle1 = new GUIStyle(GUI.skin.button);
			myButtonStyle.fontSize = 70;
			myButtonStyle1.fontSize = 30;
			GUI.depth = 0;
			if(Network.peerType == NetworkPeerType.Disconnected)
			{
				GUI.Label(new Rect(320,15,480,60),"Status:Disconnected",myButtonStyle1);
				GUI.Label(new Rect(25,15,280,60),Network.player.ipAddress,myButtonStyle1);
				ipAdd=GUI.TextField(new Rect(112,480,800,130),ipAdd,myButtonStyle);
				if(GUI.Button(new Rect(112,320,800,130),"Connect client",myButtonStyle))
				{
					role = "Guard";
					Debug.Log("in connect");
					Network.Connect(ipAdd,portNum);
					//				ConnectionTesterStatus ct = Network.TestConnection();
					//				while(ct.Equals(ConnectionTesterStatus.Undetermined))
					//				Debug.Log(ct);
					//				Debug.Log(ct);
				}
				
				if(GUI.Button(new Rect(112,160,800,130),"Initialize server",myButtonStyle))
				{
					Network.InitializeServer(8,portNum,false);
					role = "Thief";
					//MasterServer.RegisterHost("MuseumHeist","Guard1");
					//networkView.RPC("startGame",RPCMode.All,role);
				}
				
			}
			else if(Network.peerType == NetworkPeerType.Client)
			{
				GUI.Label(new Rect(100,100,100,100),"Connected as client");
				if(GUI.Button(new Rect(112,240,100,100),"Disconnect"))
					Network.Disconnect(200);
			}
			else if(Network.peerType == NetworkPeerType.Server)
			{
				GUI.Label(new Rect(25,15,280,60),"Server ready",myButtonStyle1);
				if(GUI.Button(new Rect(112,240,800,180),"Disconnect",myButtonStyle))
					Network.Disconnect(200);
			}
		}
		// Use this for initialization
		void Start () {
		}
		
		// Update is called once per frame
		void Update () {
			
		}
		void OnConnectedToServer()
		{
			Debug.Log ("in onconnected");
			networkView.RPC("startGame",RPCMode.All,role);
		}
		[RPC]
		public void startGame(string role) 
		{
			Application.LoadLevel("MuseumScene");
			Debug.Log (role);
		}
	}

	
	
	
	/*using UnityEngine;
using System.Collections;

public class ConnectionTutorial : MonoBehaviour {
	public int portNum = 25001;
	public string ipAdd = "127.0.0.1";
	private string role;
	private GameObject player;
	void OnGUI()
	{
		GUI.depth = 0;
		if(Network.peerType == NetworkPeerType.Disconnected)
		{
			GUI.Label(new Rect(100,100,100,100),"Status:Disconnected");
			GUI.Label(new Rect(0,50,50,50),Network.player.ipAddress);
			ipAdd=GUI.TextField(new Rect(50,100,100,100),ipAdd);
			if(GUI.Button(new Rect(100,240,100,100),"Connect client"))
			{
				role = "Guard";
				Debug.Log("in connect");
				Network.Connect(ipAdd,portNum);
//				ConnectionTesterStatus ct = Network.TestConnection();
//				while(ct.Equals(ConnectionTesterStatus.Undetermined))
//				Debug.Log(ct);
//				Debug.Log(ct);
			}
			
			if(GUI.Button(new Rect(100,350,100,100),"Initialize server"))
			{
				Network.InitializeServer(8,portNum,false);
				role = "Thief";
				//MasterServer.RegisterHost("MuseumHeist","Guard1");
				//networkView.RPC("startGame",RPCMode.All,role);
			}

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
	}
	// Use this for initialization
	void Start () {
	}
	
	// Update is called once per frame
	void Update () {
	
	}
	void OnConnectedToServer()
	{
		Debug.Log ("in onconnected");
		networkView.RPC("startGame",RPCMode.All,role);
	}
	[RPC]
	public void startGame(string role) 
	{
		Application.LoadLevel("MuseumScene");
		Debug.Log (role);
	}
}*/

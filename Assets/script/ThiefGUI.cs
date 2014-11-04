using UnityEngine;
using System.Collections;

public class ThiefGUI : MonoBehaviour {
	
	public Texture2D lockpick,disalarm,stealth,cracker;
	public GameObject player;
	public float timer = 6.0f;
	public float duration = 10.0f;
	public static float finalTime = 3.0f;
	public float t = 0;
	public GameObject door;
	public Collider2D col;
	public static int key = 0;
	public int fontsize = 30;
	public static bool flags = true;
	public static bool isTouchAlarm = false;
	public static int random = 0;
	public int artifactNum = 16;
	public bool turnOnTime;
	public static bool turnOffLight = false;
	// Use this for initialization
	void Awake()
	{
		random = Random.Range (1, artifactNum);
	}
	void Start () {
		
	}
	void Update(){
		if(flags == false){
			timer -= Time.deltaTime;
			if (timer < 0) {
				timer = 0;
			}
		}
		
		if (ThiefControl.enterArtifact == true) {
			finalTime -= Time.deltaTime;
			if(finalTime<0){
				finalTime = 0;
			}
		}
		
		t = Time.deltaTime;
	}
	
	
	void OnGUI () {
		GUI.depth = 1;
		if(Network.isClient)
		{
			GUI.depth =0;
			player = GameObject.Find("ThiefChild");
			//			Debug.Log (player);
			door = GameObject.Find("Door");
			col = player.collider2D;
			GUIStyle myButtonStyle = new GUIStyle(GUI.skin.button);
			myButtonStyle.fontSize = 40;
//			GUI.TextField (new Rect (800, 180, 200, 100), "finalArtifact:" + random.ToString ());
			GUI.Button (new Rect (800, 10, 200, 100), "COINS:" + ThiefControl.money.ToString (),myButtonStyle);
//			if (GUI.Button (new Rect (100,180, 100, 100), lockpick)) {
//				key = 1;
//			}
			
			if (GUI.Button (new Rect (300,565, 150, 150), disalarm)) {
				//			light.light.color = color2;
				//send message to guard;
				isTouchAlarm = true;
			}
			if (GUI.Button (new Rect (450,565, 150, 150), stealth)) {
				if(flags == true){
					player.renderer.enabled = false;
					flags = false;
					networkView.RPC("hidePlayer",RPCMode.All);
					
				}
			}
			
			
			if(timer == 0){
				player.renderer.enabled = true;
				flags = true;
				timer = 6.0f;
				networkView.RPC("setPlayerTagBack",RPCMode.All);
			}
			if (flags == false) {
				GUI.Box(new Rect(450,10,100,100),""+timer.ToString("0"),myButtonStyle);
			}
			
			if(ThiefControl.enterArtifact ==  true&&finalTime>0){
				Vector2 finalPos = ThiefControl.pos;
				Vector2 screenPos = Camera.main.WorldToScreenPoint(finalPos);
				GUI.Box(new Rect(screenPos.x,screenPos.y,100,100),""+finalTime.ToString("0"),myButtonStyle);
			}
			
			if(GUI.Button(new Rect(600,565,150,150),cracker)){
				//turnOffLight = true;
				if(ThiefControl.inLightSight){
					Debug.Log("in light: " );
					lightScript lScript = (lightScript)ThiefControl.inLightSightCol.GetComponent("lightScript");
					//col.GetComponent<lightScript>().isOn = false;
					Debug.Log("thiefGui_light name: "+ThiefControl.inLightSightCol.name);
					ThiefControl.inLightSightCol.collider2D.enabled = false;
					lScript.isOn = false;
					ThiefControl.inLightSight = false;
					
					
					networkView.RPC("turnOffLight1",RPCMode.Server,ThiefControl.inLightSightCol.name);
					
				}
			}
		}
		
	}	
	[RPC]
	void turnOffLight1(string colName) {
		GameObject colObj = GameObject.Find(colName);
		lightScript lScript1 = (lightScript)colObj.GetComponent("lightScript");
		lScript1.isOn = false;
		colObj.collider2D.enabled = false;
		GameObject thiefObj = GameObject.Find ("Thief(Clone)/ThiefChild");
		thiefObj.renderer.enabled = false;
	}
	[RPC]
	void hidePlayer()
	{
		GameObject tempPlayer = GameObject.FindGameObjectWithTag ("Player");
		tempPlayer.tag = "thief_hidden";
	}
	[RPC]
	void setPlayerTagBack()
	{
		GameObject tempPlayer = GameObject.FindGameObjectWithTag ("thief_hidden");		
		tempPlayer.tag = "Player";
	}
	// Update is called once per frame
	
}

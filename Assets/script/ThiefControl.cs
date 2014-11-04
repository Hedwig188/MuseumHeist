using UnityEngine;
using System.Collections;

public class ThiefControl : MonoBehaviour {
	public float speed = 3f;
	private EasyJoystick joystick;
	Animator anim;
	Collider2D col;
	GameObject player;
	bool f = false;
	public GameObject light;
	public Color color1;
	public Color color2 = Color.red;
	public static int money = 0;
	public GameObject[] artifacts;
	int length;
	public static bool getFinalArtifact = false;
	public static Vector2 pos;
	public static bool enterArtifact = false;
	public GameObject finalArt;
	private bool touchGuard = false;
	public static bool inLightSight = false;
	public static Collider2D inLightSightCol;
	private GameObject thiefChildObj;
	private float lightTimer;
	
	[RPC]
	void thiefHide() {
		//GameObject thiefObj = GameObject.Find ("Thief(Clone)");
		//thiefObj.renderer.enabled = false;
		thiefChildObj.renderer.enabled = false;
	}
	[RPC]
	void thiefDisplay() {
		//GameObject thiefObj = GameObject.Find ("Thief(Clone)");
		//thiefObj.renderer.enabled = true;
		thiefChildObj.renderer.enabled = true;
	}
	[RPC]
	void setAnimation(int phase) {
		anim.SetInteger ("flag", phase);
	}
	
	void Start(){
		thiefChildObj = GameObject.Find(this.name+"/ThiefChild");
		player = GameObject.Find("ThiefChild");
		anim = thiefChildObj.GetComponent<Animator> ();//player.GetComponent<Animator> ();
		col = player.collider2D; 
		joystick = GameObject.Find("joystick").GetComponent<EasyJoystick>();
		light = GameObject.Find("Point light");
		color1 = light.light.color;
		lightTimer = 0;
		artifacts = GameObject.FindGameObjectsWithTag("artifact");
		if(Network.isClient)
		{
			for(int i = 0;i< artifacts.Length;i++) {
				//			Debug.Log(ThiefGUI.random+"random");
				//			Debug.Log(artifacts[i].GetComponent<ItemRotate>().randomChoose);
				if(artifacts[i].GetComponentInChildren<ItemRotate>().randomChoose == ThiefGUI.random){
					Debug.Log(artifacts[i].name);
					artifacts[i].tag = "finalArtifact";
					
					//GameObject changeColorObj = GameObject.FindGameObjectWithTag("finalArtifact");
					artifacts[i].GetComponentInChildren<SpriteRenderer>().color = Color.green;
					//networkView.RPC("ChooseFinalArtifact",RPCMode.All,i);
				}
			}
		}
	}
	
	void Update()
	{	
		Movement ();
		//		OnTriggerEnter2D (col);
		if (enterArtifact == true && ThiefGUI.finalTime == 0) {
			getFinalArtifact = true;
			Network.Destroy (finalArt);
		}
		
		if (touchGuard == true) {
			Application.LoadLevel("you lose scene");
		}
		if (lightTimer > 0)
			lightTimer -= Time.deltaTime;
		if (lightTimer<=0) {
			Debug.Log("light back");
			light.light.color = color1;
		}
	}
	
	void Movement()
	{
		if (Network.isClient) {
			if (joystick.JoystickAxis.x < -Mathf.Abs (joystick.JoystickAxis.y)) {
				//		if (Input.GetAxisRaw("Horizontal")<0) {
				anim.SetInteger ("flag", 3);
				networkView.RPC ("setAnimation", RPCMode.Server, 3);
				transform.Translate (Vector3.left * speed * Time.deltaTime * Mathf.Abs (joystick.JoystickAxis.x));
			}
			if (joystick.JoystickAxis.x > Mathf.Abs (joystick.JoystickAxis.y)) {
				//		if(Input.GetAxisRaw("Horizontal")>0) {
				anim.SetInteger ("flag", 4);
				networkView.RPC ("setAnimation", RPCMode.Server,4);
				transform.Translate (Vector3.right * speed * Time.deltaTime * Mathf.Abs (joystick.JoystickAxis.x));
			}
			if (joystick.JoystickAxis.y < -Mathf.Abs (joystick.JoystickAxis.x)) {
				anim.SetInteger ("flag", 1);
				networkView.RPC ("setAnimation", RPCMode.Server, 1);
				//		if (Input.GetAxisRaw("Vertical")<0) {
				transform.Translate (Vector3.down * speed * Time.deltaTime * Mathf.Abs (joystick.JoystickAxis.y));
			}
			if (joystick.JoystickAxis.y > Mathf.Abs (joystick.JoystickAxis.x)) {
				anim.SetInteger ("flag", 2);
				networkView.RPC ("setAnimation", RPCMode.Server, 2);
				//		if (Input.GetAxisRaw("Vertical")>0) {
				transform.Translate (Vector3.up * speed * Time.deltaTime * Mathf.Abs (joystick.JoystickAxis.y));
			}
		}
	}
	
	
	
	void OnTriggerEnter2D(Collider2D col){
		if (Network.isClient) {
			Debug.Log ("col: " + col.name);
			GameObject door = GameObject.Find ("Door");
			GameObject laser = GameObject.FindGameObjectWithTag ("laserAlarm");
			
			if (col.gameObject.name == "coins") {
				if (col.gameObject.tag == "finalArtifact") {
					pos = col.gameObject.transform.position;
					enterArtifact = true;
					finalArt = col.gameObject;
					if (ThiefGUI.finalTime == 0) {
						money += 10;
					}
				} else {
					money += 10;
					Network.Destroy (col.gameObject);
				}
			}
			
			if (col.gameObject.name == "magazine") {
				if (col.gameObject.tag == "finalArtifact") {
					if (ThiefGUI.finalTime == 0) {
						money += 20;
					}
					pos = col.gameObject.transform.position;
					
					enterArtifact = true;
					finalArt = col.gameObject;
				} else {
					money += 20;
					Network.Destroy (col.gameObject);
				}
			}
			
			if (col.gameObject.name == "backpack") {
				if (col.gameObject.tag == "finalArtifact") {
					if (ThiefGUI.finalTime == 0) {
						money += 30;
					}
					pos = col.gameObject.transform.position;
					enterArtifact = true;
					finalArt = col.gameObject;
				} else {
					money += 30;
					Network.Destroy (col.gameObject);
				}
			}
			if (col.gameObject.name == "cash") {
				if (col.gameObject.tag == "finalArtifact") {
					pos = col.gameObject.transform.position;
					enterArtifact = true;
					if (ThiefGUI.finalTime == 0) {
						money += 40;
					}
					finalArt = col.gameObject;
				} else {
					money += 40;
					Network.Destroy (col.gameObject);
				}
			}
			
			if (col.gameObject.name == "diamend") {
				if (col.gameObject.tag == "finalArtifact") {
					if (ThiefGUI.finalTime == 0) {
						money += 100;
					}
					pos = col.gameObject.transform.position;
					enterArtifact = true;
					finalArt = col.gameObject;
				} else {
					money += 100;
					Network.Destroy (col.gameObject);
				}
			}
			
			if (col.gameObject.name == "Entrance") {
				if (getFinalArtifact == true) {
					Application.LoadLevel ("thief_winscene");
					networkView.RPC ("RPCLoadGuardLoseScene", RPCMode.Server);
				}
			}
			
			
			if (col.gameObject.tag == "guards") {
				touchGuard = true;
				
			}
			if (col.name.StartsWith ("guard_light")) {
				Debug.Log ("before enter light: " + inLightSight);
				inLightSight = true;
				inLightSightCol = col;
				networkView.RPC ("thiefDisplay", RPCMode.Server);
				
				//Debug.Log("after enter light: " + inLightSight);
				/*if(ThiefGUI.turnOffLight == true){
				Debug.Log("in light: " );
				lightScript lScript = (lightScript)col.transform.gameObject.GetComponent("lightScript");
				//col.GetComponent<lightScript>().isOn = false;
				lScript.isOn = false;
				inLightSight = false;

		}*/
				
			}
			
			
			if (ThiefGUI.key == 1) {
				print (col.gameObject);
				door.collider2D.isTrigger = true;
				if (col.gameObject == door) {
					
					Network.Destroy (col.gameObject);
				}
			}
			if (ThiefGUI.isTouchAlarm == false) {
				if (col.gameObject.tag == "laserAlarm") {
					light.light.color = color2;	
					networkView.RPC("LaserAlarm",RPCMode.All);
					Debug.Log("laser detected");
				}
				
			}
			
			if (ThiefGUI.isTouchAlarm == true) {
				
				if (col.gameObject.tag == "laserAlarm") {
					Network.Destroy (col.gameObject);
					ThiefGUI.isTouchAlarm = false;
				}
				
			}
			
		}
	}
	
	void OnTriggerExit2D(Collider2D col){
		if (Network.isClient) {
			if (finalArt && finalArt.tag == "finalArtifact") {
				enterArtifact = false;
				if (ThiefGUI.finalTime < 3.0f) {
					ThiefGUI.finalTime = 3.0f;
				}
			}
			
			if (col.name.StartsWith ("guard_light")) {
				ThiefGUI.turnOffLight = false;
				inLightSight = false;
				networkView.RPC ("thiefHide", RPCMode.Server);
				Debug.Log ("after exit light: " + inLightSight);
			}
			
		}
	}
	public static void changePlayerScene()
	{
		Application.LoadLevel ("thief_losescene");
	}
	
	[RPC]
	void LaserAlarm()
	{
		Debug.Log ("server laser");
		GuardGUI.displayLaserAlarm = true;
		Debug.Log (GuardGUI.displayLaserAlarm);
		float offset = 50;
		GameObject player = GameObject.FindGameObjectWithTag ("Player");
		float displayTime = 5.0f;
		//		if (Network.isServer) {
		//			while (displayTime>0) {
		//				player.renderer.enabled = true;
		//				displayTime -= Time.deltaTime;
		//				Debug.Log (displayTime);
		//			}
		//			GuardGUI.displayLaserAlarm = false;
		//			player.renderer.enabled = false;
		Debug.Log("in laseralarm");
		GuardGUI.timer = 5.0f;
		lightTimer = 5.0f;
	}
	[RPC]
	void RPCLoadGuardLoseScene()
	{
		Debug.Log ("change guard sceNE LOSE");
		New_Guard_script.LoadGuardLoseScene ();
	}
	void ChooseFinalArtifact(int i)
	{
		artifacts[i].tag = "finalArtifact";
	}
	
	
	
	
}
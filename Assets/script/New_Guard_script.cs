using UnityEngine;
using System.Collections;

public class New_Guard_script : MonoBehaviour {
	
	// Use this for initialization
	void Start () {
		coneObj = GameObject.Find(this.name+"/cone");
		anim = this. GetComponentInChildren<Animator> (); 
		anim.SetInteger ("flag", 1);
		if(Network.isServer)
		{
			Debug.Log (this.name);
			objIndex = int.Parse(this.name);
			//print ("ss"+objIndex);
			gameObj = GameObject.Find("GUIGuard");
			Debug.Log (gameObj.transform.gameObject.GetComponent("GuardGUI"));
			//gameObj.tag = "Player";
			//print (gameObj.tag);
			script1 = (GuardGUI)gameObj.transform.gameObject.GetComponent("GuardGUI");
			//newTag = "waypoint" +  objIndex;


			patrolFwd = true;
			//anim = this. GetComponentInChildren<Animator> (); 
			//anim.SetInteger ("flag", 1);
			//print (anim.GetInteger("flag"));
		}
	}
	
	// Update is called once per frame
	void Update () {
		//print (newTag);
		if(Network.isServer)
		{
			moveOrPatrol = script1.statusOfGuards [objIndex, 0];
			noOfNodes = script1.statusOfGuards [objIndex, 2];
			if (script1.statusOfGuards[objIndex, 1] == 0) {
				//print ('test99');
				return;		
			}
			
			if (indexOfNode < noOfNodes) {
				//print (script1.statusOfGuards [objIndex, 2]);
				if (firstPrePos) {
					prePos = script1.nodesArr[objIndex][0].transform.position;
					firstPrePos = false;
				}
				if (firstWalk && moveOrPatrol == 0) {
					Destroy(script1.nodesArr[objIndex][0], 0.0f);
					firstWalk = false;
				}
				walk (script1.nodesArr[objIndex][indexOfNode]);	
				changeIndex();

				//print ("walk, "+indexOfNode);
			}
		}
		
	}
	
	public void initializeGuard() {
		firstTime = true;
		indexOfNode = 1;
		patrolFwd = true;
		isFirstCol = true;
		firstWalk = true;
		firstPrePos = true;
	}
	[RPC]
	void guardAnimation(int phase) {
		anim.SetInteger("flag",phase);
		//GameObject clientGObj = GameObject.Find(gName);
		//New_Guard_script scriptC = (New_Guard_script)clientGObj.transform.gameObject.GetComponent ("New_Guard_script");


	}
	[RPC]
	void guardConeRotation(float angle) {
		coneObj.transform.rotation = Quaternion.AngleAxis(angle, Vector3.forward);
		//GameObject clientGObj = GameObject.Find(gName);
		//New_Guard_script scriptC = (New_Guard_script)clientGObj.transform.gameObject.GetComponent ("New_Guard_script");
		
		
	}
	void walk(GameObject curWaypointObj) {
		//print ("walk");
		targetPos = curWaypointObj.transform.position;
		Vector3 waypointDirection = targetPos - prePos;
		//print ("targetPos: "+targetPos);
		//print ("prePos: "+prePos);
		//print ("waypointDirection: "+waypointDirection);
		//prePos = targetPos;
		waypointDirection = waypointDirection.normalized;
		//int ang;
		/*Vector3 ori = new Vector3 (0,0,1);
		if (waypointDirection.x < 0) {
			//ori.Set(-1,0,0);
			print ("rotation"+ori);
			this.transform.LookAt (curWaypointObj.transform.position, ori);
		//	ang = 1;
		}
		else if(waypointDirection.x > 0) {
			//ori.Set(1,0,0);
			this.transform.LookAt (curWaypointObj.transform.position, ori);
			//this.transform.LookAt (1,transform.position.y,transform.position.z);
		//	ang = 3;
		}
		else if (waypointDirection.y < 0) {
			//ori.Set(0,-1,0);
			this.transform.LookAt (curWaypointObj.transform.position, ori);
		//	ang = 0;
			//this.transform.LookAt (transform.position.x,-1,transform.position.z);
		}
		else {
			//ori.Set(0,1,0);
			this.transform.LookAt (curWaypointObj.transform.position, ori);
			//this.transform.LookAt (transform.position.x,1,transform.position.z);
		//	ang = 2;
		}*/
		//
		float ang = Mathf.Atan2(waypointDirection.y, waypointDirection.x) * Mathf.Rad2Deg + 90;
		networkView.RPC("guardConeRotation",RPCMode.Others,ang);
		coneObj.transform.rotation = Quaternion.AngleAxis(ang, Vector3.forward);
		//transform.Rotate (Vector3. * -90);
		// = Quaternion.Euler(0, 0, AngleDeg);

		if (waypointDirection.x <= -Mathf.Abs (waypointDirection.y)) {//left
			anim.SetInteger("flag",3);

			networkView.RPC("guardAnimation",RPCMode.Others,3);


			print (anim.GetInteger("flag"));
			transform.Translate (waypointDirection.x*Time.deltaTime * speed,waypointDirection.y*Time.deltaTime * speed,0);
		}
		else if (waypointDirection.x >= Mathf.Abs (waypointDirection.y)) {//right
			anim.SetInteger("flag",4);
			print (anim.GetInteger("flag"));
			networkView.RPC("guardAnimation",RPCMode.Others,4);
			transform.Translate (waypointDirection.x*Time.deltaTime * speed,waypointDirection.y*Time.deltaTime * speed,0);
		}
		else if (waypointDirection.y < -Mathf.Abs (waypointDirection.x)) {//down
			anim.SetInteger("flag",1);
			print (anim.GetInteger("flag"));
			networkView.RPC("guardAnimation",RPCMode.Others,1);
			transform.Translate (waypointDirection.x*Time.deltaTime * speed,waypointDirection.y*Time.deltaTime * speed,0);
		}
		else  {//up(waypointDirection.y > Mathf.Abs (waypointDirection.x))
			anim.SetInteger("flag",2);
			print (anim.GetInteger("flag"));
			networkView.RPC("guardAnimation",RPCMode.Others,2);
			transform.Translate (waypointDirection.x*Time.deltaTime * speed,waypointDirection.y*Time.deltaTime * speed,0);
		}
		
	}
	void changeIndex() {
		float dis = Vector3.Distance (this.transform.position, targetPos);
		//print ("node pos "+indexOfNode + ": "+script1.nodesArr[objIndex][indexOfNode].transform.position);
		if (dis < 0.1f){//this.transform.position == targetPos) {
			if(temp && Vector3.Distance (this.transform.position, tempPos) < 0.1f) {
				return;
			}
			else {
				temp = true;
				tempPos = targetPos;
			}
			if(Vector3.Distance (this.transform.position, tempPos) >= 0.1f) {
				temp = false;
			}


			if (moveOrPatrol == 0) {
				if (indexOfNode >= noOfNodes) {
					return;
				}
				//Destroy(script1.nodesArr[objIndex][0], 0.0f);
				Destroy(script1.nodesArr[objIndex][indexOfNode], 0.0f);//
				prePos = targetPos;
				indexOfNode++;
				//print (indexOfNode);
			}
			if (moveOrPatrol == 1) {
				print ("moveOrPatrol:"+moveOrPatrol+", ");

				if (patrolFwd) {
					//if (temp) {
						prePos = targetPos;//}
					//print ("FwdPrePos: " + prePos);
					//temp = true;
					indexOfNode++;
					//print ("fwd prePos Index: " + indexOfNode);
					if (indexOfNode >= noOfNodes) {
						patrolFwd = false;
						//temp = false;
						indexOfNode = indexOfNode - 2;
					}
					//print ("fwd:"+indexOfNode+", ");
				}
				else {
					//if (temp) {
						prePos = targetPos;
					//print ("BkwPrePos: " + prePos);
					//}
					//temp = true;
					indexOfNode--;
					//print ("bkw prePos Index: " + indexOfNode);
					if (indexOfNode < 0) {
						patrolFwd = true;
						//temp = false;
						indexOfNode = indexOfNode + 2;
					}
					//print ("bwd:"+indexOfNode+", ");
				}
				
			}	
		}
	}
	void OnTriggerEnter2D (Collider2D col)
	{   if(Network.isServer)
		{
		Debug.Log ("in guard collider");
		if (string.Equals (col.tag, "Player")) {

				Debug.Log("in on trigger");
				Application.LoadLevel("guard_winscene");
			networkView.RPC("changePlayerScene",RPCMode.Others);
		

		}
		}
		
	/*	if (string.Equals(col.tag, newTag)) {//(col.tag == newTag) {
			print ("collide ");
			//print ("old: "+objID+",  New: "+col.GetInstanceID());
			
			
			if (isFirstCol) {
				objID = col.GetInstanceID();
				isFirstCol = false;
				
			} else {
				if (col.GetInstanceID() == objID) {
					return;
				}
				else {
					objID = col.GetInstanceID();
				}
			}
			
			
			
			
			if (firstTime) {
				indexOfNode++;
				print ("firstIndex:"+indexOfNode+", ");
				firstTime = false;
				return;
			}
			//indexOfNode++;
			//print (script1.statusOfGuards [objIndex, 0]);
			if (moveOrPatrol == 0) {
				if (indexOfNode >= noOfNodes) {
					return;
				}
				//Destroy(script1.nodesArr[objIndex][0], 0.0f);
				Destroy(script1.nodesArr[objIndex][indexOfNode], 0.0f);//
				indexOfNode++;
				//print (indexOfNode);
			}
			if (moveOrPatrol == 1) {
				print ("moveOrPatrol:"+moveOrPatrol+", ");
				if (patrolFwd) {
					indexOfNode++;
					if (indexOfNode >= noOfNodes) {
						patrolFwd = false;
						indexOfNode = indexOfNode - 2;
					}
					print ("fwd:"+indexOfNode+", ");
				}
				else {
					indexOfNode--;
					if (indexOfNode < 0) {
						patrolFwd = true;
						indexOfNode = indexOfNode + 2;
					}
					print ("bwd:"+indexOfNode+", ");
				}
				
			}
		}*/
	}
	public static void LoadGuardLoseScene()
	{
		Application.LoadLevel ("guard_losescene");
	}
	[RPC]
	void changePlayerScene()
	{
		Debug.Log ("thiefloses");
		ThiefControl.changePlayerScene ();
	}

	private bool firstTime = true;
	private int objIndex;
	private GameObject gameObj;
	private GuardGUI script1;
	//private float rotationDamping = 1.0f;
	//private float accel = 1.6f;
	private bool firstWalk = true;
	private const float speed = 0.4f;
	private int moveOrPatrol;
	private int noOfNodes;
	private int indexOfNode = 1;
	private bool patrolFwd;
	private string newTag;
	private Animator anim;
	private int objID;
	private bool isFirstCol = true;
	private Vector3 prePos;
	private Vector3 targetPos;
	private Vector3 tempPos;
	private bool temp = false;
	private bool firstPrePos = true;
	public GameObject coneObj;// = GameObject.Find(this.name+"/cone");
	
}

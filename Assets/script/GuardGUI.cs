using UnityEngine;
using System.Collections;

public class GuardGUI : MonoBehaviour {

	public GameObject[] guards = new GameObject[100];
	public GameObject[] cameras = new GameObject[100];
	public GameObject[] laser_traps = new GameObject[100];
	public int indexOfGuard = 0;
	public int indexOfCamera = 0;
	public int indexOfLaser = 0;
	
	public int guardMenuStartIndex = 0;
	public int guardMenuOffset = 0;
	public int[,] statusOfGuards = new int[100,3]; 
	public GameObject[][] nodesArr = new GameObject[100][];
	public GameObject[] tempNode = new GameObject[100];
	public bool eachGuardBtnPressed = false;
	public bool moveBtnPressed = false;
	public bool patrolBtnPressed = false;
	public bool nodesBtnPressed0 = false;
	public bool nodesBtnPressed1 = false;
	public GUIStyle moveBtn;
	public GUIStyle	patrolBtn;
	public int guardNumber;
	private int nodePlacementIndex = 1;
	public GameObject patrolNodePrefab;
	private Ray ray; //**
	
	private Vector2 placementPos;
	private Vector2 firstNodePos;
	
	public Texture2D eachGuardBtn;
	public Texture2D toolWindow;
	private int tempGuardStatus;
	private const float xyBoundary = 0.2f;
	private const float vicinityBoundary = 0.7f;
	private int guardCursor=0;
	private bool clearMenu = true;
	public static bool displayLaserAlarm;
	public static float timer;
	
	//private Vector3 currentPos;
	
	
	//public GameObject[] nodesArr= new GameObject[4];  
	//public GameObject wp0Obj;
	//public GameObject wp1Obj;
	//public GameObject wp2Obj;
	//public GameObject wp3Obj;
	//private int nuOfNodes = 3;
	//public GameObject twoPatrolNodes;
	//public GameObject threePatrolNodes;
	//public GameObject fourPatrolNodes;
	
	//private int moveOrPatrol = 0;
	//private bool startWalk = false;
	
	private const int GUARD = 1;
	private const int CAMERA = 2;
	private const int LASER_TRAP = 3;
	
	private int noOfGuards = 3;
	private int noOfCameras = 3;
	private int noOfLaserTraps = 3;
	
	private int coinsOfGuard = 200;
	private int coinsOfThief = 200;
	private int objectToPlace;
	private RaycastHit hit;
	private int indexOfNodes = 0;
	private bool menuOfGuard = false;
	
	
	public GameObject guardObj;
	public GameObject cameraObj;
	public GameObject laserTrapObj;
	
	public Texture2D placeGuardBtn;
	public Texture2D placeCameraBtn;
	public Texture2D placeLaserTrapBtn;



	// Use this for initialization
	void Start () {

		placementPos = new Vector2(0, 0);
		firstNodePos = new Vector2(0, 0);
		displayLaserAlarm = false;
	}
	
	// Update is called once per frame
	void Update () {


		//clearMenu = true;
		//ray = Camera.main.ScreenPointToRay (Input.mousePosition);
		//placementPos     = ray.origin + (ray.direction * 28);  

		//placementPos.Set (Input.GetTouch (0).position.x, Input.GetTouch (0).position.y);
		if(Network.isServer)
		{
			Debug.Log("in server udate");
		placementPos.Set(Input.mousePosition.x, Input.mousePosition.y); 
		placementPos = Camera.main.ScreenToWorldPoint(placementPos);
		//print(placementPos);  // debug
		if (Input.GetMouseButtonDown (0)) {
			//if (Physics.Raycast (ray, out hit, Mathf.Infinity)) {
				if (true) {//hit.collider.tag != "ground" && hit.collider.tag != "enemyAim" && hit.collider.tag != "temple")
					if (objectToPlace == GUARD) { 
						guards[indexOfGuard] = Network.Instantiate(guardObj,placementPos,Quaternion.identity,0) as GameObject;
						Debug.Log ("create guard "+indexOfGuard);
						networkView.RPC("ChangeGuardName",RPCMode.All,indexOfGuard);
						//tempNode[indexOfGuard] = (GameObject)Instantiate(patrolNodePrefab,placementPos,Quaternion.identity);
						
						//tempNode[indexOfGuard].tag = "waypoint" + indexOfGuard;

						statusOfGuards[indexOfGuard, 1] = 0;
						//statusOfGuards[indexOfGuard, 3] = 1;
						indexOfGuard++;
						objectToPlace = 0;
						nodesBtnPressed0 = false;
						nodesBtnPressed1 = false;
						//clearMenu = false;
						//print (indexOfGuard);
						//print ("tse");
					}
				if (objectToPlace == CAMERA) {
						cameras[indexOfCamera] =Network.Instantiate(cameraObj,placementPos,Quaternion.identity,0)as GameObject;
					//cameras[indexOfCamera].name = "guard_light" + indexOfCamera;
						networkView.RPC("ChangeLightName",RPCMode.All, indexOfCamera);
	
					indexOfCamera++;
					objectToPlace = 0;
					//clearMenu = false;
					//nodesBtnPressed0 = false;
					//nodesBtnPressed1 = false;
					//print (indexOfGuard);
					//print ("tse");
				}
				if (objectToPlace == LASER_TRAP) {
						laser_traps[indexOfLaser] = Network.Instantiate(laserTrapObj,placementPos,Quaternion.identity,0)as GameObject;
					//guards[indexOfGuard].name = indexOfGuard + "";
					//statusOfGuards[indexOfGuard, 3] = 1;
					indexOfLaser++;
					objectToPlace = 0;
					//clearMenu = false;
				}

					if (nodesBtnPressed1) {
						//clearMenu = false;
						float oldX = nodesArr[guardNumber][nodePlacementIndex-1].transform.position.x;
						float oldY = nodesArr[guardNumber][nodePlacementIndex-1].transform.position.y;
						float newX = placementPos.x;
						float newY = placementPos.y;
					if ( ((oldX - xyBoundary < newX) && (oldX + xyBoundary > newX)) && ((newY < oldY - vicinityBoundary) || (newY > oldY + vicinityBoundary)) ){
							placementPos.Set(oldX, newY);
						}
					else if (((oldY - xyBoundary < newY) && (oldY + xyBoundary > newY)) && ((newX < oldX - vicinityBoundary) || (newX > oldX + vicinityBoundary))) {
							placementPos.Set(newX, oldY);
						}
						else {
							return;
						}


						if (nodePlacementIndex < statusOfGuards[guardNumber, 2]){
							nodesArr[guardNumber][nodePlacementIndex] =Instantiate(patrolNodePrefab,placementPos,Quaternion.identity)as GameObject;
							nodesArr[guardNumber][nodePlacementIndex].transform.gameObject.tag = "waypoint" + guardNumber;
							nodePlacementIndex++;
						}
						if (nodePlacementIndex == statusOfGuards[guardNumber, 2]) {
							nodesBtnPressed1 = false;
							statusOfGuards[guardNumber, 1] = 1;
							nodePlacementIndex = 1;
						}

					}
				}
			//}

		}
			if(timer>0)
				timer-=Time.deltaTime;

		}
	}
	void guardCursorChange(int guardCursorNo) {
		GameObject cursorObj = GameObject.Find(guardCursor+"/cursor");
		cursorObj.renderer.enabled = false;
		guardCursor = guardCursorNo;
		cursorObj = GameObject.Find(guardCursor+"/cursor");
		cursorObj.renderer.enabled = true;

	}
	void OnGUI() {
		GUIStyle font0 = new GUIStyle(GUI.skin.button);
		font0.fontSize = 30;
		GUIStyle font1 = new GUIStyle(GUI.skin.button);
		font1.fontSize = 40;
		font1.normal.textColor = Color.red;
		//GUI.Label (new Rect (220, 270, 600, 40), toolWindow);  // don't delete this line
		if(Network.isServer)
		{
			GUI.depth = 0;
			Debug.Log("in server ongui");
			Debug.Log(displayLaserAlarm);
			if(displayLaserAlarm && timer>0)
			{
				Debug.Log("display laser message");
				GUI.Label(new Rect(320,10,460,100),"LASER BROKEN",font1);
			}
			if (GUI.Button (new Rect (300,565,150,150), placeGuardBtn)) {
			//clearMenu = true;
			if(objectToPlace == GUARD) {
				objectToPlace = 0;
			}
			else {
				objectToPlace = GUARD;
			}
		}

		if (GUI.Button (new Rect (450,565,150,150), placeCameraBtn)) {
			//clearMenu = true;
			if(objectToPlace == CAMERA) {
				objectToPlace = 0;
			}
			else {
				objectToPlace = CAMERA;
			}

		}
		if (GUI.Button (new Rect (600,565,150,150), placeLaserTrapBtn)) {
			//clearMenu = true;
			if(objectToPlace == LASER_TRAP) {
				objectToPlace = 0;
			}
			else {
				objectToPlace = LASER_TRAP;
			}
		}

		if (indexOfGuard > 0) {
			if (indexOfGuard == 1) {
				if (GUI.Button (new Rect (840, 140, 130, 130), eachGuardBtn)) {
					//clearMenu = false;
					guardMenuOffset = 0;
					guardCursorChange(guardMenuStartIndex + guardMenuOffset);
					eachGuardBtnPressed = true;
					moveBtnPressed = false;
					patrolBtnPressed = false;
					//clearMenu = false;
					//indexOfNodes++;
				}
			}
			if (indexOfGuard == 2) {
					if (GUI.Button (new Rect (840, 140, 130, 130), eachGuardBtn)) {//clearMenu = false;
					guardMenuOffset = 0;
					guardCursorChange(guardMenuStartIndex + guardMenuOffset);
					eachGuardBtnPressed = true;
					moveBtnPressed = false;
					patrolBtnPressed = false;
					//clearMenu = false;
					//indexOfNodes++;
				}
					if (GUI.Button (new Rect (840, 270, 130, 130),  eachGuardBtn)) {//clearMenu = false;
					guardMenuOffset = 1;
					guardCursorChange(guardMenuStartIndex + guardMenuOffset);
					eachGuardBtnPressed = true;
					moveBtnPressed = false;
					patrolBtnPressed = false;
					//clearMenu = false;
					//indexOfNodes++;
				}
			}
			if (indexOfGuard > 2) {
				if(guardMenuStartIndex != 0){
						if (GUI.Button (new Rect (840, 40, 130, 100), "Up",font0)) {
						//clearMenu = true;
					if(guardMenuStartIndex>0) {
						guardMenuStartIndex--;
					}

				}
				}


					if (GUI.Button (new Rect (840, 140, 130, 130), eachGuardBtn)) {//clearMenu = false;
					guardMenuOffset = 0;
					guardCursorChange(guardMenuStartIndex + guardMenuOffset);
					eachGuardBtnPressed = true;
					moveBtnPressed = false;
					patrolBtnPressed = false;
					//indexOfNodes++;
				}
					if (GUI.Button (new Rect (840, 270, 130, 130),  eachGuardBtn)) {//clearMenu = false;
					guardMenuOffset = 1;
					guardCursorChange(guardMenuStartIndex + guardMenuOffset);
					eachGuardBtnPressed = true;
					moveBtnPressed = false;
					patrolBtnPressed = false;
					//indexOfNodes++;
				}
					if (GUI.Button (new Rect (840, 400, 130, 130),  eachGuardBtn)) {//clearMenu = false;
					guardMenuOffset = 2;
					guardCursorChange(guardMenuStartIndex + guardMenuOffset);
					eachGuardBtnPressed = true;
					moveBtnPressed = false;
					patrolBtnPressed = false;
					//indexOfNodes++;
				}

				if(guardMenuStartIndex != indexOfGuard - 3){
					//clearMenu = true;
						if (GUI.Button (new Rect (840, 530, 130, 100), "Down",font0)) {
					if(guardMenuStartIndex < indexOfGuard - 3){guardMenuStartIndex++;}
				}
				}
			}
			 
		}

		if (eachGuardBtnPressed) {
			//if (GUI.Button (new Rect (480, 60, 60, 30), "", moveBtn)) {
				if (GUI.Button (new Rect (710, 300, 120, 60), "move",font0)) {//clearMenu = false;
				moveBtnPressed = true;
				patrolBtnPressed = false;
				tempGuardStatus = 0;
				//statusOfGuards[guardMenuStartIndex + guardMenuOffset, 0] = 0;
				//closeMenus = false;
			}
			//if (GUI.Button (new Rect (480, 100, 60, 30), "", patrolBtn)) {
				if (GUI.Button (new Rect (710, 380, 120, 60), "patrol",font0)) {//clearMenu = false;
				patrolBtnPressed = true;
				moveBtnPressed = false;
				tempGuardStatus = 1;
				//statusOfGuards[guardMenuStartIndex + guardMenuOffset, 0] = 1;
				//closeMenus = false;
			}

		}

		if (moveBtnPressed || patrolBtnPressed) {
				if (GUI.Button (new Rect (570, 280, 130, 60), "2 nodes",font0)) { 
				nodesBtnPressed0 = true;
				//statusOfGuards[guardMenuStartIndex + guardMenuOffset, 1] = 1;
				statusOfGuards[guardMenuStartIndex + guardMenuOffset, 2] = 2;
				//closeMenus = false;
			}
				if (GUI.Button (new Rect (570, 350, 130, 60), "3 nodes",font0)) { 
				nodesBtnPressed0 = true;
				//statusOfGuards[guardMenuStartIndex + guardMenuOffset, 1] = 1;
				statusOfGuards[guardMenuStartIndex + guardMenuOffset, 2] = 3;
				//closeMenus = false;
				 
			}
				if (GUI.Button (new Rect (570, 420, 130, 60), "4 nodes",font0)) {
				nodesBtnPressed0 = true;
				//statusOfGuards[guardMenuStartIndex + guardMenuOffset, 1] = 1;
				statusOfGuards[guardMenuStartIndex + guardMenuOffset, 2] = 4;
				//closeMenus = false;
				 
			}
			if(nodesBtnPressed0) {

				eachGuardBtnPressed = false;
				moveBtnPressed = false;
				patrolBtnPressed = false;
				guardNumber = guardMenuStartIndex + guardMenuOffset;
				statusOfGuards[guardNumber, 0] = tempGuardStatus;
				statusOfGuards[guardNumber, 1] = 0;
				//initializeGuard()
				GameObject gameObj = GameObject.Find(guardNumber + "");
				//gameObj.tag = "Player";
				//print (gameObj.tag);
				New_Guard_script script1 = (New_Guard_script)gameObj.transform.gameObject.GetComponent("New_Guard_script");
				script1.initializeGuard();

				if(nodesArr[guardNumber]!= null){
					for (int i = 0; i < nodesArr[guardNumber].Length; i++) {
						Network.Destroy(nodesArr[guardNumber][i]);
					}
				}
				//print("length: " + nodesArr[guardNumber].Length);
				nodesArr[guardNumber] = new GameObject[statusOfGuards[guardNumber, 2]];

				//tempNode[indexOfGuard] = (GameObject)Instantiate(patrolNodePrefab,placementPos,Quaternion.identity);
				firstNodePos.Set(guards[guardNumber].transform.position.x, guards[guardNumber].transform.position.y);

				nodesArr[guardNumber][0] = Instantiate(patrolNodePrefab,firstNodePos,Quaternion.identity) as GameObject;
				nodesArr[guardNumber][0].tag = "waypoint" + guardNumber;
				nodesBtnPressed0 = false;
				nodesBtnPressed1 = true;
			}
		}
			}

				//if (clearMenu) {
		//	eachGuardBtnPressed = false;		
		//}
	}

	[RPC]
	void ChangeGuardName(int indexG)
	{
		Debug.Log ("in change guard rpc");
		guards [indexG] = GameObject.Find ("Guard(Clone)");
		guards [indexG].name = indexG + "";//indexOfGuard + "";
	}
	[RPC]
	void ChangeLightName(int indexC)
	{
		cameras[indexOfCamera] = GameObject.Find ("light(Clone)");
		cameras [indexOfCamera].name = "guard_light" + indexC;//indexOfCamera;
	}



}




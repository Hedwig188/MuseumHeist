using UnityEngine;
using System.Collections;

public class PlayerController : MonoBehaviour 
{
	
	public float speed;
	private int count;
	public GameObject prefab;
	private GameObject playerClient;
	private NetworkViewID clientID;
	
	void Start()
	{
		
		//nt = 0;
		//ntText.text = "Count: "+count.ToString();
		//winText.text = "";
		//playerPrefab = (GameObject)Resources.Load("playerPrefab");
	}

	void FixedUpdate ()
	{
		//if(networkView.isMine)
		moveBall();
		
	}
	void moveBall()
	{
		if(networkView.isMine)
		{
			float moveHorizontal = Input.GetAxis("Horizontal");
			float moveVertical = Input.GetAxis("Vertical");
			
			//GameObject toMove;
			
			//		if(obj.Equals("clientBall"))
			//		toMove = clientBall;
			//		else
			//		toMove = serverBall;
			
			Vector3 movement = new Vector3(moveHorizontal,0.0f,moveVertical);
			rigidbody.AddForce(movement * speed * Time.deltaTime);
		}
	}
	public void OnConnectedToServer()
	{
		Debug.Log("in onconnectedtoserver");
		prefab = Resources.Load("playerPrefab") as GameObject;
		playerClient = Network.Instantiate(prefab,Vector3.zero, Quaternion.identity,0) as GameObject;
		//playerClient.SetActive (true);
		//			clientID = Network.AllocateViewID();
		//		prefab = Resources.Load("playerPrefab") as GameObject;
		//		playerClient = Network.Instantiate(prefab,Vector3.zero, Quaternion.identity,0) as GameObject;
		
	}
	void OnTriggerEnter( Collider collidedObject)
	{
		Debug.Log ("in collider");
		if (collidedObject.gameObject.tag == "laser") 
		{
			//Debug.Log("in if");
			Color currentColor = Color.blue;
			Color newColor = Color.red;
			GameObject.FindWithTag("mainLight").light.color = Color.Lerp(currentColor,newColor,Time.deltaTime);
		}
	}
	void OnPlayerConnected(NetworkPlayer player)
	{
		//playerClient.SetActive(false);
		Debug.Log ("renderer set false");
	}

	public void OnSerializeNetworkView(BitStream stream, NetworkMessageInfo info)
	{
		Vector3 clientPos = playerClient.transform.position;
		//Vector3 serverPos = transform.position;
		
		if(stream.isWriting)
		{
			stream.Serialize(ref clientPos);
		}
		if(stream.isReading)
		{
			stream.Serialize(ref clientPos);
			playerClient.transform.position = clientPos;
		}
	}
}
using UnityEngine;
using System.Collections;
using KiiCorp.Cloud.Storage;
using System;
using KiiCorp.Cloud.Unity;

public class PlayerController : MonoBehaviour {

	public float speed;
	public float verticalBoost;
	public GUIText countText;
	public GUIText winText;
	public CNJoystick joystick;
	private int count;
	private float gameTime;
	private KiiUser user = null;
	public static string appScopeScoreBucket = "global_scores";
	private bool scoreSent = false;

	void Awake()
	{
		joystick.JoystickMovedEvent += JoystickMovedEventHandler;
		if (Application.platform != RuntimePlatform.Android || Application.platform != RuntimePlatform.IPhonePlayer || Application.platform != RuntimePlatform.WP8Player || Application.platform != RuntimePlatform.TizenPlayer) {
			joystick.CurrentCamera.enabled = false;
		}
	}

	void Start(){
		count = 0;
		if(Kii.AppId != null)
			user = KiiUser.CurrentUser;

		//Set up push listeners
		KiiPushPlugin kiiPushPlugin = GameObject.Find ("KiiPushPlugin").GetComponent<KiiPushPlugin> ();
		Debug.Log ("Found KiiPushPlugin object in game objects");
		kiiPushPlugin.OnPushMessageReceived += (ReceivedMessage message) => {
			// This event handler is called when received the push message.
			switch (message.PushMessageType)
			{
			case ReceivedMessage.MessageType.PUSH_TO_APP:
				Debug.Log ("#####PUSH_TO_APP Message");
				// do something to notify your app of the incomig message
				break;
			case ReceivedMessage.MessageType.PUSH_TO_USER:
				Debug.Log ("#####PUSH_TO_USER Message");
				// your user received a message, do something
				break;
			case ReceivedMessage.MessageType.DIRECT_PUSH:
				Debug.Log ("#####DIRECT_PUSH Message");
				// A direct push message was sent from developer.kii.com
				// Let's grab the url value of the message and open that page
				string url = message.GetString("url");
				Debug.Log ("Url in message is: " + url);
				Application.OpenURL("http://" + url);
				break;
			}
			Debug.Log("Type=" + message.PushMessageType);
			Debug.Log("Sender=" + message.Sender);
			Debug.Log("Scope=" + message.ObjectScope);
			// You can get the value of custom field using GetXXXX method.
			Debug.Log("Payload=" + message.GetString("payload"));
		};

		#if UNITY_IPHONE
		KiiPushInstallation.DeviceType deviceType = KiiPushInstallation.DeviceType.IOS;
		#elif UNITY_ANDROID
		KiiPushInstallation.DeviceType deviceType = KiiPushInstallation.DeviceType.ANDROID;
		#else
		KiiPushInstallation.DeviceType deviceType = KiiPushInstallation.DeviceType.ANDROID;
		#endif

		kiiPushPlugin.RegisterPush((string pushToken, Exception e0)=> {
			if (e0 != null) {
				Debug.Log("#####failed to RegisterPush");
				return;
			}
			Debug.Log ("#####RegistrationId=" + pushToken);
			Debug.Log ("#####Install");
			KiiUser.PushInstallation (true).Install (pushToken, deviceType, (Exception e3)=>{
				if (e3 != null)
				{
					Debug.Log ("#####failed to Install");
					return;
				}
			});
		});
	}

	void Update(){
		ScoreLoop ();
	}

	void FixedUpdate(){
		float moveHorizontal = Input.GetAxis("Horizontal");
		float moveVertical = Input.GetAxis("Vertical");
		float jump = Input.GetKey("space") ? verticalBoost : 0.0f;

		Vector3 movement = new Vector3(moveHorizontal, jump, moveVertical);

		rigidbody.AddForce(movement * speed * Time.deltaTime);
	}

	void OnTriggerEnter(Collider other){
		if(other.gameObject.tag == "PickUp"){
			other.gameObject.SetActive(false);
			count++;
		}
	}

	private void JoystickMovedEventHandler(Vector3 dragVector)
	{
		float moveHorizontal = dragVector.x;
		float moveVertical = dragVector.y;
		float jump = Input.GetKey("space") ? verticalBoost : 0.0f;
		
		Vector3 movement = new Vector3(moveHorizontal, jump, moveVertical);
		
		rigidbody.AddForce(movement * speed * Time.deltaTime);
		//Debug.Log ("x:"+dragVector.x+" y:"+dragVector.y+" z:"+dragVector.z);
		/*dragVector.z = dragVector.y;
		dragVector.y = 0f;
		Vector3 movement = mainCamera.transform.TransformDirection(dragVector);
		movement.y = 0f;
		// Uncomment this line if you want to normalize speed,
		// to keep the speed at a constant value
		// -- UNCOMMENT THIS ---
		// movement.Normalize();
		// ---------------------
		totalMove.x = movement.x * runSpeed;
		totalMove.z = movement.z * runSpeed;
		FaceMovementDirection(movement);*/
	}

	void OnGUI () {
		if(count == 27){
			countText.enabled = false;
			// Make a background box
			GUI.Box(new Rect(10,10,100,120), "Time: " + gameTime.ToString("n2"));
			
			// Make the first button. If it is pressed, Application.Loadlevel (1) will be executed
			if(GUI.Button(new Rect(20,40,80,20), "Retry")) {
				scoreSent = false;
				count = 0;
				Application.LoadLevel(1);
			}
			
			// Make the second button.
			if(GUI.Button(new Rect(20,70,80,20), "Highscores")) {
				Application.LoadLevel(2);
			}

			if(GUI.Button(new Rect(20,100,80,20), "Quit")) {
				Application.Quit();
			}
		}
	}

	void ScoreLoop ()
	{
		if(count < 27)
			gameTime = Time.timeSinceLevelLoad;
		else{
			winText.text = "YOU WIN!!";
			if(user != null && !scoreSent){
				scoreSent = true;
				KiiBucket bucket = Kii.Bucket(appScopeScoreBucket);
				KiiObject score = bucket.NewKiiObject();
				score["time"] = gameTime;
				score["username"] = user.Username;
				// score is game completion time, the lower the better
				Debug.Log ("Saving score...");
				score.Save((KiiObject obj, Exception e) => {
					if (e != null)
						Debug.LogError(e.ToString());
					else
						Debug.Log("Score sent: " + gameTime.ToString("n2"));
				});
			}
		}
		string username = "";
		if(user != null)
			username = user.Username + " ";
		countText.text = username + count.ToString() + "/27 " + gameTime.ToString("n2");
	}
}

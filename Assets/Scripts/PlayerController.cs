using UnityEngine;
using System.Collections;
using KiiCorp.Cloud.Storage;
using System;

public class PlayerController : MonoBehaviour {

	public float speed;
	public float verticalBoost;
	public GUIText countText;
	public GUIText winText;
	private int count;
	private float gameTime;
	private KiiUser user = null;
	public static string appScopeScoreBucket = "global_scores";
	private bool scoreSent = false;

	void Start(){
		count = 0;
		if(Kii.AppId != null)
			user = KiiUser.CurrentUser;
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

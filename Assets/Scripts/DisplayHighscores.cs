using UnityEngine;
using System.Collections;
using KiiCorp.Cloud.Storage;
using System;

public class DisplayHighscores : MonoBehaviour {

	public GUIText highscoresText;
	private KiiQueryResult<KiiObject> scores = null;

	void Start() {
		scores = null;
		GetTop10Highscores();
	}

	void Update() {
		if (Application.platform == RuntimePlatform.Android) {
			if (Input.GetKeyUp(KeyCode.Escape)) {
				Application.LoadLevel(1);
				return;
			}
		}
	}

	void OnGUI () {
		var centeredStyle = GUI.skin.GetStyle("Label");
		centeredStyle.alignment = TextAnchor.UpperCenter;
		centeredStyle.wordWrap = false;

		if(scores == null){
			GUI.Label(new Rect(Screen.width/2-50, Screen.height/2-165, 100, 50), "Loading...", centeredStyle);
			return;
		}

		if(scores.Count == 0){
			GUI.Label(new Rect(Screen.width/2-50, Screen.height/2-165, 100, 50), "Empty", centeredStyle);
			return;
		}

		GUI.Label(new Rect(Screen.width/2-50, Screen.height/2-165, 100, 50), "Highscores", centeredStyle);
		
		GUI.Label(new Rect(Screen.width/2-50, Screen.height/2-150, 100, 50), "", centeredStyle);

		int position = 135;
		foreach (KiiObject obj in scores) {
			double highscore = obj.GetDouble ("time");
			string username = obj.GetString ("username");
			GUI.Label(new Rect(Screen.width/2-50, Screen.height/2-position, 100, 50), username + " - " +highscore.ToString("n2"), centeredStyle);
			position -= 15;
		}
	}

	void GetTop10Highscores(){
		if (Kii.AppId == null || KiiUser.CurrentUser == null) {
			return;
		}
		
		//KiiUser user = KiiUser.CurrentUser;
		KiiBucket bucket = Kii.Bucket (PlayerController.appScopeScoreBucket);
		KiiQuery query = new KiiQuery ();
		query.SortByAsc ("time");
		query.Limit = 10;
		
		bucket.Query(query, (KiiQueryResult<KiiObject> list, Exception e) =>{
			if (e != null)
			{
				Debug.LogError ("Failed to load high scores " + e.ToString());
				scores = null;
			} else {
				scores = list;
			}
		});
	}
}

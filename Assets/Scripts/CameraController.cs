using UnityEngine;
using System.Collections;

public class CameraController : MonoBehaviour {

	public GameObject player;
	private Vector3 offset;

	// Use this for initialization
	void Start () {
		offset = transform.position;
	}
	
	// LateUpdate is best for follow cameras, procedural animations and gathering last know states
	void LateUpdate () {
		transform.position = player.transform.position + offset;
	}

	public void Update () {
		if (Application.platform == RuntimePlatform.Android) {
			if (Input.GetKeyUp(KeyCode.Escape)) {
				//quit application on return button
				Application.Quit();
				return;
			}
		}
	}

}

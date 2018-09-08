using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class KeepHUD : MonoBehaviour {

	// Use this for initialization
	void Start () {
		DontDestroyOnLoad(this.gameObject);
	}
	//TODO: make it so that HUD doesnt go away on load either
	
}

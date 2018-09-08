using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Poof : MonoBehaviour {

	/**
	 * Method that is called by the Animator to destroy the object
	 * once the animation is finished.
	 */ 
	public void DonePoofing() {
		Destroy(gameObject);
	}
}

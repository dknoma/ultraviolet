using System.Collections;
using System.Collections.Generic;
using UnityEngine;

/**
 * For switches that only get activated by the gaze aware blocks
 */ 
public class GazeSwitch : MonoBehaviour {

	public int gateHeight = 4;
	public bool isVerticalGate;
	public bool moveUp;
	public bool moveRight;

	private Transform gate;
	private Animator anim;
	private bool isPressed;
	
	void Start () {
		gate = transform.parent.Find("Gate");	//gets the transform of the gate
		anim = GetComponent<Animator>();
		isPressed = false;
	}

	private void OnCollisionEnter2D(Collision2D coll) {
		// will make it so once the plate is activated, it cant get activated again
		if (coll.gameObject.tag == "MoveBlock" && !isPressed) {			
			GetComponent<BoxCollider2D>().offset = new Vector2(0, -9);	// changes offset of the button
			isPressed = true;
			anim.SetBool("isPressed", isPressed);
			StartCoroutine(moveGate());
		}
	}

	private IEnumerator moveGate() {
		//move the gate up 8 pixels at a time
		for (int i = 0; i < gateHeight * 2; i++) {
			GameMaster.GateOpen();
			if (isVerticalGate) {
				if (moveUp) {
					gate.Translate (new Vector3 (0, 8, 0));
				} else {
					gate.Translate (new Vector3 (0, -8, 0));
				}
			} else {
				if (moveRight) {
					gate.Translate (new Vector3 (8, 0, 0));
				} else {
					gate.Translate (new Vector3 (-8, 0, 0));
				}
			}
			yield return new WaitForSeconds(0.2f);
		}
	}

}

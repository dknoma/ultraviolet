using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class control_grab : MonoBehaviour {

	public Rigidbody2D box;

	private Animator boxAnimator;
	private float halfHeight, halfWidth;

	private float nextTimeToSearch = 0;
	private Player player;
	private CircleCollider2D gazeRadius;
	private float radius;
	private bool hasGaze;

	void Start () {
		boxAnimator = GetComponentInParent<Animator>();
		//box = GetComponentInParent<Rigidbody2D>();
	}


	private void OnTriggerStay2D(Collider2D coll) {
		if (!GameMaster.Biofeedback()) {
			if (coll.gameObject.tag == "GazeRadius") {
				gazeRadius = coll.gameObject.GetComponent<CircleCollider2D>();
			}

			if (coll.gameObject.tag == "gaze_pt") {
				if (player == null) {
					FindPlayer();                   //only find the player if it's null
				}
				
				if (Input.GetAxis(ControllerInputManager.Grab()) > 0 && player != null) {
					boxAnimator.SetBool("hasGaze", true);
					float radius = gazeRadius.radius;
					transform.position = player.pointer.transform.position;
					Vector3 centerPt = player.GetComponentInChildren<CircleCollider2D>().transform.position;

					float distance = Vector3.Distance(player.pointer.transform.position, centerPt);

					if (distance > radius) {                                                            //if distance becomes greater than radius
						Vector3 newPos = transform.position - centerPt;                                 //create vector of difference of cube pos and center pos
						newPos *= radius / distance;                                                    //calc position in circle around center pos
						transform.position = centerPt + newPos;                                         //sets cubes pos to the max distance away from center pos
						box.velocity = (transform.position - box.transform.position) * 10;
						transform.position = box.transform.position;
					} else {
						box.velocity = (transform.position - box.transform.position) * 10;
						transform.position = box.transform.position;
					}
				} else {
					boxAnimator.SetBool("hasGaze", false);
				}
			}
		}
	}

	void OnTriggerExit2D(Collider2D coll) {
		if (!GameMaster.Biofeedback()) {
			if (coll.gameObject.tag == "gaze_pt") {
				boxAnimator.SetBool("hasGaze", false);
			}
		}
	}

	/*
	 * Finds the player object. Used when the player spawns as new instances of the player
	 * will technically be a clone and the camera will disjoint after the player disappears.
	 * 
	 * Makes sure that the target of the script will never be null. Uses nextTimeToSearch
	 * in order to make sure that the method is not done every frame as it would be very
	 * taxing on the cpu.
	 */
	void FindPlayer() {
		if (nextTimeToSearch <= Time.time) {
			GameObject searchResult = GameObject.FindGameObjectWithTag("Player");
			if (searchResult != null) {
				player = searchResult.GetComponent<Player>();
			}
			nextTimeToSearch = Time.time + 0.5f;
		}
	}
}

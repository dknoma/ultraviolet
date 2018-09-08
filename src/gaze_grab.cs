using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Tobii.Gaming;

//GazeAware does NOT work on 2D objects, need to do raycasting or assigning a 3D object to the 2D object
[RequireComponent(typeof(GazeAware))]
public class Gaze_grab : MonoBehaviour {

	public Rigidbody2D box;

	private Animator boxAnimator; 
    private GazeAware _gazeAware;
	private float nextTimeToSearch = 0;
	private Player player;
	private GameObject gazeRadius;
	private float radius;
	private bool hasGaze;
	private GazePoint gazePoint;

	void Awake () {
		player = GameObject.FindGameObjectWithTag ("Player").GetComponent<Player>();
		_gazeAware = GetComponent<GazeAware>();
		boxAnimator = GetComponentInParent<Animator> ();
	}

	void OnTriggerStay(Collider coll) {
		if(coll.gameObject.tag == "GazeRadius") {
			gazeRadius = coll.gameObject;           //get the sphere collider around the player

			if (GameMaster.Biofeedback()) {
				boxAnimator.SetBool("hasGaze", _gazeAware.HasGazeFocus);    //makes it so the box's eye will open up when has gaze
				if (_gazeAware.HasGazeFocus) {
					if (player == null) {
						FindPlayer();                   //only find the player if it's null
					}

					gazePoint = TobiiAPI.GetGazePoint();

					if (Input.GetAxis(ControllerInputManager.Grab()) > 0 && player != null) {
						float radius = gazeRadius.GetComponent<SphereCollider>().radius;

						//sets the cube's pos to gaze pos; converts screen space to world space
						transform.position = Camera.main.ScreenToWorldPoint(gazePoint.Screen);  

						Vector3 centerPt = player.GetComponentInChildren<SphereCollider>().transform.position;
						
						float distance = Vector3.Distance(Camera.main.ScreenToWorldPoint(gazePoint.Screen), centerPt);

						if (distance > radius) {                                                            //if distance becomes greater than radius
							Vector3 newPos = transform.position - centerPt;                                 //create vector of difference of cube pos and center pos
							newPos *= radius / distance;                                                    //calc position in circle around center pos
							transform.position = centerPt + newPos;                                         //sets cubes pos to the max distance away from center pos
							box.velocity = (transform.position - box.transform.position) * 10;              //uses box's physics to move along with cube's movements
							transform.position = box.transform.position;                                    //this prevents the box from moving into the ground
						} else {
							box.velocity = (transform.position - box.transform.position) * 10;
							transform.position = box.transform.position;                                    //anchors 3D collider to the box so they always stay together
																											//Debug.Log ("transform: " + transform.position);	
						}
					}                                          
				}
			} else {
				
			}
		}
	}

	void OnTriggerExit(Collider coll) {
		if(coll.gameObject.tag == "GazeRadius") {
			boxAnimator.SetBool("hasGaze", false);	//makes it so the box's eye will open up when has gaze
		}
	}
	
	/*
	 * Clamp the radius of the user's gaze to the radius around the player.
	 * This is done to restrict how far a player can move and grab the
	 * movable object.
	 */ 
	public Vector3 clampRadius(GameObject grabRadius, Vector3 gazePosition) {
		float finalXPos = Mathf.Clamp(gazePosition.x, grabRadius.GetComponent<SphereCollider>().bounds.min.x /*+ halfWidth*/, 
			grabRadius.GetComponent<SphereCollider>().bounds.max.x /*- halfWidth*/);
		//clamp the camera's y position to the minimum and maximum y vaddlues of the new room
		float finalYPos = Mathf.Clamp(gazePosition.y, grabRadius.GetComponent<SphereCollider>().bounds.min.y /*+ halfHeight*/, 
			grabRadius.GetComponent<SphereCollider>().bounds.max.y /*- halfHeight*/);
		Vector3 newBounds = new Vector3(finalXPos, finalYPos, 0);
		return newBounds;
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
		if(nextTimeToSearch <= Time.time) {
			GameObject searchResult = GameObject.FindGameObjectWithTag ("Player");
			if(searchResult != null) {
				player = searchResult.GetComponent<Player>();
			} 
			nextTimeToSearch = Time.time + 0.5f;
		}
	}
}

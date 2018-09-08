using System.Collections;
using System.Collections.Generic;
using UnityEngine;

/**
 * For enemy projectiles that move in a sine wave motion. In the inspector the following can be configured:
 *	- the projectile velocity
 *	- frequency
 *	- amplitude
 *	- up or down sine wave
 *	- how long the projectiles can last for
 */ 
public class SinMovement : MonoBehaviour {

	public int projectileVelocity = 100;
	[Range(0f, 100f)]
	public float frequency = 0;
	[Range(0f, 100f)]
	public float amplitude = 0;
	public bool up;					//sets if want to move in upwards sin or downwards sin
	public bool right;

	public float timer = 0.01f;
	public float timeout = 4;

	private GameObject playerRenderer;
	private Vector3 direction;
	private float halfHeight, halfWidth;
	private Camera cam;
	private SpriteRenderer player;
	private Vector3 pos;				

	void Start() {
		if(GetComponentInParent<ParentXFlip>().right){
			direction = Vector3.right;								//sets direction of bullet to the right
		} else {
			direction = Vector3.left;								//sets direction of bullet to the left
		}
		pos = transform.position;
		cam = Camera.main;
		halfHeight = cam.orthographicSize;		//half of the height of the camera
		halfWidth = halfHeight * cam.aspect;	//half width of camera
		//		//animator = GetComponent<Animator> ();				//TODO: create animator for hit animation
	}

	void FixedUpdate () {												//we use FixedUpdate here since we're dealing with physics; acceleration = direction * time * speed
		if (!GameMaster.PlayerTransitionState ()) {
			timer += 0.02f;											//create a timer so that each projectile has it's own frame counter
			pos += direction * Time.deltaTime * projectileVelocity;
			if(up){
				transform.position = pos + Vector3.up * Mathf.Sin(timer * frequency) * amplitude;	//moves object in sin wave motion
			} else {
				transform.position = pos + Vector3.down * Mathf.Sin(timer * frequency) * amplitude;	//moves object in cos wave motion
			}
		}
		Destroy (this.gameObject, 4);
		if(this.transform.position.x >= cam.transform.position.x + halfWidth || this.transform.position.x <= cam.transform.position.x - halfWidth) {
			Destroy (this.gameObject);	//destroy the projectile if it goes off screen
		}	
	}

	void OnTriggerEnter2D(Collider2D coll) {
		if(coll.gameObject.tag == "MoveBlock") {
			Destroy(this.gameObject);
		}
		if(coll.gameObject.tag == "Ground") {
			Destroy(this.gameObject);
		}
		if(coll.gameObject.tag == "Player" && !GameMaster.PlayerIsInvulnerable()) {
			Destroy(this.gameObject);
		}
	}

    private void OnCollisionEnter2D(Collision2D coll) {
        if (coll.gameObject.tag == "MoveBlock") {
            Destroy(this.gameObject);
        }
    }
}

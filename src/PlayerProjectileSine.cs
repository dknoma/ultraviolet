using System.Collections;
using System.Collections.Generic;
using UnityEngine;

/**
 * Class for player projectiles that travel in a sine wave motion.
 * In the inspector the following can be configured: 
 *	- projectile velocity
 *	- frequency
 *	- amplitude
 *  - up or down sine wave
 *  - time till destruction
 */
public class PlayerProjectileSine : MonoBehaviour {

	public int projectileVelocity = 100;
	[Range(0f, 100f)]
	public float frequency = 0;
	[Range(0f, 100f)]
	public float amplitude = 0;
	public bool up;					//sets if want to move in upwards sin or downwards sin

	public float timer = 0.01f;
	public float timeout = 4;

	private GameObject playerRenderer;
	private Vector3 direction;
	private float halfHeight, halfWidth;
	private Camera cam;
	private SpriteRenderer player;
	private Vector3 pos;

	void Awake() {
		GameObject searchResult = GameObject.FindGameObjectWithTag ("Player");	//find player object
		if(searchResult != null) {												//needed if for player projectile
			playerRenderer = searchResult;
		} 
		player = playerRenderer.GetComponent<SpriteRenderer> ();	//gets the player spriterenderer
		if(player.flipX == true) {									//used to determine the direction the player is facing
			direction = Vector3.right;								//sets direction of bullet to the right
		} else {
			direction = Vector3.left;								//sets direction of bullet to the left
		}
		pos = transform.position;
		cam = Camera.main;
		halfHeight = cam.orthographicSize;		//half of the height of the camera
		halfWidth = halfHeight * cam.aspect;	//half width of camera
		//TODO: instantiate poof
	}

	void FixedUpdate () {															//acceleration = direction * time * speed
		if (!GameMaster.PlayerTransitionState ()) {
			timer += 0.02f;											//create a timer so that each projectile has it's own frame counter
			pos += direction * Time.deltaTime * projectileVelocity;
			if(up){
				transform.position = pos + Vector3.up * Mathf.Sin(timer * frequency) * amplitude;	//moves object in sin wave motion
			} else {
				transform.position = pos + Vector3.down * Mathf.Sin(timer * frequency) * amplitude;	//moves object in cos wave motion
			}
		}
		Destroy (this.gameObject, 3);
		if(this.transform.position.x >= cam.transform.position.x + halfWidth || this.transform.position.x <= cam.transform.position.x - halfWidth) {
			Destroy (this.gameObject);	//destroy the projectile if it goes off screen
		}	
	}

	void OnTriggerEnter2D(Collider2D coll) {
		if (coll.gameObject.tag == "BombTile") {
			Destroy(this.gameObject);
		}
		if (coll.gameObject.tag == "EnemySm" || coll.gameObject.tag == "Enemy") {
			Destroy(this.gameObject);
		}
	}
}

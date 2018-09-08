using System.Collections;
using System.Collections.Generic;
using UnityEngine;

/**
 * Normal player projectile taht shoots in a straight line
 */ 
public class PlayerProjectilePhysics : MonoBehaviour {

	public int projectileVelocity = 100;
	public bool isSwordBeam = false;
	private GameObject playerRenderer;

	private Vector3 direction;
	private float halfHeight, halfWidth;
	private Camera cam;

	void Awake() {
		GameObject searchResult = GameObject.FindGameObjectWithTag ("Player");	//find player object
		if(searchResult != null) {
			playerRenderer = searchResult;
		} 
		SpriteRenderer player = playerRenderer.GetComponent<SpriteRenderer> ();	//gets the player spriterenderer
		if(player.flipX == true) {							//used to determine the direction the player is facing
			direction = Vector3.right;                      //sets direction of bullet to the right
		} else {
			direction = Vector3.left;                       //sets direction of bullet to the left
			GetComponent<SpriteRenderer>().flipX = true;
		}
		cam = Camera.main;
		halfHeight = cam.orthographicSize;		//half of the height of the camera
		halfWidth = halfHeight * cam.aspect;
		//TODO: instantiate bullet poof object
	}

	void Update () {															//acceleration = direction * time * speed
		if (!GameMaster.PlayerTransitionState ()) {
			transform.Translate (direction * Time.deltaTime * projectileVelocity);	//moves objects over time in direction
		}
		Destroy (this.gameObject, 2);
		if(this.transform.position.x >= cam.transform.position.x + halfWidth || this.transform.position.x <= cam.transform.position.x - halfWidth) {
			Destroy (this.gameObject);	//destroy the projectile if it goes off screen
		}	
	}
	
	void OnTriggerEnter2D(Collider2D coll) {
		if(coll.gameObject.tag == "Ground" || coll.gameObject.tag == "Gate" || coll.gameObject.tag == "MoveBlock" 
			|| coll.gameObject.tag == "BombTile") {
			if (!isSwordBeam) {
				Destroy(this.gameObject);
			}
		}
		if(coll.gameObject.tag == "EnemySm" || coll.gameObject.tag == "Enemy") {
			if (!isSwordBeam) {
				Destroy(this.gameObject);
			}
		}
	}
}

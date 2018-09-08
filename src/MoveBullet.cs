using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PlayerProjectile : MonoBehaviour {

	public int projectileVelocity = 100;
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
			direction = Vector3.right;						//sets direction of bullet to the right
		} else {
			direction = Vector3.left;						//sets direction of bullet to the left
		}
		cam = Camera.main;
		halfHeight = cam.orthographicSize;		//half of the height of the camera
		halfWidth = halfHeight * cam.aspect;
//		Debug.Log ("TODO: create animator for hit animation");
//		//animator = GetComponent<Animator> ();				//TODO: create animator for hit animation
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
		if(coll.gameObject.tag == "MoveBlock") {
			Destroy(this.gameObject);
		}
		if(coll.gameObject.tag == "Ground") {
			Destroy(this.gameObject);
		}
		if(coll.gameObject.tag == "EnemySm" || coll.gameObject.tag == "Enemy") {
			//			Debug.Log ("TODO: create hit animation");
			//Enemy_walk enemy = coll.gameObject.GetComponent<Enemy_walk>();
			//if(enemy != null) {
			//	enemy.damageEnemy (10);
			//}
			Destroy(this.gameObject);
		}
	}
//
//	void OnTriggerStay2D(Collider2D coll) {
//		if(coll.gameObject.tag == "Ground") {
//			Debug.Log ("In ground");
//			Destroy(this.gameObject);
//		}
//	}
}

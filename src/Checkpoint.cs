using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Checkpoint : MonoBehaviour {

	public Transform spawnPoint;

	private bool activated;
	private Animator anim;

	void Awake () {
		anim = GetComponent<Animator> ();
		activated = false;
	}

	void Update() {
		if (!GameMaster.PlayerRespawnState ()) {
			anim.SetBool ("playerTransition", GameMaster.PlayerTransitionState ());	//animation disappears if transitioning, reappears when not transitioning
		}
	}

	void OnTriggerEnter2D(Collider2D coll) {
		if(coll.gameObject.tag == "Player") {
			GameMaster.PlayCheckpoint ();
			activated = true;
			spawnPoint.position = this.transform.position - new Vector3(0,8,0);	//moves the player's spawn position to the checkpoints location
			Destroy (GetComponent<BoxCollider2D> ());		//remove the collider so player cant activate checkpoint again
			anim.SetBool ("activated", activated);
		}
	}
}

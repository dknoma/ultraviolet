using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class LadderClimb : MonoBehaviour {

	private GameObject player;
	private BoxCollider2D coll;
	private Vector2 bottomLeftBound;
	private Vector2 topRightBound;

	private void Awake() {
		coll = GetComponent<BoxCollider2D>();
		bottomLeftBound = new Vector2((Mathf.Round((coll.transform.position.x - (coll.bounds.size.x / 2)) * 2) / 2) - coll.offset.x ,
				(Mathf.Round((coll.transform.position.y - (coll.bounds.size.y / 2)) * 2) / 2) - coll.offset.y);             //Get the bottom left
		topRightBound = new Vector2((Mathf.Round((coll.transform.position.x + (coll.bounds.size.x / 2)) * 2) / 2) - coll.offset.x,
			(Mathf.Round((coll.transform.position.y + (coll.bounds.size.y / 2)) * 2) / 2) - coll.offset.y);					//Get the top right
	}

	private void OnTriggerStay2D(Collider2D coll) {
		if (coll.gameObject.tag == "Player") {
			if (player == null) {
				player = coll.gameObject;
			}
			GameMaster.LadderCheck(true);
		}
	}

	private void OnTriggerExit2D(Collider2D coll) {
		if (coll.gameObject.tag == "Player") {
			if ((player.transform.position.x <= bottomLeftBound.x || player.transform.position.x >= topRightBound.x)
				|| (player.transform.position.y >= topRightBound.y || player.transform.position.y <= bottomLeftBound.y)) {
				GameMaster.LadderCheck(false);
			}
		}
	}
}

using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CollisionDetection : MonoBehaviour {

	private Camera cam;
	private GameObject currentRoom;
	private float halfHeight, halfWidth;

	void Update() {
		if(currentRoom != null) {
			transform.position = ClampPosition(currentRoom, this.transform.position);  
			//clamp	the	position of	this object	so it doesnt go	into other rooms
		}
	}

	public Vector3 ClampPosition(GameObject currentRoom, Vector3 boxPosition) {
		halfWidth = currentRoom.GetComponent<BoxCollider2D>().size.x/2;
		//print("halfwidth: " + halfWidth);
		float finalXPos = Mathf.Clamp(boxPosition.x, currentRoom.transform.position.x - halfWidth + 24,
			currentRoom.transform.position.x + halfWidth - 24);

		Vector3 newBounds = new Vector3(finalXPos, this.transform.position.y, -1);
		return newBounds;
	}

	void OnTriggerEnter2D(Collider2D coll) {
		if (coll.gameObject.tag == "Room") {
			currentRoom = coll.gameObject;
		}
	}

	void OnTriggerExit2D(Collider2D coll) {
		if (coll.gameObject.tag == "Room") {
			Destroy(this.gameObject);
		}
	}
}

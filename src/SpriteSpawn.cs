using System.Collections;
using System.Collections.Generic;
using UnityEngine;

/*
 * A spawner that works	for	any	sprite.	This make it so	sprites	act	like they did in NES games.
 * When	the	player leaves the room,	all	sprites	will disappear.	When the player	comes back into
 * the room	the	sprites	respawn	in their original locations	after the player is	done transitioning.
 */	
public class SpriteSpawn : MonoBehaviour {

	public bool instant = false;
	public GameObject sprite;

	private	GameObject spritesCurrentRoom;
	private	Player player;
	private	float nextTimeToSearch = 0;
	private	Transform parent;
	private	bool spriteExists;
	private bool move;

	void Start() {
		Instantiate(sprite, transform);
		spriteExists = true;
		move = false;
		FindPlayer();
	}

	void Update	() {
		FindPlayer();
		Spawning();
	}

	void OnTriggerEnter2D(Collider2D coll) {
		if(coll.gameObject.tag == "Room") {
			spritesCurrentRoom = coll.gameObject;

			if (player.currentRoom != spritesCurrentRoom) {
				foreach (Transform child in transform) {
					if (child.GetComponent<SpriteShooter>() != null) {
						child.GetComponent<SpriteShooter>().CancelInvoke();
					}
					Destroy(child.gameObject);
				}
				spriteExists = false;
			}
		}
	}

	private void Spawning() {
		if(!instant) {
			// makes it so sprites don't spawn until the player is finished transitioning
			if(GameMaster.PlayerTransitionState() && !GameMaster.PlayerRespawnState()
			   && player.newRoom != spritesCurrentRoom) {
				foreach(Transform child in	transform) {							//and leaves the room destroy the sprite
					if(child.GetComponent<SpriteShooter>() != null) {
						child.GetComponent<SpriteShooter>().CancelInvoke();			//if spawning a shooter, cancel shooting
					}			 
					Destroy	(child.gameObject);
				}
				spriteExists = false;
			} else if(player.currentRoom == spritesCurrentRoom) {					//if the player	comes back into	the	room and 
				if(!spriteExists) {
					Instantiate(sprite, this.transform);				//the sprite doesnt	exist, reinstantiates
					spriteExists = true;
				}
			}
		} else {
			// makes it so sprites spawn right away regardless of player state
			if(player.newRoom != spritesCurrentRoom) {
				if (spriteExists) {
					foreach (Transform child in	transform) {						//and leaves the room destroy the sprite
						if (child.GetComponent<SpriteShooter> () != null) {
							child.GetComponent<SpriteShooter> ().CancelInvoke ();	//if spawning a shooter, cancel shooting
						}			 
						Destroy	(child.gameObject);
					}
					spriteExists = false;
				}

			} else if(player.newRoom == spritesCurrentRoom) {
				if(GameMaster.PlayerTransitionState() && !spriteExists) {
					Instantiate(sprite, this.transform);				//the sprite doesnt	exist, reinstantiates
					spriteExists = true;
				}
			}
		}
	}

	/*
	 * Finds the player	object.	Used when the player spawns	as new instances of	the	player
	 * will	technically	be a clone and the camera will disjoint	after the player disappears.
	 * 
	 * Makes sure that the target of the script	will never be null.	Uses nextTimeToSearch
	 * in order	to make	sure that the method is	not	done every frame as	it would be	very
	 * taxing on the cpu.
	 */	
	void FindPlayer() {
		if(nextTimeToSearch	<= Time.time) {
			GameObject searchResult	= GameObject.FindGameObjectWithTag("Player");
			if(searchResult	!= null) {
				player = searchResult.GetComponent<Player>();
			} 
			nextTimeToSearch = Time.time + 0.5f;
		}
	}
}

using System.Collections;
using System.Collections.Generic;
using UnityEngine;

/**
 * Class that sets off a sequence of explosions. In order to do this, the script must be placed on a parent
 * object, as the children will be destroyed in sequence of their order. The amount of tiles to explode per
 * explosion, delay of the explosions can all be set in the inspector.
 */ 
public class ExplodeTiles : MonoBehaviour {

	public GameObject tileExplosion;
	// height/width/etc of tiles to destroy at the same time
	public int explosionLength;
	// delay for sequential explosions, explosionLength = 1 && delay ~= 0.5f will explode like in kirby
	public float delay;

	// boolean val to check if exploding
	private bool exploding = false;
	private bool once = true;
	private int children;

	private void Start() {
		children = transform.childCount;
	}

	private void Update() {
		if(exploding) {
			exploding = false;
			StartCoroutine(delayedDestroy());
		}
	}

	private void OnTriggerEnter2D(Collider2D coll) {
		if((coll.gameObject.tag == "PlayerProjectile" || coll.gameObject.tag == "sword"
			|| coll.gameObject.tag == "upgraded_projectile") && once) {
			exploding = true;
			once = false;
			print("exploding: " + exploding);
			Destroy(this.gameObject.GetComponent<BoxCollider2D>());
			Destroy(this.gameObject.GetComponent<CompositeCollider2D>());
		}
	}

	private IEnumerator delayedDestroy() {
		for (int i = 0; i < children; i += explosionLength) {
			for (int j = i; j < i + explosionLength; j++) {
				Transform child = transform.GetChild(j);
				GameMaster.TileExplosionSFX();
				GameObject te = Instantiate(tileExplosion, child.transform);
				Destroy(child.GetComponent<BoxCollider2D>());
				Destroy(child.GetComponent<SpriteRenderer>());
			}
			yield return new WaitForSeconds(delay);
		}
		if (explosionLength != 0) {
			Destroy(this.gameObject, Mathf.Ceil(children / (explosionLength * 4))); //destroy this object and all children
		} else {
			Destroy(this.gameObject,2);
		}
	}
}

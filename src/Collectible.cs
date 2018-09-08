using System.Collections;
using System.Collections.Generic;
using UnityEngine;

/**
 * Class for any objects that will give the player currency, ammo, etc
 */
public class Collectible : MonoBehaviour {

	public int amount = 5;
	public bool isNrg = false;
	public GameObject shine;

	private void OnTriggerEnter2D(Collider2D coll) {
		if (coll.gameObject.tag == "Player") {
			Destroy(GetComponent<Rigidbody2D>());
			Destroy(GetComponent<Collider2D>());
			Destroy(GetComponent<SpriteRenderer>());
			try {
				foreach(Transform child in transform) {
					Destroy(child.gameObject);
				}
			} catch {
				print("No extra children FX to destroy");
			}

			if (amount < 100) {
				GameMaster.CoinSFX();
				int count = Random.Range(2, 4);
				StartCoroutine(SpawnShine(count));
			} else {
				GameMaster.GemSFX();
				int count = Random.Range(3, 6);
				StartCoroutine(SpawnShine(count));
			}
			if (!isNrg) {
				GameMaster.incGems(amount, GameMaster.gm);
			} else {
				GameMaster.incAmmo(amount, GameMaster.gm);
			}
		}
	}

	/**
	 * Spawns a given number of shine FX at random positions relative to where the item was collected
	 */ 
	private IEnumerator SpawnShine(int count) {
		float randX, randY;
		float spawnX, spawnY;
		for(int i = 0; i < count; i++) {
			randX = Random.Range(-8, 8);
			randY = Random.Range(-8, 8);
			spawnX = Random.Range(transform.position.x - randX, transform.position.x + randX);
			spawnY = Random.Range(transform.position.y - randY, transform.position.y + randY);
			Instantiate(shine, new Vector2(spawnX, spawnY), Quaternion.identity);
			yield return new WaitForSeconds(0.1f);
		}
		yield return new WaitForSeconds(0.5f);
		Destroy(gameObject);
	}
}

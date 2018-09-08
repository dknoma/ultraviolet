using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Sword : MonoBehaviour {
	
	void OnTriggerEnter2D(Collider2D coll) {
		if(coll.gameObject.tag == "EnemySm" || coll.gameObject.tag == "Enemy") {
			//Enemy_walk enemy = coll.gameObject.GetComponent<Enemy_walk>();
			//if(enemy != null) {
			//	enemy.damageEnemy (20);
			//}
			Destroy(this.gameObject);
		}
	}
}

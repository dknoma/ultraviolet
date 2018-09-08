using System.Collections;
using System.Collections.Generic;
using UnityEngine;

/**
 * Class meant for shooters that can spawn more than 1 projectile
 */ 
public class SpriteShooter : MonoBehaviour {

	public GameObject spriteOne;
	public GameObject spriteTwo;
	[Range(1f,25f)]
	public int NumberOfBullets = 3;
	[Range(0.1f,5f)]
	public float delay = 0.1f;

	private	Transform firePoint1;
	private	Transform firePoint2;

	void Awake () {
		firePoint1 = transform.GetChild(0);
		firePoint2 = transform.GetChild(1);
		if(!GetComponent<SpriteRenderer>().flipX) {
			firePoint1.position	*= 1;						//Set spawn	point of bullet	to right
			firePoint2.position	*= 1;
		} else {
			firePoint1.position	= new Vector2(firePoint1.position.x	* -1, firePoint1.position.y);
			firePoint2.position	= new Vector2(firePoint2.position.x	* -1, firePoint2.position.y);
		}	
		InvokeRepeating("Shoot", 1,	5);
	}
	
	private	void Shoot() {
		GameMaster.PlayShooterSFX ();
		for	(int i = 0;	i <	NumberOfBullets; i++) {
			Invoke ("SpawnBullet", delay);
			delay += 0.3f;
		}
		delay =	0.1f;
		return;
	}

	private	void SpawnBullet() {
		GameObject proj1 = Instantiate (spriteOne, firePoint1.position,	firePoint1.rotation);
		proj1.transform.SetParent (firePoint1.transform);
		GameObject proj2 = Instantiate (spriteTwo, firePoint2.position,	firePoint2.rotation);
		proj2.transform.SetParent (firePoint2.transform);
		return;
	}
}

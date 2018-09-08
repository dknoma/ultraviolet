using System.Collections;
using System.Collections.Generic;
using UnityEngine;

/**
 * Contains all methods related to the player's weapons. These methods can change the player's current weapon,
 * spawn their corresponding game objects and sfx
 */ 
public class Weapon : MonoBehaviour {

	public float fireRate = 2.5f;			//shots per second, can be changed in editor; lower firerate = slower
	public LayerMask whatToHit;				//what want to hit
	public GameObject playerPrefab;
	public GameObject projectilePrefab;
	public GameObject upgradedProjectileUp;
	public GameObject upgradedProjectileDown;
	public GameObject upgradedSwordBeam;
	//public GameObject upgradedSwordProjectile;
	public GameObject swordHitbox;
	public GameObject swordHitboxAlt;
	public float bulletSpawnRate = 10;

	private float swordAttackRate = 1;
	private float timeToSpawnBullet = 0;
	private float timeToFire = 0;
	private float upgradeFireRate = 1f;
	private float timeToFireUpgrade = 0;
	private Transform firePoint;
	private Transform swordPoint;
	private GameObject sword;
	private int[] weaponState;
	private bool buffer;
	private bool cancelBuffer;
	private float delay = 0f;

	void Awake () {
		firePoint = transform.Find ("FirePoint");			//finds specific object
		swordPoint = transform.Find("SwordPoint");
		if(firePoint == null) {
			Debug.LogError ("Something went wrong. Firepoint not found.");
		} else {
			Debug.Log ("Firepoint found");
		}

		/* int[] of what weapons in which place
		 *	0 = sword
		 *	1 = shooter
		 *	etc
		 */
		weaponState = new int[2];
	}

	void Update() {
		ChooseWeapon();
		UseWeapon();
	}

	/**
	 * Method that checks inputs to see which weapon to use
	 */ 
	private void UseWeapon() {
		switch(GameMaster.Weapon()){
			case "sword":
				if (upgradeFireRate == 0) {
					if (!GameMaster.UpgradedSword()) {
						if (Input.GetButton(ControllerInputManager.Fire()) && !GameMaster.PlayerTransitionState()
							&& !GameMaster.IsPaused() && !GameMaster.Swinging()) {
							Sword();
						}
					} else {
						if (Input.GetAxisRaw(ControllerInputManager.Vertical()) > 0 && !GameMaster.Swinging()) {
							if (Input.GetButton(ControllerInputManager.Fire()) && Time.time > upgradeFireRate
							&& !GameMaster.PlayerTransitionState() && !GameMaster.IsPaused()) {
								ShootSwordBeam();
							}
						} else {
							if (Input.GetButton(ControllerInputManager.Fire()) && !GameMaster.PlayerTransitionState()
							&& !GameMaster.IsPaused() && !GameMaster.Swinging()) {
								Sword();
							}
						}
					}
				} else {
					if (!GameMaster.UpgradedSword()) {
						if (Input.GetButton(ControllerInputManager.Fire()) && !GameMaster.PlayerTransitionState()
							&& !GameMaster.IsPaused() && !GameMaster.Swinging()) {
							Sword();
						}
					} else {
						if (Input.GetAxisRaw(ControllerInputManager.Vertical()) > 0 && !GameMaster.Swinging()) {
							if (Input.GetButton(ControllerInputManager.Fire()) && Time.time > timeToFireUpgrade
							&& !GameMaster.PlayerTransitionState() && !GameMaster.IsPaused()) {
								timeToFireUpgrade = Time.time + 1 / upgradeFireRate;
								//shoot sword beam
								ShootSwordBeam();
							}
						} else {
							if (Input.GetButton(ControllerInputManager.Fire()) && !GameMaster.PlayerTransitionState()
							&& !GameMaster.IsPaused() && !GameMaster.Swinging()) {
								Sword();
							}
						}
					}
				}
				break;

			case "blaster":
				if (fireRate == 0 && upgradeFireRate == 0) {
					if (GameMaster.UpgradedBlaster()) {
						if (Input.GetAxisRaw(ControllerInputManager.Vertical()) > 0
						&& !GameMaster.PlayerTransitionState() && !GameMaster.IsPaused()) {

							if (Input.GetButtonDown(ControllerInputManager.Fire()) && Time.time > upgradeFireRate
								&& !GameMaster.PlayerTransitionState() && !GameMaster.IsPaused()) {
								if (GameMaster.Ammo() > 5) {
									ShootUpgrade();
								} else {
									timeToFireUpgrade = Time.time + 1 / upgradeFireRate;
									GameMaster.CancelSFX();
								}
							}
						} else if (Input.GetButton(ControllerInputManager.Fire()) && Time.time > timeToFire
						 && !GameMaster.PlayerTransitionState() && !GameMaster.IsPaused()) {
							if (GameMaster.Ammo() > 0) {
								Shoot();
							} else {
								timeToFire = Time.time + 1 / fireRate;
								GameMaster.CancelSFX();
							}
						}
					}
				} else {

					if (GameMaster.UpgradedBlaster()) {
						if (Input.GetAxisRaw(ControllerInputManager.Vertical()) > 0
							&& !GameMaster.PlayerTransitionState() && !GameMaster.IsPaused()) {

							if (Input.GetButtonDown(ControllerInputManager.Fire()) && Time.time > timeToFireUpgrade
								&& !GameMaster.PlayerTransitionState() && !GameMaster.IsPaused()) {
								if (GameMaster.Ammo() > 5) {
									timeToFireUpgrade = Time.time + 1 / upgradeFireRate;
									ShootUpgrade();
								} else {
									timeToFireUpgrade = Time.time + 1 / upgradeFireRate;
									GameMaster.CancelSFX();
								}
							}
						} else if (Input.GetButton(ControllerInputManager.Fire()) && Time.time > timeToFire
							&& !GameMaster.PlayerTransitionState() && !GameMaster.IsPaused()) {
							if (GameMaster.Ammo() > 0) {
								timeToFire = Time.time + 1 / fireRate;
								Shoot();
							} else {
								timeToFire = Time.time + 1 / fireRate;
								GameMaster.CancelSFX();
							}
						}
					} else {
						if (Input.GetButton(ControllerInputManager.Fire()) && Time.time > timeToFire
						&& !GameMaster.PlayerTransitionState() && !GameMaster.IsPaused()) {
							if (GameMaster.Ammo() > 0) {
								timeToFire = Time.time + 1 / fireRate;
								Shoot();
							} else {
								timeToFire = Time.time + 1 / fireRate;
								GameMaster.CancelSFX();
							}
						}
					}

				}
				break;
			default:
				GameMaster.SetWeapon("sword");
				break;
		}
	}

	/**
	 * Method to shoot regular bullets
	 * Plays SFX, decreases ammo, chooses which direction the bullets will spawn, and the time to shoot the bullets
	 */
	public void Shoot() {
		GameMaster.PlayShoot ();
		GameMaster.decAmmo(1, GameMaster.gm);
		if(playerPrefab.GetComponent<SpriteRenderer> ().flipX == true) {		//Check player direction
			firePoint.localPosition = new Vector2 (4, 0);						//Set spawn point of bullet to right
		} else {
			firePoint.localPosition = new Vector2 (-4, 0);						//Set spawn point of bullet to left
		}

		if(Time.time >= timeToSpawnBullet) {
			SpawnBullet ();
			timeToSpawnBullet = Time.time + 1/bulletSpawnRate;
		}
	}

	/****************************************************************************************
	 *																						*
	 *																						*
	 *									Weapon_spawning										*
	 *																						*
	 *																						*
	 ****************************************************************************************/

	/**
	 * Method to shoot upgraded bullets
	 * Plays SFX, decreases ammo, chooses which direction the bullets will spawn, and the time to shoot the bullets
	 */
	public void ShootUpgrade() {
		GameMaster.UShotSFX();
		GameMaster.decAmmo(6, GameMaster.gm);
		if (playerPrefab.GetComponent<SpriteRenderer>().flipX == true) {        //Check player direction
			firePoint.localPosition = new Vector2(4, 0);                        //Set spawn point of bullet to right
		} else {
			firePoint.localPosition = new Vector2(-4, 0);                       //Set spawn point of bullet to left
		}

		if (Time.time >= timeToSpawnBullet) {
			Vector2 pos = firePoint.position;
			//for (int i = 0; i < 2; i++) { //if want more than 2 bullets spawned, uncomment
			StartCoroutine(SpawnUpgradedBullets(pos, delay));
			//delay	+= 0.2f;
			//}
			//delay = 0;
			timeToSpawnBullet = Time.time + 1 / bulletSpawnRate;
		}
	}

	/**
	 * Method to shoot upgraded bullets
	 * Plays SFX, decreases ammo, chooses which direction the bullets will spawn, and the time to shoot the bullets
	 */
	public void ShootSwordBeam() {
		GameMaster.SwordBeamSFX();
		GetComponentInParent<Animator>().SetBool("upgradedSwing", true);
		GameMaster.CheckSwingingSword(true);
		if (playerPrefab.GetComponent<SpriteRenderer>().flipX == true) {        //Check player direction
			swordPoint.localPosition = new Vector2(17, 8);                        //Set spawn point of bullet to right
		} else {
			swordPoint.localPosition = new Vector2(-17, 8);                       //Set spawn point of bullet to left
		}

		if (Time.time >= timeToSpawnBullet) {
			StartCoroutine(SpawnSwordBeam());
			timeToSpawnBullet = Time.time + 1 / bulletSpawnRate;
		}
	}

	/*
	 * Spawns the swords hitbox
	 * Animation will stop by setting false through Event in animation clip
	 */
	public void Sword() {
		GameMaster.SwordSFX ();
		GetComponentInParent<Animator> ().SetBool ("swordAttack", true);
		GameMaster.CheckSwingingSword (true);
		if(playerPrefab.GetComponent<SpriteRenderer> ().flipX == true) {		//Check player direction
			swordPoint.localPosition = new Vector2 (16, 0.5f);						//Set spawn point of bullet to right
		} else {
			swordPoint.localPosition = new Vector2 (-16, 0.5f);						//Set spawn point of bullet to left
		}

		if(Time.time >= timeToSpawnBullet) {
			StartCoroutine(SpawnSword ());
			timeToSpawnBullet = Time.time + 1/bulletSpawnRate;
		}
	}

	/****************************************************************************************
	 *																						*
	 *																						*
	 *							Weapon_instantiation								*
	 *																						*
	 *																						*
	 ****************************************************************************************/
	/**
	 * Spawns the player projectile
	 */
	private void SpawnBullet() {
		Instantiate (projectilePrefab, firePoint.position, firePoint.rotation);
	}

	/**
	 * Spawns the upgraded bullets
	 */ 
	private IEnumerator SpawnUpgradedBullets(Vector2 firePointPos, float delay) {
		yield return new WaitForSeconds(delay);
		Instantiate(upgradedProjectileDown, firePointPos, firePoint.rotation);
		yield return new WaitForSeconds(0.025f);
		Instantiate(upgradedProjectileUp, firePointPos, firePoint.rotation);
	}

	/**
	 * Spawns the sword after a tiny delay
	 */
	private IEnumerator SpawnSword() {
		yield return new WaitForSeconds (0.08f);
		sword = Instantiate (swordHitbox, swordPoint.position, swordPoint.rotation, swordPoint);
		Destroy (sword, 0.22f);
	}

	/**
	 * Spawns the sword after a tiny delay
	 */
	private IEnumerator SpawnSwordBeam() {
		yield return new WaitForSeconds(0.1f);
		Instantiate(upgradedSwordBeam, swordPoint.position, swordPoint.rotation);
		yield return new WaitForSeconds(0.01f);
		sword = Instantiate(swordHitboxAlt, swordPoint.position, swordPoint.rotation, swordPoint);
		Destroy(sword, 0.15f);
	}

	/****************************************************************************************
	 *																						*
	 *																						*
	 *									Weapong_changing									*
	 *																						*
	 *																						*
	 ****************************************************************************************/
	/* 
	 * Changes the current weapon based on what buttons are pressed
	 */
	private void ChooseWeapon() {
		if (!GameMaster.PlayerTransitionState()) {
			if (Input.GetButtonDown("PS4_R1")) {
				GameMaster.SelectSFX();
				GameMaster.ChangeWeaponIndex((GameMaster.WeaponIndex() + 1) % weaponState.Length);
				print("weapon index = " + GameMaster.WeaponIndex());
				if (GameMaster.WeaponIndex() == 0) {
					print("Current weapon: sword.");
					ChooseSword();
				} else if (GameMaster.WeaponIndex() == 1) {
					print("Current weapon: blaster.");
					ChooseShooter();
				}
			} else if (Input.GetButtonDown("PS4_L1")) {
				GameMaster.SelectSFX();
				GameMaster.ChangeWeaponIndex((GameMaster.WeaponIndex() + weaponState.Length - 1) % weaponState.Length);
				print("weapon index = " + GameMaster.WeaponIndex());
				if (GameMaster.WeaponIndex() == 0) {
					print("Current weapon: sword.");
					ChooseSword();
				} else if (GameMaster.WeaponIndex() == 1) {
					print("Current weapon: blaster.");
					ChooseShooter();
				}
			}
		}
	}

	/*			Changing weapons methods		 */
	private void ChooseSword() {
		GameMaster.CheckSword (true);
		GameMaster.CheckShooter (false);
	}

	private void ChooseShooter() {
		GameMaster.CheckSword (false);
		GameMaster.CheckShooter (true);
	}

}

using System.Collections;
using System.Collections.Generic;
using UnityEngine;

/**
 * This class is meant for any kind of enemy that just walks along the ground. It can also be specified
 * whether or not it can walk, if it will fall off edges or stay on the platform, what kind of loot it
 * will drop on death.
 *		Call this class as a parent class if you need extra functionaility ie. separate attacks
 */ 
public class Enemy_walk : EnemyStats {

	public float maxSpeed = 80;
	public float gravityModifier = 50;
	public LayerMask mask;
	public bool canFall = true;
	public bool canWalk = true;
	public bool smallLoot = true;
	public GameObject collectibles;
	public GameObject poof;

	/* physics */
	private Vector2 move;
	private Vector2 groundNormal;
	private Vector2 velocity;
	private Vector2 knockedVelocity;
	private Vector2 targetVelocity;
	private float minGroundNormalY = .65f;
	private int fallBoundary = 1;
	private bool grounded;
	private bool isFalling;
	private bool invulnerable;
	private bool enemyKnockedBack;
	private bool inGround;

	private RaycastHit2D[] hitBuffer = new RaycastHit2D[16];
	private List<RaycastHit2D> hitBufferList = new List<RaycastHit2D>(16);
	private const float minMoveDistance = 0;
	private const float shellRadius = 0.01f;

	/* Enemy components and rendering */
	private Rigidbody2D rb2d;
	private SpriteRenderer spriteRenderer;
	private Animator anim;
	private ContactFilter2D contactFilter;
	private float moving;
	private bool tryFlip;
	private Transform platformCheck;
	private RaycastHit2D[] platformHitBuffer = new RaycastHit2D[16];
	private GameObject currentRoom;
	private Vector2 bottomLeftBound;
	private Vector2 topRightBound;



	/****************************************************************************************
	 *																						*
	 *																						*
	 *								Built_in_Unity_Methods									*
	 *																						*
	 *																						*
	 ****************************************************************************************/

	private void Awake() {
		currentHealth = maxHealth;
		//ignore colliders that aren't triggers between player and enemy
		if (canWalk) {
			Physics2D.IgnoreCollision(GameObject.FindGameObjectWithTag("Player").GetComponent<BoxCollider2D>(), GetComponent<Collider2D>());
		}
		rb2d = GetComponent<Rigidbody2D>();

		if (GetComponent<Animator>() != null) {
			anim = GetComponent<Animator>();
		}

		if (transform.Find("platform_check") != null) {
			platformCheck = transform.Find("platform_check").transform;
		}
		spriteRenderer = gameObject.GetComponent<SpriteRenderer>();
		contactFilter.useTriggers = false;
		contactFilter.SetLayerMask(mask);
		contactFilter.useLayerMask = true;
		moving = -1;
		tryFlip = false;
		grounded = true;
		invulnerable = false;
	}

	void Update() {
		if (canWalk) {
			targetVelocity = Vector2.zero;
			ComputeVelocity();
		}

		if(currentRoom != null) {
			transform.position = ClampPosition(currentRoom, transform.position);
		}
	}

	void FixedUpdate() {

		if (canWalk) {
			velocity += gravityModifier * Physics2D.gravity * Time.deltaTime;
			// go on to normal physics calculations
			if (enemyKnockedBack) {
				velocity.x = knockedVelocity.x;
			} else {
				velocity.x = targetVelocity.x;
			}
			
			grounded = false;                                                                   //grounded is initially false
			Vector2 deltaPosition = velocity * Time.deltaTime;

			Vector2 moveAlongGround = new Vector2(groundNormal.y, -groundNormal.x);

			Vector2 move = moveAlongGround * deltaPosition.x;

			Movement(move, false);

			move = Vector2.up * deltaPosition.y;

			Movement(move, true);
		}
	}

	private void OnTriggerEnter2D(Collider2D coll) {
		if(coll.gameObject.tag == "sword" && !invulnerable) {
			damageEnemy(20, coll.gameObject);
		}
		if(coll.gameObject.tag == "PlayerProjectile" && !invulnerable) {
			damageEnemy(10, coll.gameObject);
		}

		if(coll.gameObject.tag == "upgraded_projectile" && !invulnerable) {
			damageEnemy(30, coll.gameObject);
		}

		if (coll.gameObject.tag == "sword_beam" && !invulnerable) {
			damageEnemy(40, coll.gameObject);
		}

		if(coll.gameObject.tag == "Room") {
			currentRoom = coll.gameObject;
			bottomLeftBound = new Vector2((Mathf.Round((coll.transform.position.x - (coll.bounds.size.x / 2)) * 2) / 2) - coll.offset.x,
				(Mathf.Round((coll.transform.position.y - (coll.bounds.size.y / 2)) * 2) / 2) - coll.offset.y);             //Get the bottom left
			topRightBound = new Vector2((Mathf.Round((coll.transform.position.x + (coll.bounds.size.x / 2)) * 2) / 2) - coll.offset.x,
				(Mathf.Round((coll.transform.position.y + (coll.bounds.size.y / 2)) * 2) / 2) - coll.offset.y);
		}
	}

	/****************************************************************************************
	 *																						*
	 *																						*
	 *									Enemy_physics										*
	 *																						*
	 *																						*
	 ****************************************************************************************/

	/**
	 * Computes the enemy's velocity
	 */
	private void ComputeVelocity() {
		move = Vector2.zero;

		move.x = moving;
		tryFlip = true;

		if (canFall) {
			if (velocity.y > 0) {                           //reduce y speed and set animation state to walking if applicable
				velocity.y = velocity.y * 0.5f;
			}
		} else {
			PlatformCheck();
		}
		if (move.x > 0.01f) {                               //if has x speed, flip sprite direction when moving to move the correct way
			if (spriteRenderer.flipX == false) {
				spriteRenderer.flipX = true;
			}
		} else if (move.x < -0.01f) {
			if (spriteRenderer.flipX == true) {
				spriteRenderer.flipX = false;
			}
		}
		targetVelocity = move * maxSpeed;
	}

	/*
	 * Raycasting to check the edge of the platform
	 */ 
	private void PlatformCheck() {
		//raycasts to check if a platform is present underneath the sprite
		Physics2D.Raycast(platformCheck.position, Vector2.down, contactFilter, platformHitBuffer, Mathf.Infinity);
		//print("check = " + platformHitBuffer[0].distance);
		if ((platformHitBuffer[0].distance > 1 || platformHitBuffer[0].distance == Mathf.Infinity) && tryFlip) {
			Flip();
		}
	}

	/*
	 * Calculates overall movement of the enemy.
	 */
	private void Movement(Vector2 move, bool yMovement) {
		float distance = move.magnitude;

		if (distance > minMoveDistance) {
			int count = rb2d.Cast(move, contactFilter, hitBuffer, distance + shellRadius);
			hitBufferList.Clear();
			for (int i = 0; i < count; i++) {
				//makes the raycast ignore objects with these tags to make sure the bodies do not collide
				if (hitBuffer[i].collider.tag != "Player" && hitBuffer[i].collider.tag != "collectible_ground"
					&& hitBuffer[i].collider.tag != "Enemy" && hitBuffer[i].collider.tag != "Enemy_Sm") {
					if(hitBuffer[i].collider.tag == "MoveBlock") {
					}
					hitBufferList.Add(hitBuffer[i]);
				}
			}

			for (int i = 0; i < hitBufferList.Count; i++) {
				Vector2 currentNormal = hitBufferList[i].normal;
				if (currentNormal.y > minGroundNormalY) {
					grounded = true;
					if (yMovement) {
						groundNormal = currentNormal;
						currentNormal.x = 0;
					}
				} else {
					grounded = false;
				}

				float projection = Vector2.Dot(velocity, currentNormal);
				if (projection < 0) {
					velocity = velocity - projection * currentNormal;
				}

				float modifiedDistance = hitBufferList[i].distance - shellRadius;
				distance = modifiedDistance < distance ? modifiedDistance : distance;
			}

			if (velocity.x == 0 && tryFlip) {
				Flip();
			}

			if (velocity.y < 0 && grounded == false) {      //If have y movement and not grounded, character is falling
				isFalling = true;
			} else {
				isFalling = false;
			}

		}
		rb2d.position = rb2d.position + move.normalized * distance;
	}

	/**
	 * Knocks back the enemy left or right with the given force and duration
	 * based on the given parameters
	 */ 
	public IEnumerator Knockback(float knockbackDuration, float knockbackForce, bool knockLeft) {
		float timer = 0;
		enemyKnockedBack = true;

		while (knockbackDuration > timer) {
			timer += Time.deltaTime;
			
			if (knockLeft) {
				knockedVelocity = new Vector2(-knockbackForce, 0);      //horizontal knockback depending on which side hit from
			} else {
				knockedVelocity = new Vector2(knockbackForce, 0);
			}
		}
		yield return new WaitForSeconds(0.2f);
		enemyKnockedBack = false;
	}

	/**
	 * Flips the sprite as well as its x speed
	 */ 
	private void Flip() {
		moving *= -1;
		platformCheck.localPosition = new Vector2(-1 * platformCheck.localPosition.x, -8);
		tryFlip = false;
	}

	/**
	 * Clamps the enemy's x position to make sure it does not leave the current room it is in
	 */ 
	public Vector3 ClampPosition(GameObject currentRoom, Vector3 enemyPosition) {
		float halfWidth = 0;
		float offset = 8;
		try {
			halfWidth = currentRoom.GetComponent<BoxCollider2D>().size.x / 2;
		} catch {
			print("BoxCollider2D probably does not exist on the object.");
		}

		float finalXPos = Mathf.Clamp(enemyPosition.x, currentRoom.transform.position.x - halfWidth + offset,
			currentRoom.transform.position.x + halfWidth - offset);

		Vector3 newBounds = new Vector3(finalXPos, this.transform.position.y, -1);
		return newBounds;
	}

	/****************************************************************************************
	 *																						*
	 *																						*
	 *									Enemy_stats											*
	 *																						*
	 *																						*
	 ****************************************************************************************/

	/**
	 * Damages the enemy based on the given damage value and calls the enemy's knockback
	 * coroutine
	 */
	public void damageEnemy(float damageValue, GameObject collidingObject) {            //damages this enemy by designated amount
		if (!invulnerable) {
			currentHealth -= damageValue;
			if (currentHealth <= 0) {
				Instantiate(poof, transform.position, Quaternion.identity);
				SpawnLoot(smallLoot);
				GameMaster.EnemyDeathSFX();
				Destroy(gameObject);
			} else {
				GameMaster.EnemyHurtSFX();
			}
		}
		StartCoroutine(Invulnerable());
		Vector3 relativePosition = transform.InverseTransformPoint(collidingObject.transform.position); //which side of player was hit?
		if (relativePosition.x > 0) {
			Debug.Log("Right");
			StartCoroutine(Knockback(0.2f, 100, true));
		} else {
			Debug.Log("Left");
			StartCoroutine(Knockback(0.2f, 100, false));
		}
	}

	/**
	 * Used for flying enemies to do other animations
	 */ 
	public void DoneAwake() {
		try {
			anim.SetBool("open", true);
		} catch {
			Debug.LogError("Parameter \"open\" does not exist.");
		}
	}

	/**
	 * Sets the enemy invulnerable and plays the hurt animation for it
	 */ 
	private IEnumerator Invulnerable() {
		print("enemy is invulnerable.");
		invulnerable = true;
		anim.SetBool("hurt", true);
		yield return new WaitForSeconds(0.5f);
		invulnerable = false;
		anim.SetBool("hurt", false);
	}

	/**
	 * Spawns loot based on whether or not the sprite should spawn
	 * small lot or not
	 */ 
	private void SpawnLoot(bool smallLoot) {
		int lootCount;
		int lootRange;
		float spawnRangex;
		if (smallLoot) {
			lootCount = Random.Range(0, 5);
			for (int i = 0; i < lootCount; i++) {
				lootRange = Random.Range(0, 5);
				spawnRangex = Random.Range(-40, 40f);
				GameObject collectible = collectibles.transform.GetChild(lootRange).gameObject;
				GameObject item = Instantiate(collectible, this.transform.position, Quaternion.identity);
				item.GetComponent<Rigidbody2D>().velocity = new Vector2(spawnRangex, 150);
			}

		} else {
			lootCount = Random.Range(1, 4);
			for (int i = 0; i < lootCount; i++) {
				lootRange = Random.Range(3, 7);
				spawnRangex = Random.Range(-40, 40f);
				GameObject collectible = collectibles.transform.GetChild(lootRange).gameObject;
				GameObject item = Instantiate(collectible, this.transform.position, Quaternion.identity);
				item.GetComponent<Rigidbody2D>().velocity = new Vector2(spawnRangex, 150);

			}
		}
	}
}

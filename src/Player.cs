using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using Affdex;
using Tobii.Gaming;
using UnityEngine.SceneManagement;

/*
 * This	subclass of	Physics	Object contains	information	and	stats about	the	player.	This will keep track 
 * of the player's health, healing and damaging	the	player,	what room they are currently in, and if	the
 * player is leaving or	entering a room.
 */
public class Player	: PlayerController {

	//public float currentHealth { get; set; }
	//public float maxHealth { get; set; }

	public GameObject point;
	
	private	Camera cam;
	[HideInInspector]
	public GameObject newRoom; 
	[HideInInspector]
	public GameObject currentRoom; 

	private	Vector2	newRoomBL, newRoomTR;
	private	Vector2	currentRoomBL, currentRoomTR;
	private	Vector2	camBounds;
	private	Vector2	currentTransitionDir;
	private	Vector2	tempVelocity, tempMove;
	private	Vector3	targetPos;
	//private	int	healthContainers = 8;
	private	int	fallBoundary = 64;
	private	float halfHeight, halfWidth;
	private	float horizontalPlayerBuffer = 16, verticalPlayerBuffer = 24, guiBuffer = 16;
	private	float minLerp =	1;
	private	float transitionSpeed =	8;
	private	Animator gazeUIAni;
	private	Animator pointerAni;
    private float flashSpeed = 0.08f;

	[HideInInspector]
	public GameObject pointer;

	private GazePoint gazePoint;
	private Vector2 gaze;

	private float holdTime = 3;
	/* Emotion tracking */
	private ImageResultsParser userEmotions;
	private float counterOne = 0;
	private float counterTwo = 0;
	private bool usingPowerup;
	private int tempSpeed;
	private float tempJumpSpeed;

	void Start () {
		GameMaster.CurrentPlayerHealthUpdate(GameMaster.CurrentPlayerHealth());
		userEmotions = GetComponent<ImageResultsParser>();

		cam = Camera.main;
		pointer = Instantiate(point, transform);

		halfHeight = cam.orthographicSize;		//half of the height of	the	camera
		halfWidth =	halfHeight * cam.aspect;    //half of the width
		if (biofeedback) {
			//gazeUIAni = GameObject.FindWithTag("GazeUI").GetComponent<Animator>();    //get the animator of the gaze radius ui sprite
			gazeUIAni = GameObject.Find("gazerad_0_bf").GetComponent<Animator>();
		} else {
			gazeUIAni = GameObject.Find("gazerad_0_ctrl").GetComponent<Animator>();
		}
		// is always null?
		//Physics2D.IgnoreCollision(GameObject.FindGameObjectWithTag("collectible").GetComponent<Collider2D>(), GetComponent<Collider2D>());
		//Physics2D.IgnoreCollision(GameObject.FindGameObjectWithTag("collectible_ground").GetComponent<Collider2D>(), GetComponent<Collider2D>());
		pointerAni = pointer.GetComponent<Animator>();
		GameMaster.PlayerIsInvulnerableCheck(false);
		GameMaster.PlayerIsKnockedBackCheck(false);
		GameMaster.CheckSwingingSword(false);
		//need to set respawning to false when done respawning
		StartCoroutine(doneSpawning());
		usingPowerup = false;
		tempSpeed = maxSpeed;
		tempJumpSpeed = jumpTakeOffSpeed;
		gaze = transform.position;
	}

	/*********************
	 * Protected methods *
	 *********************/
	//make a version of this method that will add to joy and anger depending on if hitting enemy, collect coin, getting hit, etc
	protected override void FacialExpressions() {

		if (biofeedback) {
			if (!usingPowerup) {
				if (userEmotions.joyLevel > 0.0022f) {
					if (userEmotions.smiling > 10 || userEmotions.innerBrowRaise > 0.01f || userEmotions.noseWrinkle > 0.4f) {
						counterOne += 0.1f;
					}
				}
				if (counterOne >= 5) {
					counterOne = 0;
					GameMaster.AddToJoy(10);
				}
				if (userEmotions.angerLevel > 0.01f) {
					if (userEmotions.browRaise > 10 || userEmotions.browFurrow > 0.01f || userEmotions.upperLipRaise > 0.4f) {
						counterTwo += 0.1f;
					}
				}
				if (counterTwo >= 5) {
					counterTwo = 0;
					GameMaster.AddToAnger(10);
				}
			}
		}
	}

	/*
	 * Powerup methods
	 */
	protected override void UsePowerup() {
		if (GameMaster.CanUseJoy() && !usingPowerup) {
			if (Input.GetButtonDown("PS4_Triangle") && !usingPowerup) {
				GameMaster.UpgradeSFX();
				GameMaster.ResetJoy();
				GameMaster.UpgradeJoy();
				StartCoroutine(PowerupTimer(true));
			}
		}

		if (GameMaster.CanUseAnger() && !usingPowerup) {
			if (Input.GetButtonDown("PS4_O") && !usingPowerup) {
				GameMaster.UpgradeSFX();
				GameMaster.ResetAnger();
				GameMaster.UpgradeAnger();
				maxSpeed = 100;
				jumpTakeOffSpeed = 460;
				StartCoroutine(PowerupTimer(false));
			}
		}
	}

	private IEnumerator PowerupTimer(bool joy) {
		usingPowerup = true;

		yield return new WaitForSeconds(15f);
		GameMaster.EndUpgradeSFX();
		GameMaster.ResetUpgrade();
		if (joy) {
			GameMaster.UpgradeBlaster(false);
			GameMaster.UpgradeSword(false);
			GameMaster.ResetJoy();
		} else {
			maxSpeed = tempSpeed;
			jumpTakeOffSpeed = tempJumpSpeed;
			GameMaster.ResetAnger();
		}
		usingPowerup = false;
	}

	/**
	 * Detects if the player falls out of bounds and subsquently kills the player if true
	 */ 
	private void FallingOutOfBounds() {
		if (transform.position.y <= currentRoomBL.y - fallBoundary && !GameMaster.PlayerRespawnState()) {
			//if player	falls off the bottom of	the	screen,	kill the player
			Damageplayer(99999, gameObject);
		}
	}

	/*
	 * Updates the GazePoint so all methods in this class can access its position.
	 */
	protected override void GetGaze() {
		FallingOutOfBounds();

		if (Input.GetKeyDown(KeyCode.U)) {
			GameMaster.AddToJoy(100);
			GameMaster.AddToAnger(100);
		}

		if (biofeedback) {
			gazePoint = TobiiAPI.GetGazePoint();
		}

		if (Input.GetAxis(ControllerInputManager.Grab()) > 0) {
			pointerAni.SetBool("holdL", true);
			gazeUIAni.SetBool("holdL", true);
			Vector3 centerPt;
			float radius;
			float distance;

			//if using biofeedback, will set pointer to gaze positions
			if (biofeedback) {
				gaze = Camera.main.ScreenToWorldPoint(gazePoint.Screen);

				centerPt = GetComponentInChildren<SphereCollider>().transform.position;
				radius = GetComponentInChildren<SphereCollider>().radius;
				
				distance = Vector3.Distance(Camera.main.ScreenToWorldPoint(gazePoint.Screen), centerPt);

				if (distance > radius) {
					Vector3 newPos = transform.position - centerPt;
					newPos *= radius / distance;
					pointer.transform.position = centerPt + newPos;
				} else {
					pointer.transform.position = gaze;
				}
			} else {
				//else will move via analog stick movement
				pointer.transform.position += new Vector3(15 * Input.GetAxis("PS4_RightAnalogHorizontal"),
					-15 * Input.GetAxis("PS4_RightAnalogVertical"), 0);
				
				centerPt = GetComponentInChildren<CircleCollider2D>().transform.position;
				radius = GetComponentInChildren<CircleCollider2D>().radius;
				distance = Vector3.Distance(pointer.transform.position, centerPt);

				if (distance > radius) {
					Vector3 newPos = pointer.transform.position - centerPt;
					newPos *= radius / distance;

					pointer.transform.position = centerPt + newPos;
				}
			}
		} else {
			if (pointer != null) {
				pointerAni.SetBool("holdL", false);
			}
			gazeUIAni.SetBool("holdL", false);
		}
	}


	IEnumerator LoadYourAsyncScene() {
		// The Application loads the Scene in the background as the current Scene runs.
		// This is particularly good for creating loading screens.
		// You could also load the Scene by using sceneBuildIndex. In this case Scene2 has
		// a sceneBuildIndex of 1 as shown in Build Settings.

		AsyncOperation asyncLoad = SceneManager.LoadSceneAsync("Level2-Fort");

		// Wait until the asynchronous scene fully loads
		while (!asyncLoad.isDone) {
			yield return null;
		}
	}

	/************************************
	 * Character Health Related Methods *
	 ************************************/
	/*
	 * Damages the player with the given damageValue amount. Will change the fill amount of	the	healthbar as well
	 * based on	the	player's current health. If	the	player's health	drops to <=	0, calls GameMaster	to kill	the	player.
	 */
	private void Damageplayer(float damageValue, GameObject collidingObject) {
		GameMaster.PlayHurt	();
		GameMaster.CurrentPlayerHealthUpdate(GameMaster.CurrentPlayerHealth() - damageValue);
		if (GameMaster.CurrentPlayerHealth() <= 0) {
			GameMaster.CurrentPlayerHealthUpdate(0);
			Debug.Log("You dead.");
			GameMaster.KillPlayer(this);
		}
		GameMaster.PlayerIsInvulnerableCheck(true);
		StartCoroutine(Flash(flashSpeed));
		Vector3 relativePosition = transform.InverseTransformPoint(collidingObject.transform.position);	//which side of player was hit?
		if (relativePosition.x > 0) {
			StartCoroutine(Knockback(0.8f, 100, true));
		} else {
			StartCoroutine(Knockback(0.8f, 100, false));
		}
	}

	/*
	 * Heals the player	with the given healValue amount. Will change the fill amount of	the	healthbar as well
	 * based on	the	player's current health. Makes sure	that current health	doesn't	go over	the	maximum	value.
	 */
	public void	Heal(float healValue) {
		Debug.Log ("Healed for " + healValue);
		GameMaster.PlayHeal	();
		GameMaster.CurrentPlayerHealthUpdate(GameMaster.CurrentPlayerHealth() + healValue); //Heals	the	character for healValue	amount
		
		if(GameMaster.CurrentPlayerHealth() > GameMaster.MaxPlayerHealth()) {
			Debug.Log ("Health is full");
			GameMaster.CurrentPlayerHealthUpdate(GameMaster.MaxPlayerHealth());
		}
	}


	/*******************************
	 * Collision Detection Methods *
	 *******************************/ 

	void OnTriggerEnter2D(Collider2D coll) {					//When touches another 2D collider trigger
		if (coll.gameObject.tag	== "HealthPackSmall") {
			Heal (25);
			Destroy	(coll.gameObject);
		}

		if (coll.gameObject.tag	== "HealthPackLarge") {
			Heal (25);
			Destroy	(coll.gameObject);
		}

		if(coll.gameObject.tag == "Checkpoint") {
			GameMaster.SetSpawningRoom(currentRoom);
		}

		if(!biofeedback) {
			if (coll.gameObject.tag == "gem") {
				GameMaster.AddToJoy(20);
			}

			if(coll.gameObject.tag == "diamond") {
				GameMaster.AddToJoy(50);
			}

			if (coll.gameObject.tag == "collectible" || coll.gameObject.tag == "collectible_ground") {
				GameMaster.AddToJoy(10);
			}
		}

		if (coll.gameObject.tag == "EnemySm" && !GameMaster.PlayerIsInvulnerable()) {
			if (!biofeedback) {
				GameMaster.AddToAnger(5);
			}
			Damageplayer(5, coll.gameObject);
		}

		if (coll.gameObject.tag == "Enemy" && !GameMaster.PlayerIsInvulnerable()) {
			if (!biofeedback) {
				GameMaster.AddToAnger(10);
			}
			Damageplayer(10, coll.gameObject);
		}

		/* This check is the "Room transition" check. Rooms in this game are based off of objects with BoxCollider2D triggers 
		 +	This makes it so that when entering a new "room" (entering another trigger), the following happens:
		 +		- The player is diabled from moving or pressing any buttons to prevent unwanted effects
		 +		- The player's current room is updated
		 +		- The camera transitions to the new room
		 +		- The player finishes transitioning and can now freely move about
		 */
		if (coll.gameObject.tag	== "Room" && newRoom !=	coll.gameObject	&& !GameMaster.PlayerTransitionState()) {
			//If we're triggering a	new	room, the room isn't the one we're in or transitioning to, and we aren't current transitioning
			//Hold the player's	velocity before	the	transition starts
			GameMaster.PlayerTransitionStateCheck(true);
			tempVelocity = velocity;

			//Disable the player from moving
			DisablePlayer();

			//Get the new room
			newRoom	= coll.gameObject;

			//Get the bottom left of the room, using the center of the room as the origin
			newRoomBL =	new	Vector2(Mathf.Round((coll.transform.position.x - (coll.bounds.size.x / 2)) * 2)	/ 2,
				Mathf.Round((coll.transform.position.y - (coll.bounds.size.y / 2)) * 2)	/ 2);

			//Get the top right of the room, using the center of the room as the origin
			newRoomTR =	new	Vector2(Mathf.Round((coll.transform.position.x + (coll.bounds.size.x / 2)) * 2)	/ 2,
				Mathf.Round((coll.transform.position.y + (coll.bounds.size.y / 2)) * 2)	/ 2);  

			//Create a local copy of the player's position at the start	of the transition
			Vector3	playerPos =	rb2d.transform.position;

			//Find whether the transition will be horizontal or	vertical
			if (newRoomBL.x	>= currentRoomTR.x)	{			//If the new room's	bottom left	is further right than our current top right
				currentTransitionDir = Vector2.right;			//Then we must be transitioning	right
			} else if (newRoomTR.x <= currentRoomBL.x) {	//Otherwise	if the new room's top right	is further left	than our current bottom	left 
				currentTransitionDir = Vector2.left;			//Transitioning	Left 
			} else if (newRoomBL.y >= currentRoomTR.y ){	//If the new room's	bottom left	is further up (higher y	value) than	the	current	room's top right
				currentTransitionDir = Vector2.up;				//Transitioning	Up
			} else if (newRoomTR.y <= currentRoomBL.y) {	//If the new room's	top	right is lower than	the	current	rooms bottom left
				currentTransitionDir = Vector2.down;			//Transitioning	Down
			} else {
				currentTransitionDir = Vector2.zero;		//Placeholder for if something goes	wrong
			}

			targetPos =	ClampCamera(newRoom, cam.transform.position); //Clamp the camera's position	to the bounds of the new room
		}

		if(coll.gameObject.tag == "EnemyProjectile" && !GameMaster.PlayerIsInvulnerable()) {
			if(!biofeedback) {
				GameMaster.AddToAnger(10);
			}
			Damageplayer(10, coll.gameObject);
		}
		if (coll.gameObject.tag == "Hazards" && !GameMaster.PlayerIsInvulnerable()) {
			Damageplayer(1000, coll.gameObject);
		}
	}

	private void OnCollisionEnter2D(Collision2D coll) {
		if (coll.gameObject.tag == "Hazards" && !GameMaster.PlayerIsInvulnerable()) {
			Damageplayer(1000, coll.gameObject);
		}
	}

	void OnCollisionStay2D(Collision2D coll) {
		if (coll.gameObject.tag == "EnemySm" && !GameMaster.PlayerIsInvulnerable()) {
			if (!biofeedback) {
				GameMaster.AddToAnger(5);
			}
			Damageplayer(5, coll.gameObject);
		}

		if(coll.gameObject.tag == "Enemy" && !GameMaster.PlayerIsInvulnerable()) {
			if (!biofeedback) {
				GameMaster.AddToAnger(10);
			}
			Damageplayer(10, coll.gameObject);
		} 

		if (coll.gameObject.tag == "Hazards" && !GameMaster.PlayerIsInvulnerable()) {
			Damageplayer(1000, coll.gameObject);
		}
	}

	private void OnTriggerStay2D(Collider2D coll) {
		if ((coll.gameObject.tag == "EnemySm" || coll.gameObject.tag == "Enemy") && !GameMaster.PlayerIsInvulnerable()) {
			Damageplayer(10, coll.gameObject); ;
		}
	}


	/****************************************************************************************
	 *																						*
	 *									  Room_based										*
	 *									camera_methods										*
	 *																						*
	 *		Disclaimer: the following methods are based off of code found on				*
	 *		blog.phantombadger.com. Much of the code was incomplete or not					*
	 *		present so I developed my own methods to get the player to						*
	 *		be disabled/enabled correctly, to get the camera's position to					*
	 *		be clamped, etc.																*
	 *																						*
	 ****************************************************************************************/
	protected override void	RoomBasedCamera() {
		//Save the current camera bounds so	even if	the	screen is re-sized the transition remains consistent
		//camBounds	= new Vector2(cam.nativeResolutionWidth	/ pixelPerMeter, cam.nativeResolutionHeight	/ pixelPerMeter);
		camBounds =	new	Vector2(halfWidth*2, halfHeight*2);

		if (GameMaster.PlayerTransitionState()) {
			TransitionCamera ();        //Call the transition camera method
			//do not transition	if character is	respawning;	prevents character from	respawning at an unwanted location
			if (!GameMaster.PlayerRespawnState()) {		
				TransitionPlayer ();    //Call the transition player method
			}
		} else {						//If we're not transitioning
			PanCamera ();				//Pan the camera within	the	bounds of the current room
		}
	}

	/*
	 * Pans	the	camera to the new room and clamps the camera to	the	new	room's bounds
	 */	
	void PanCamera() {
		//Set the cameras center to	the	player
		Vector3 newCamPos =	new	Vector3(transform.position.x, cam.transform.position.y, cam.transform.position.z);
		if(newRoom != null) {
			//Re-clamp it	to the bounds of the room, so that it follows the player, but doesnt leave the current room
			cam.transform.position = ClampCamera(newRoom, newCamPos); 
		}
	}

	/*
	 * Clamps the camera's x position and y	position to	the	bounds of the new room
	 * so that it does not move	outside	these bounds.
	 */	
	public Vector3 ClampCamera(GameObject newRoom, Vector3 cameraPosition) {
		//clamp	the	camera's x position	to the minimum and maximum x values	of the new room
		float finalXPos	= Mathf.Clamp(cameraPosition.x,	newRoom.GetComponent<BoxCollider2D>().bounds.min.x + halfWidth,	
			newRoom.GetComponent<BoxCollider2D>().bounds.max.x - halfWidth);
		//clamp	the	camera's y position	to the minimum and maximum y values	of the new room
		float finalYPos	= Mathf.Clamp(cameraPosition.y,	newRoom.GetComponent<BoxCollider2D>().bounds.min.y + halfHeight, 
			newRoom.GetComponent<BoxCollider2D>().bounds.max.y - halfHeight);
		Vector3	newBounds =	new	Vector3(finalXPos, finalYPos, -10);
		return newBounds;
	}

	/*
	 * Smoothly	transitions	the	camera to the new room
	 */	
	void TransitionCamera()	{
		Vector3	newCamPos =	targetPos; //Create	a local	copy of	the	target position	(as	the	new	camera position)

	
		if (Mathf.Abs(targetPos.x -	cam.transform.position.x) >	minLerp) {	//Lerp camera between two positions
			newCamPos.x	= Mathf.Lerp(cam.transform.position.x, targetPos.x,	Time.deltaTime * transitionSpeed);	//If the x has changed,	then move along	the	x
		} else if (Mathf.Abs(targetPos.y - cam.transform.position.y) > minLerp)	{
			newCamPos.y	= Mathf.Lerp(cam.transform.position.y, targetPos.y,	Time.deltaTime * transitionSpeed);	//If the y has changed,	then move along	the	y
		} else {	//Both x and y are equal so	the	camera has stopped moving
			newCamPos =	targetPos;
			GameMaster.PlayerTransitionStateCheck (false);

			EnablePlayer();		//Enable Player	again once the camera is in	position

			//Make the current room	equal the new room
			currentRoomBL =	newRoomBL;
			currentRoomTR =	newRoomTR;
			currentRoom	= newRoom;
		}

		newCamPos.z	= cam.transform.position.z;	//Set the camera's z axis position to what it was before
		cam.transform.position = newCamPos;		//moves	the	camera
	}

	void DisablePlayer() {
		velocity = Vector2.zero;
		rb2d.constraints = RigidbodyConstraints2D.FreezeAll;
		animator.enabled = false;
		//Input.ResetInputAxes(); //TODO:would prevent the player	from bringing a	block with them, but resets	ALL	player directional input
	}

	void EnablePlayer()	{
		velocity = tempVelocity;
		rb2d.constraints = RigidbodyConstraints2D.None | RigidbodyConstraints2D.FreezeRotation;
		animator.enabled = true;
	}

	void TransitionPlayer()	{
		Vector3	playerPos =	transform.position;

		//Lerp player between the four directions
		if (currentTransitionDir ==	Vector2.right) {
			//If the player	has	reached	the	target position	(including a horizontal	offset)	then we	can	finish the transition
			if (Mathf.Abs(targetPos.x +	horizontalPlayerBuffer - playerPos.x - (camBounds.x	/ 2)) >	minLerp) {
				//Right	transition
				playerPos.x	= Mathf.Lerp(playerPos.x, targetPos.x +	horizontalPlayerBuffer - (camBounds.x /	2),	Time.deltaTime * transitionSpeed);
			} else {
				//Right	Lerp finished
				playerPos.x	= targetPos.x +	horizontalPlayerBuffer - (camBounds.x /	2);
			}
		} else if (currentTransitionDir	== Vector2.left) {
			if (Mathf.Abs(((targetPos.x	+ (camBounds.x / 2)) - horizontalPlayerBuffer) - transform.position.x)	> minLerp) {
				//Left transition
				playerPos.x	= Mathf.Lerp(playerPos.x, (targetPos.x + (camBounds.x /	2))	- horizontalPlayerBuffer, Time.deltaTime * transitionSpeed);
			} else {
				//Left Lerp	finished
				playerPos.x	= (targetPos.x + (camBounds.x /	2))	- horizontalPlayerBuffer;
			}
		} else if (currentTransitionDir	== Vector2.up) {
			if (Mathf.Abs((targetPos.y + verticalPlayerBuffer) - playerPos.y - (camBounds.y	/ 2)) >	minLerp) {
				//Up Transition
				playerPos.y	= Mathf.Lerp(playerPos.y, targetPos.y +	verticalPlayerBuffer - (camBounds.y	/ 2), Time.deltaTime * transitionSpeed);
			} else {
				//Up Lerp finished
				playerPos.y	= targetPos.y +	verticalPlayerBuffer - (camBounds.y	/ 2);
			}
		} else if (currentTransitionDir	== Vector2.down) {
			//The player has to	be moved a little further due to the space the GUI takes up
			if (Mathf.Abs(((targetPos.y	+ (camBounds.y / 2)) - (verticalPlayerBuffer + guiBuffer)) - playerPos.y) >	minLerp) {
				//Down Transition
				playerPos.y	= Mathf.Lerp(playerPos.y, (targetPos.y + (camBounds.y /	2))	- (verticalPlayerBuffer	+ guiBuffer), Time.deltaTime * transitionSpeed);
			} else {
				//Down Lerp	finished
				playerPos.y	= (targetPos.y + (camBounds.y /	2))	- (verticalPlayerBuffer	+ guiBuffer);
			}
		}
		transform.position	= playerPos;
	}

	/****************************************************************************************
	 *																						*
	 *																						*
	 *										Coroutines										*
	 *																						*
	 *																						*
	 ****************************************************************************************/

	//required to make sure player doesnt transition when respawning
	private IEnumerator doneSpawning() {
		yield return new WaitForSeconds(1);
		GameMaster.PlayerIsRespawningCheck(false);
	}

	private	IEnumerator	Flash(float	x) {
		for	(int i = 0;	i <	10;	i++) {
			spriteRenderer.enabled = false;
			yield return new WaitForSeconds(x);
			spriteRenderer.enabled = true;
			yield return new WaitForSeconds(x);
		}
		GameMaster.PlayerIsInvulnerableCheck(false);
	}

	public IEnumerator Knockback(float knockbackDuration, float knockbackForce, bool knockLeft) {
		GameMaster.PlayerIsKnockedBackCheck(true);
		float timer = 0;
		
		while (knockbackDuration > timer) {
			timer += Time.deltaTime;
			
                velocity = new Vector2(0, knockbackForce * 1.5f);           //vertical knockback
                if (knockLeft) {
                    knockedVelocity = new Vector2(-knockbackForce, 0);		//horizontal knockback depending on which side hit from
                } else {
                    knockedVelocity = new Vector2(knockbackForce, 0);
                }
		}
		yield return new WaitForSeconds(0.4f);
		GameMaster.PlayerIsKnockedBackCheck(false);
	}
}

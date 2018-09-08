using UnityEngine;

/*
 * PlayerController script by Unity3D PennyPixel tutorials
 * 		Modified by Drew Noma.
 * 
 * This subclass of PhysicsObject takes care of all user input for the player sprite.
 * Depending on the states calculated in the PhysicsObject, this script will flip the
 * 		player if needed, set animations states as needed, and computes the velocity of
 * 		the player.
 * 
 * A couple notes for MacOS vs Windows PS4 controller settings:
 * 	- L2 on the controller uses 5th Axis on Mac but 6th on Windows
 * 	- Up on the dpad is < 0, while down is > 0 on Mac, but up > 0 and down < 0 on Windows
 * 
 */
public class PlayerController : PhysicsObject {

	public int maxSpeed = 80;
	public float jumpTakeOffSpeed = 380;

	protected SpriteRenderer spriteRenderer;
	protected Animator animator;
	protected bool knockedBack;
	protected bool knockedOnGround;
	protected Vector2 move;
	protected float lastMove;

	void Awake() {
		spriteRenderer = GetComponent<SpriteRenderer> (); 	//renders the player sprite
		animator = GetComponent<Animator> ();				//set up animator for player animation
		GameMaster.PlayerIsRespawningCheck(true);
		animator.SetBool ("isRespawning", GameMaster.PlayerRespawnState());
		grounded = true;
		animator.SetBool ("grounded", grounded);            //do this on awake so that player doesn't spawn with jumping animation
	}

	/*
	 * Protected method from the parent class PhysicsObject. Will calculate velocity based on player input.
	 * This method also sets the player's animation states based on these velocity values.
	 * Also checks player input to do other things as well.
	 * 
	 */ 
	protected override void ComputeVelocity() {				//gets called in the PhysicsObject script
		move = Vector2.zero;

		//		move.x = Input.GetAxis("PS4_DPadHorizontal");      //sets vector 2 movement to controller movement
		if (!GameMaster.Swinging()) {
			move.x = Input.GetAxisRaw(ControllerInputManager.Horizontal());
			//lastMove = move.x;
		} else {
			move.x = Input.GetAxisRaw(ControllerInputManager.Horizontal()) / 1.2f;
		}

		if (GameMaster.OnLadder()) {
			//TODO: have 2 frames that alternate (exit time w/ event method) while climbing
			if ((Input.GetAxisRaw(ControllerInputManager.Vertical()) == 0 || GameMaster.Swinging()) && climbing) {
				velocity = Vector2.zero;    // if climbing and not holding a button, don't move
			} else if (Input.GetAxisRaw(ControllerInputManager.Vertical()) > 0 && !GameMaster.Swinging()) {
				climbing = true;
				// checks to os so that vertical controls don't get reversed
				if (Application.platform == RuntimePlatform.WindowsPlayer || Application.platform == RuntimePlatform.WindowsEditor) {
					velocity = -10f * gravityModifier * Physics2D.gravity * Time.deltaTime;
				} else if (Application.platform == RuntimePlatform.OSXPlayer || Application.platform == RuntimePlatform.OSXEditor) {
					velocity = 10f * gravityModifier * Physics2D.gravity * Time.deltaTime;
				}

			} else if (Input.GetAxisRaw(ControllerInputManager.Vertical()) < 0 && !GameMaster.Swinging()) {
				climbing = true;

				if (Application.platform == RuntimePlatform.WindowsPlayer || Application.platform == RuntimePlatform.WindowsEditor) {
					velocity = 10f * gravityModifier * Physics2D.gravity * Time.deltaTime;
				} else if (Application.platform == RuntimePlatform.OSXPlayer || Application.platform == RuntimePlatform.OSXEditor) {
					velocity = -10f * gravityModifier * Physics2D.gravity * Time.deltaTime;
				}
			}

			if (grounded) {
				climbing = false;
				velocity += gravityModifier * Physics2D.gravity * Time.deltaTime;
			}
			
			if (Input.GetButtonDown(ControllerInputManager.Jump()) && climbing) {
				climbing = false;
				if (velocity.y >= 0) {
					velocity += gravityModifier * 3 * Physics2D.gravity * Time.deltaTime;
					animator.SetInteger("State", 3);
				}
			}

			if (!climbing) {
				if (Input.GetButtonDown(ControllerInputManager.Jump()) && grounded
					&& !GameMaster.PlayerTransitionState() && !GameMaster.IsPaused() 
					&& !GameMaster.Swinging()) {        //if jump button is pressed and is grounded
					GameMaster.PlayJump();
					velocity.y = jumpTakeOffSpeed;                  //velocity becomes jump speed
				} else if (Input.GetButtonUp(ControllerInputManager.Jump())) {          //else if jump is let go
					if (velocity.y > 0) {                           //reduce y speed and set animation state to walking if applicable
						velocity.y = velocity.y * 0.5f;
						animator.SetInteger("State", 3);
					}
				}
			}

		} else {
			climbing = false;

			if (Input.GetButtonDown(ControllerInputManager.Jump()) && grounded
				&& !GameMaster.PlayerTransitionState() && !GameMaster.IsPaused()
				&& !GameMaster.Swinging() && !climbing) {        //if jump button is pressed and is grounded
				velocity.y = jumpTakeOffSpeed;                  //velocity becomes jump speed
				GameMaster.PlayJump();
			} else if (Input.GetButtonUp(ControllerInputManager.Jump())) {          //else if jump is let go
				if (velocity.y > 0) {                           //reduce y speed and set animation state to walking if applicable
					velocity.y = velocity.y * 0.5f;
					animator.SetInteger("State", 3);
				}
			}
		}

		if (!GameMaster.PlayerTransitionState() && !GameMaster.Swinging()) {
			if (move.x > 0.01f) {                               //if has x speed, flip sprite direction when moving to move the correct way
				if (spriteRenderer.flipX == false) {
					spriteRenderer.flipX = true;
				}
			} else if (move.x < -0.01f) {
				if (spriteRenderer.flipX == true) {
					spriteRenderer.flipX = false;
				}
			}
		}

		if(isFalling && !GameMaster.PlayerRespawnState()) {
			animator.SetInteger ("State", 1);		//if falling, set animation state to falling
		}

		if(grounded) {
			animator.SetInteger ("State", 0);		//if grounded, set animation state to 0 (go back to idle)
		}

		animator.SetBool ("grounded", grounded);	//set animator boolean variables depending on state gathered from PhysicsObject
		animator.SetFloat ("velocityX", Mathf.Abs (velocity.x) / maxSpeed); //sets the animator parameter to this x velocity

		targetVelocity = move * maxSpeed;
	}

	/*
	 * Used in the animation to notify when the animation is done
	 */ 
	public void DoneSwinging() {
		animator.SetBool ("swordAttack", false);
		animator.SetBool("upgradedSwing", false);
		GameMaster.CheckSwingingSword (false);
	}
}

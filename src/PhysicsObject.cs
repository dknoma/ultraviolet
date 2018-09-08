using System.Collections;
using System.Collections.Generic;
using UnityEngine;

/*
 * PhysicsObject script by Unity3D PennyPixel tutorials
 * 		Modified by Drew Noma
 * 
 * This class takes care of all the physics interactions for the player.
 * Calls protected method to be called by the PlayerController script to update sprite direction and animation states.
 */
public class PhysicsObject : MonoBehaviour {

    public float minGroundNormalY = .65f;
    public float gravityModifier = 100f;
	public LayerMask mask;
	public bool biofeedback = true;

	protected Vector2 targetVelocity;
    protected bool grounded;
	protected bool isFalling;
	protected bool climbing;
	protected Vector2 groundNormal;
	protected Vector2 knockedVelocity;
    protected Rigidbody2D rb2d;
    protected Vector2 velocity;
    protected ContactFilter2D contactFilter;
    protected RaycastHit2D[] hitBuffer = new RaycastHit2D[16];
    protected List<RaycastHit2D> hitBufferList = new List<RaycastHit2D> (16);

    protected const float minMoveDistance = 0;
    protected const float shellRadius = 0.01f;

    void OnEnable() {
		rb2d = GetComponent<Rigidbody2D> ();
		GameMaster.PlayerIsRespawningCheck(true);
		GameMaster.UsingBiofeedback(biofeedback);
	}

    void Awake ()  {
        contactFilter.useTriggers = false;  //initially sets trigger colliding to false
		contactFilter.SetLayerMask(mask);
		contactFilter.useLayerMask = true;	//initially set to use layer masks
		climbing = false;
	}
    
    void Update () {
        targetVelocity = Vector2.zero;
		if (GameMaster.IsPaused()) {
		} else {
			ComputeVelocity();
		}
		GetGaze();
		MenuChecking();
		FacialExpressions();
		UsePowerup();
	}

	void LateUpdate() {
		RoomBasedCamera ();
	}

    void FixedUpdate() {
		if (!climbing) {
			velocity += gravityModifier * Physics2D.gravity * Time.deltaTime;
		}
		// go on to normal physics calculations

		if (!GameMaster.PlayerIsKnockedBack()) {
			/* If swinging sword, stop horizontal motion */
			if(GameMaster.Swinging() && grounded) {
				velocity.x = 0;
			} else {
				velocity.x = targetVelocity.x;                                                      //x velocity of target
			}
		} else if(GameMaster.PlayerIsKnockedBack()){
			velocity.x = knockedVelocity.x;
		}



		grounded = false;																	//grounded is initially false
        Vector2 deltaPosition = velocity * Time.deltaTime;

        Vector2 moveAlongGround = new Vector2 (groundNormal.y, -groundNormal.x);

		Vector2 move = moveAlongGround * deltaPosition.x;

        Movement (move, false);

        move = Vector2.up * deltaPosition.y;

        Movement (move, true);
    }

	/****************************************************************************************
	 *																						*
	 *																						*
	 *									Protected_Methods									*
	 *																						*
	 *																						*
	 ****************************************************************************************/

	protected virtual void ComputeVelocity() {		//like an abstract method in java
	}

	protected virtual void RoomBasedCamera() {
	}

	protected virtual void MenuChecking() {
	}

	protected virtual void GetGaze() {
	}

	protected virtual void FacialExpressions() {
	}

	protected virtual void UsePowerup() {
	}

	/****************************************************************************************
	 *																						*
	 *																						*
	 *									Physics_calculations								*
	 *																						*
	 *																						*
	 ****************************************************************************************/

	/*
	 * Calculates overall movement of the player.
	 */
	private void Movement(Vector2 move, bool yMovement){
        float distance = move.magnitude;

        if (distance > minMoveDistance) {
			int count = rb2d.Cast (move, contactFilter, hitBuffer, distance + shellRadius);
            hitBufferList.Clear ();
            for (int i = 0; i < count; i++) {
				//makes the raycast ignore objects with these tags to make sure the bodies do not collide
				if (hitBuffer[i].collider.tag != "Enemy" && hitBuffer[i].collider.tag != "collectible_ground") {
					//print(hitBuffer[i].distance);
					hitBufferList.Add(hitBuffer[i]);
				}
            }

            for (int i = 0; i < hitBufferList.Count; i++) {
                Vector2 currentNormal = hitBufferList [i].normal;
				//print("currentNormal.y = " + currentNormal.y);
                if (currentNormal.y > minGroundNormalY)  {
                    grounded = true;
                    if (yMovement) {
                        groundNormal = currentNormal;
                        currentNormal.x = 0;
                    }
                } else {
					grounded = false;
				}

                float projection = Vector2.Dot (velocity, currentNormal);
                if (projection < 0)  {
                    velocity = velocity - projection * currentNormal;
                }

                float modifiedDistance = hitBufferList [i].distance - shellRadius;
                distance = modifiedDistance < distance ? modifiedDistance : distance;
            }

			if (velocity.y < 0 && grounded == false) {		//If have y movement and not grounded, character is falling
				isFalling = true;
			} else{
				isFalling = false;
			}

        }
		rb2d.position = rb2d.position + move.normalized * distance;
    }
}

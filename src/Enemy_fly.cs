using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Tobii.Gaming;

/**
 * This class is for enemies who can fly freely through the level; They can't have their movement blocked
 * This specific enemy will keep chasing the player if they are not looking at it and will stop once looked at.
 * It also has a specific detection range so it won't chase the player when not on screen.
 */ 
[RequireComponent(typeof(GazeAware))]
public class Enemy_fly : MonoBehaviour {

	public float moveSpeed = 70;
	public bool biofeedback = true;
	public bool alwaysChase = true;

	private GazeAware gazeAware;
	private GazePoint gazePoint;
	private Player player;
	public Transform parent;
	private Rigidbody2D rb2d;
	private Animator anim;
	private SpriteRenderer render;
	private float nextTimeToSearch = 0;
	private Vector3 distance;
	private bool invulnerable;
	private bool open;
	private float detectionRange = 256;
	private bool activated;

	void Awake () {
		gazeAware = GetComponent<GazeAware>();
		player = GameObject.FindGameObjectWithTag("Player").GetComponent<Player>();
		parent = GetComponentInParent<Transform>();
		anim = GetComponentInParent<Animator>();
		rb2d = GetComponentInParent<Rigidbody2D>();
		render = GetComponentInParent<SpriteRenderer>();
		open = false;
	}

	void Update () {
		if (player == null) {
			FindPlayer();
			activated = false;
		} else {
			if (alwaysChase) {
				if (Vector2.Distance(player.transform.position, parent.position) <= detectionRange) {
					activated = true;
				}

				if (activated) {
					if (player.transform.position.x <= parent.position.x) {
						render.flipX = false;

						if (biofeedback) {
							if (!gazeAware.HasGazeFocus) {
								Move();
							} else {
								Stop();
							}
						} else {
							if (!player.GetComponent<SpriteRenderer>().flipX) {
								Move();
							} else {
								Stop();
							}
						}
					} else if (player.transform.position.x > parent.position.x) {
						render.flipX = true;

						if (biofeedback) {
							if (!gazeAware.HasGazeFocus) {
								Move();
							} else {
								Stop();
							}
						} else {
							if (player.GetComponent<SpriteRenderer>().flipX) {
								Move();
							} else {
								Stop();
							}
						}
					}
				} else {
					Stop();
				}
			} else {
				if (Vector2.Distance(player.transform.position, parent.position) <= detectionRange) {
					if (player.transform.position.x <= parent.position.x) {
						render.flipX = false;

						if (biofeedback) {
							if (!gazeAware.HasGazeFocus) {
								Move();
							} else {
								Stop();
							}
						} else {
							if (!player.GetComponent<SpriteRenderer>().flipX) {
								Move();
							} else {
								Stop();
							}
						}
					} else if (player.transform.position.x > parent.position.x) {
						render.flipX = true;

						if (biofeedback) {
							if (!gazeAware.HasGazeFocus) {
								Move();
							} else {
								Stop();
							}
						} else {
							if (player.GetComponent<SpriteRenderer>().flipX) {
								Move();
							} else {
								Stop();
							}
						}
					}
				} else {
					Stop();
				}
			}
		}
	}

	/**
	 * Adds to the sprite's velocity and sets all animator boolean values to true
	 */ 
	private void Move() {
		anim.SetBool("awaken", true);
		open = anim.GetBool("open");
		if (open) {
			rb2d.velocity += (Vector2) (player.transform.position - transform.position).normalized
			* moveSpeed * Time.deltaTime;
		}
	}

	/**
	 * Stops the sprite's velocity and sets all animator boolean values to false
	 */
	private void Stop() {
		rb2d.velocity = Vector3.zero;
		anim.SetBool("awaken", false);
		anim.SetBool("open", false);
		open = false;
	}

	/*
	 * Finds the player object. Used when the player spawns as new instances of the player
	 * will technically be a clone and the camera will disjoint after the player disappears.
	 * 
	 * Makes sure that the target of the script will never be null. Uses nextTimeToSearch
	 * in order to make sure that the method is not done every frame as it would be very
	 * taxing on the cpu.
	 */
	void FindPlayer() {
		if (nextTimeToSearch <= Time.time) {
			GameObject searchResult = GameObject.FindGameObjectWithTag("Player");
			if (searchResult != null) {
				player = searchResult.GetComponent<Player>();
			}
			nextTimeToSearch = Time.time + 0.5f;
		}
	}
}

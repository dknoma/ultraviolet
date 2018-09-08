using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class TileExplosion : MonoBehaviour {

	private Animator anim;

	void Start () {
		anim = GetComponent<Animator>();
		Destroy(gameObject, anim.GetCurrentAnimatorStateInfo(0).length);
	}

}

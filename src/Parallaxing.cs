using System.Collections;
using System.Collections.Generic;
using UnityEngine;

/**
 * Parallax scrolling for the background. Will set the parent of the background to the main camera as 
 * the background schould scroll relative to the camera, not worldspace
 */ 
public class Parallaxing : MonoBehaviour {

	public Transform[] backgrounds;	//array of all backgrounds
	public Transform staticBG;
	[Range(0.1f,2f)]
	public float smoothing = .2f;			//parallax smoothness
	private float[] parallaxScales;	//what to move the bg by

	private Transform cam;
	private Vector3 previousCamPos;
	private int bgSize;

	void Awake () {
		cam = Camera.main.transform;
	}

	void Start () {
		bgSize = backgrounds.Length;
		previousCamPos = cam.position;

		transform.SetParent(cam);	//sets these backgrounds parent to main camera
		if (staticBG != null) {
			staticBG.SetParent (cam);
		}

		parallaxScales = new float[bgSize];

		if (bgSize >= 3) {
			parallaxScales [0] = 1.5f;			//2nd furthest layer will move the slowest
			for (int i = 1; i < bgSize; i++) {
				parallaxScales [i] = 4 + (6 * i);	//sets the scaling for each layer
			} 
		} else {
			for (int i = 0; i < bgSize; i++) {
				parallaxScales [i] = 4 + (6 * i);	//sets the scaling for each layer
			} 
		}
	}

	void Update () {
		for (int i = 0; i < bgSize; i++) {
			if (backgrounds [i] != null) {
				float parallax = (previousCamPos.x - cam.position.x) * parallaxScales [i];

				float bgTargetPosX = backgrounds [i].position.x + parallax;	//realistic scrolling/closer = faster, farther = slower

				Vector3 bgTargetPos = new Vector3 (bgTargetPosX, backgrounds [i].position.y, backgrounds [i].position.z);

				backgrounds [i].position = Vector3.Lerp (backgrounds [i].position, bgTargetPos, smoothing * Time.deltaTime);
			}
		}
		previousCamPos = cam.position;
	}
}

using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Affdex;

//Parsed image data will be sent to the ImageResultsListener methods
public class ImageResultsParser : ImageResultsListener {

	/* emotion levels */
	public float joyLevel;
	public float angerLevel;

	/* positive */
	public float smiling;
	public float innerBrowRaise;
	public float noseWrinkle;

	/* "negative" */
	public float browRaise;
	public float browFurrow;
	public float upperLipRaise;

	public override void onFaceFound(float timestamp, int faceId) {

	}

	//called when the Affectiva SDK loses a facial detection
	public override void onFaceLost(float timestamp, int faceId) {
		//set emotion levels to 0 (will cause character to be Idle)
		joyLevel = 0;
		angerLevel = 0;

		smiling = 0;
		innerBrowRaise = 0;
		noseWrinkle = 0;

		browRaise = 0;
		browFurrow = 0;
		upperLipRaise = 0;
	}

	//called every second whether there is a face detected or not
	public override void onImageResults(Dictionary<int, Face> faces) {
		if (faces.Count > 0) {
			//set emotion levels
			faces[0].Emotions.TryGetValue(Emotions.Joy, out joyLevel);
			faces[0].Emotions.TryGetValue(Emotions.Anger, out angerLevel);

			//set facial expressions
			faces[0].Expressions.TryGetValue(Expressions.Smile, out smiling);
			faces[0].Expressions.TryGetValue(Expressions.InnerBrowRaise, out innerBrowRaise);
			faces[0].Expressions.TryGetValue(Expressions.NoseWrinkle, out noseWrinkle);

			faces[0].Expressions.TryGetValue(Expressions.BrowRaise, out browRaise);
			faces[0].Expressions.TryGetValue(Expressions.BrowFurrow, out browFurrow);
			faces[0].Expressions.TryGetValue(Expressions.UpperLipRaise, out upperLipRaise);
		}
	}
}


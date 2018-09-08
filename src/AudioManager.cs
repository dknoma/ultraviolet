using System.Collections.Generic;
using UnityEngine;

public class AudioManager : MonoBehaviour {

	public static AudioManager instance;
	
	[SerializeField]
	Sound[] sounds;							//create an array of Sound objects
	//maybe implement own HashMap class, w/ open hashing & string key

	void Awake (){
		if(instance != null) {
			Debug.LogError ("More than one AudioManager in the scene.");
		} else {
			instance = this;
		}
	}

	void Start() {
		for(int i = 0; i < sounds.Length; i++) {
			GameObject _go = new GameObject ("Sound_" + i + "_" + sounds[i].name);
			_go.transform.SetParent (this.transform);
			sounds[i].SetSource(_go.AddComponent<AudioSource>());
			sounds [i].SetLoop ();
		}
	}

	/**
	 * Returns the current song for the level
	 */ 
	public AudioSource getCurrentSong(string _name) {
		for (int i = 0; i < sounds.Length; i++) {
			if (sounds[i].name == _name) {
				return sounds[i].CurrentSong();
			}
		}
		Debug.Log("Song not found.");
		return null;
	}

	/**
	 * Plays the sound effect
	 */
	public void PlaySound (string _name) {
		for(int i = 0; i < sounds.Length; i++) {
			if(sounds[i].name == _name) {
				sounds [i].Play ();
				return;
			}
		}
		//Debug.Log ("Sound name not found in Dictionary");
//		if(sounds.ContainsKey(_name)){
//			sounds[_name].Play();
//			return;
//		} else {
//			Debug.Log ("Sound name not found in Dictionary");
//			return;
//		}
	}
}

[System.Serializable]
public class Sound {

	public string name;
	public AudioClip clip;
	public bool looping;

	[Range(0f, 1f)]			//adds a slider in the editor, clamping the volume range
	public float volume;
	public float pitch;

	private AudioSource source;

	public void SetLoop() {
		source.loop = looping;
	}

	public void SetSource (AudioSource _source) {
		source = _source;
		source.clip = clip;
	}

	public void Play() {
		source.volume = volume;
		source.pitch = pitch;
		source.Play ();
	}

	public AudioSource CurrentSong() {
		source.volume = volume;
		source.pitch = pitch;
		return source;
	}
}

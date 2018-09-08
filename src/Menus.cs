using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

/**
 * Menu class
 */ 
public class Menus : MonoBehaviour {

	public Image pauseMenuBG;
	public Image weaponsMenu;
	public Image pausedTile;
	public Image pauseMenuLeftCursor;
	public Image pauseMenuRightCursor;
	public Image resume;
	public Image controls;
	public Image test2;
	public Image test3;
	public Image test4;
	public Image keyboard;
	public Image controller;
	public Image exit;

	public Sprite pausedSprite;
	public Sprite controlsTitleSprite;


	private bool up;
	private bool isPressed;
	private bool mainPause;
	private bool controlsPause;
	private bool weaponPause;
	private Image currentMenu;
	private Transform[] pauseMenuOptions;
	private int currentPauseMenuOption;
	private Transform[] controlsOptions;
	private int currentControlsOption;

	private Transform[] previousOptions;

	private Image pauseTitle;
	private Image leftCursor;
	private Image rightCursor;
	private RectTransform optionRect;

	private void Start() {
		isPressed = false;
		weaponPause = false;
		pauseMenuOptions = new Transform[1];
		currentPauseMenuOption = 0;
		print (Application.platform);
	}


	void Update () {
		MenuChecking();
		if (GameMaster.IsPaused()) {
			moveSelect();
			selectOption();
		}
	}

	/************************
	 * Menu related methods *
	 ************************/

	protected void MenuChecking() {
		if (GameMaster.IsPaused()) {
			if (Input.GetButtonDown(ControllerInputManager.Pause()) /*|| Input.GetButtonDown(ControllerInputManager.Weapons())*/) {
				unpause();
			}
		} else if (!GameMaster.IsPaused() && !GameMaster.PlayerTransitionState()) {
			pause();
		}
	}
	
	/*
	 * Unpauses the game
	 */ 
	private void unpause() {
		if (!GameMaster.WeaponMenu()) {
			GameMaster.getCurrentSong().UnPause();
		}
		if (currentMenu == null) {
			print("No menu to clear.");
		} else {
			removeMenu(currentMenu);
		}
		mainPause = false;
		controlsPause = false;
		GameMaster.PlayUnpauseSFX();
		GameMaster.PauseCheck(false);
		Time.timeScale = 1;
	}

	/*
	 * Pauses the game
	 */ 
	private void pause() {
		if (Input.GetButtonDown(ControllerInputManager.Pause())) {
			mainPause = true;
			createMenu(pauseMenuBG);
			GameMaster.getCurrentSong().Pause();	//hard pause will stop the music
			GameMaster.PlayPauseSFX();
			GameMaster.PauseCheck(true);
			Time.timeScale = 0;						//sets the game's timescale to 0, effectively pausing the game
		}/* else if (Input.GetButtonDown(ControllerInputManager.Weapons())) {
			mainPause = false;
			weaponPause = true;
			createMenu(weaponsMenu);

			GameMaster.PlayPauseSFX();				//music stil plays to keep the flow of the game going
			GameMaster.PauseCheck(true);			//useful for weapon select menus
			Time.timeScale = 0;
		}*/
	}
	private void pauseMenu() {
		pauseMenuOptions = new Transform[3];
		pauseMenuOptions[0] = Instantiate(resume, currentMenu.transform).transform;
		pauseMenuOptions[1] = Instantiate(controls, currentMenu.transform).transform;
		pauseMenuOptions[2] = Instantiate(exit, currentMenu.transform).transform;

		currentPauseMenuOption = 0;

		optionRect = (RectTransform) pauseMenuOptions[currentPauseMenuOption].transform;

		pauseTitle = Instantiate(pausedTile, currentMenu.transform);
		leftCursor = Instantiate(pauseMenuLeftCursor, currentMenu.transform);
		rightCursor = Instantiate(pauseMenuRightCursor, currentMenu.transform);
		//leftCursor = Instantiate(pauseMenuLeftCursor, new Vector3(-32.5f, 70), Quaternion.identity);
		//rightCursor = Instantiate(pauseMenuRightCursor, new Vector3(32.5f, 70), Quaternion.identity);
	}

	/*
	 * Creates a menu when a pause button is pressed
	 */ 
	private void createMenu(Image menu) {
		//TODO: create vertical expanding effect for menu
		currentMenu = Instantiate(menu, transform);
		if (menu.gameObject.tag == "Pause") {
			pauseMenu();
		} else if (menu.gameObject.tag == "Weapons") {
			pauseMenuOptions = new Transform[1];
			currentPauseMenuOption = 0;
		}
	}

	/*
	 * Removes the current menu when the game is unpaused
	 */ 
	private void removeMenu(Image menu) {
		//TODO: create vertical shrinking effect for menu
		foreach (Transform child in this.transform) {
			Destroy(child.gameObject);
		}
	}

	/*
	 * Method that will move the cursors and what current option is selected
	 * Can only go up the menu if the curren option is not the first (0) option.
	 * Can only go down the menu if the current option is not the last (length - 1) option.
	 * If could go down if at length-1, get IndexOutOfBounds.
	 */ 
	private void moveSelect() {
		if (Application.platform == RuntimePlatform.WindowsPlayer || Application.platform == RuntimePlatform.WindowsEditor) {
			if (Input.GetAxisRaw(ControllerInputManager.Vertical()) > 0 && currentPauseMenuOption > 0 && !isPressed) {
				isPressed = true;
				currentPauseMenuOption--;
				GameMaster.SelectSFX ();

				optionRect = (RectTransform)pauseMenuOptions [currentPauseMenuOption].transform;
				
				/* This sets the positions of the cursors relative to the option's position and width */
				leftCursor.transform.position = new Vector2 (optionRect.transform.position.x - (optionRect.rect.width) - 35f,
					pauseMenuOptions [currentPauseMenuOption].transform.position.y);
				rightCursor.transform.position = new Vector2 (optionRect.transform.position.x + (optionRect.rect.width) + 35f,
					pauseMenuOptions [currentPauseMenuOption].transform.position.y);

			} else if (Input.GetAxisRaw(ControllerInputManager.Vertical()) < 0 && currentPauseMenuOption < pauseMenuOptions.Length - 1 && !isPressed) {
				isPressed = true;
				currentPauseMenuOption++;
				GameMaster.SelectSFX ();

				optionRect = (RectTransform)pauseMenuOptions [currentPauseMenuOption].transform;

				leftCursor.transform.position = new Vector2 (optionRect.transform.position.x - (optionRect.rect.width) - 35f,
					pauseMenuOptions [currentPauseMenuOption].transform.position.y);
				rightCursor.transform.position = new Vector2 (optionRect.transform.position.x + (optionRect.rect.width) + 35f,
					pauseMenuOptions [currentPauseMenuOption].transform.position.y);

			} else if (Input.GetAxisRaw(ControllerInputManager.Vertical()) == 0) {
				isPressed = false;
			}
		} else if (Application.platform == RuntimePlatform.OSXPlayer || Application.platform == RuntimePlatform.OSXEditor){
		/*--------------------------------------------------------------------------*
		 * MacOS has different settings for PS4 controllers. Up is < 0, down is > 0 *
		 * This changes the controls accordingly									*
		 *--------------------------------------------------------------------------*/
			if (GameMaster.UsingController ()) {
				if (Input.GetAxisRaw (ControllerInputManager.Vertical ()) < 0 && currentPauseMenuOption > 0 && !isPressed) {
					isPressed = true;
					currentPauseMenuOption--;
					GameMaster.SelectSFX ();
					//print("current: " + currentPauseMenuOption);

					optionRect = (RectTransform)pauseMenuOptions [currentPauseMenuOption].transform;
					
					/* This sets the positions of the cursors relative to the option's position and width */
					leftCursor.transform.position = new Vector2 (optionRect.transform.position.x - (optionRect.rect.width) - 10,
						pauseMenuOptions [currentPauseMenuOption].transform.position.y);
					rightCursor.transform.position = new Vector2 (optionRect.transform.position.x + (optionRect.rect.width) + 10,
						pauseMenuOptions [currentPauseMenuOption].transform.position.y);

				} else if (Input.GetAxisRaw (ControllerInputManager.Vertical ()) > 0 && currentPauseMenuOption < pauseMenuOptions.Length - 1 && !isPressed) {
					isPressed = true;
					currentPauseMenuOption++;
					GameMaster.SelectSFX ();

					optionRect = (RectTransform)pauseMenuOptions [currentPauseMenuOption].transform;

					leftCursor.transform.position = new Vector2 (optionRect.transform.position.x - (optionRect.rect.width) - 10,
						pauseMenuOptions [currentPauseMenuOption].transform.position.y);
					rightCursor.transform.position = new Vector2 (optionRect.transform.position.x + (optionRect.rect.width) + 10,
						pauseMenuOptions [currentPauseMenuOption].transform.position.y);

				} else if (Input.GetAxisRaw (ControllerInputManager.Vertical ()) == 0) {
					isPressed = false;
				}
			} else {
				if (Input.GetAxisRaw (ControllerInputManager.Vertical ()) > 0 && currentPauseMenuOption > 0 && !isPressed) {
					isPressed = true;
					currentPauseMenuOption--;
					GameMaster.SelectSFX ();

					optionRect = (RectTransform)pauseMenuOptions [currentPauseMenuOption].transform;
					
					/* This sets the positions of the cursors relative to the option's position and width */
					leftCursor.transform.position = new Vector2 (optionRect.transform.position.x - (optionRect.rect.width) - 10,
						pauseMenuOptions [currentPauseMenuOption].transform.position.y);
					rightCursor.transform.position = new Vector2 (optionRect.transform.position.x + (optionRect.rect.width) + 10,
						pauseMenuOptions [currentPauseMenuOption].transform.position.y);

				} else if (Input.GetAxisRaw (ControllerInputManager.Vertical ()) < 0 && currentPauseMenuOption < pauseMenuOptions.Length - 1 && !isPressed) {
					isPressed = true;
					currentPauseMenuOption++;
					GameMaster.SelectSFX ();
					optionRect = (RectTransform)pauseMenuOptions [currentPauseMenuOption].transform;

					leftCursor.transform.position = new Vector2 (optionRect.transform.position.x - (optionRect.rect.width) - 10,
						pauseMenuOptions [currentPauseMenuOption].transform.position.y);
					rightCursor.transform.position = new Vector2 (optionRect.transform.position.x + (optionRect.rect.width) + 10,
						pauseMenuOptions [currentPauseMenuOption].transform.position.y);

				} else if (Input.GetAxisRaw (ControllerInputManager.Vertical ()) == 0) {
					isPressed = false;
				}
			}
		}
	}

	/*
	 * Sets all the events for each option
	 */ 
	private void selectOption() {
		if (mainPause) {
			if (currentPauseMenuOption == 0) {
				if (Input.GetButtonDown ("Submit")) {
					unpause ();
				}
			} else if (currentPauseMenuOption == 1) {
				if (Input.GetButtonDown ("Submit")) {
					GameMaster.PlayUnpauseSFX ();
					mainPause = false;
					controlsPause = true;
					controlsOption ();
				}
			} else if (currentPauseMenuOption == pauseMenuOptions.Length - 1) {
				if (Input.GetButtonDown ("Submit")) {
					Application.Quit();
					//unpause ();
				}
			}
		} else if(controlsPause) {
			if (currentPauseMenuOption == 0) {
				if (Input.GetButtonDown ("Submit")) {
					GameMaster.PlayUnpauseSFX ();
					GameMaster.ControllerCheck (true);
				}
			} else if (currentPauseMenuOption == 1) {
				if (Input.GetButtonDown ("Submit")) {
					GameMaster.PlayUnpauseSFX ();
					//using keyboard instead of controller
					GameMaster.ControllerCheck (false);
				}
			}
			if (Input.GetButtonDown("PS4_O")) {
				GameMaster.PlayUnpauseSFX();
				pauseTitle.sprite = pausedSprite;
				mainPause = true;
				controlsPause = false;
				//destroys the control menu's options and reinstantiates the main pause menu
				for(int i = 0; i < pauseMenuOptions.Length; i++) {
					Destroy(pauseMenuOptions[i].gameObject);
				}
				Destroy(leftCursor.gameObject);
				Destroy(rightCursor.gameObject);
				pauseMenu();
			}
		} else if(weaponPause) {
			
		}
	}

	private void controlsOption() {
		for (int i = 0; i < pauseMenuOptions.Length; i++) {
			Destroy(pauseMenuOptions[i].gameObject);
		}
		pauseTitle.sprite = controlsTitleSprite;
		pauseMenuOptions = new Transform[2];	//sets new size for transform[]

		pauseMenuOptions[0] = Instantiate(controller, currentMenu.transform).transform;
		pauseMenuOptions[1] = Instantiate(keyboard, currentMenu.transform).transform;

		currentPauseMenuOption = 0;
		optionRect = (RectTransform) pauseMenuOptions[0].transform;
		leftCursor.transform.position = new Vector2(optionRect.transform.position.x - (optionRect.rect.width) - 10,
			pauseMenuOptions[currentPauseMenuOption].transform.position.y);
		rightCursor.transform.position = new Vector2(optionRect.transform.position.x + (optionRect.rect.width) + 10,
			pauseMenuOptions[currentPauseMenuOption].transform.position.y);
	}
}

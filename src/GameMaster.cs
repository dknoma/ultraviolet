using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

/*
 * External class that will take care of many global actions for the game.
 * Such things include drestroying and respawning players.
 * 		Useful as destroying an object will destroy any script attached as well.
 * Will take care of instantiating objects.
 * Instantiates everything related to the HUD
 *		This includes coin count, ammo count, weapon index, healthbar, etc.
 * Also takes care of playing sound effects
 */
public class GameMaster : MonoBehaviour {

	public static GameMaster gm;

	public GameObject hud;
	public Camera cam;
	public Canvas canvas;
	public Transform healthTile;
	public Transform playerPrefab;
	public Transform spawnPoint;

	public Transform healthTileSpawn1;
	public Transform healthTileSpawn2;
	public Transform healthTileSpawn3;
	public Transform healthTileSpawn4;
	public Transform healthTileSpawn5;
	public Transform healthTileSpawn6;
	public Transform healthTileSpawn7;
	public Transform healthTileSpawn8;

	/* Sprites for each number */
	[SerializeField]
	public Sprite[] numbersSprites;
	
	/* Weapon sprites */
	public Sprite blaster;
	public Sprite sword;
	private static Sprite blasterSprite;
	private static Sprite swordSprite;

	/* power up sprites */
	public Sprite neutral;
	public Sprite joy;
	public Sprite anger;
	public Image joyGauge;
	public Image angerGauge;

	private static Sprite neutralSprite;
	private static Sprite joySprite;
	private static Sprite angerSprite;
	private static Image joyG;
	private static Image angerG;

	public int spawnDelay = 2;
	/* public health tile variables */
	public Image full;
	public Sprite fullTile;
	public Sprite halfTile;
	public Sprite emptyTile;
	/* public song variables */
	public bool isMadForest;
	public bool isStE;
	public bool flowersOfAntimony;

	/* private player health variables */
	private List<Transform> positions = new List<Transform>();
	private List<Image> healthTiles = new List<Image>();
	private Image fullHealth;
	private Player player;
	private float nextTimeToSearch = 0;

	private static float currentHealth;
	private static float maxHealth;
	private static int healthContainers = 8;

	private static GameObject spawningRoom;

	/* private audio variables */
	private AudioManager audioManager;
	private static AudioSource currentSong;
	private static AudioSource[] queuedSongs;
	private bool loop;

	/* private player state variables */
	private static bool isPlayerTransitioning;
	private static bool isPlayerRespawning;
	private static bool isPlayerInvulnerable;
	private static bool isPlayerHurting;
	private static bool shooter;
	private static bool upgradedBlaster;
	private static bool swordAttack;
	private static bool upgradedSword;
	private static bool swinging;
	private static bool onLadder;

	/* private settings variables */
	private static bool useController;

	/* Numbers */
	private static Sprite[] numbers;

	/* Gem counter stuff */
	private static int gemCount;
	private static int counterDigits;
	private static Image[] gemCounterDigits;
	//private static Transform[] gemCounterDigitPositions = new Transform[counterDigits];
	private static bool incrementing;
	private static bool swap;
	private static int tempNum;

	/* Weapon counter stuff */
	private static GameObject weaponCounter;
	private static int weaponIndex = 0;
	private static int ammoCount = 99;
	private static int ammoDigits;
	private static Image[] ammoCounterDigits;
	private static bool incrementingAmmo;
	private static string weapon;

	/* Facial expression stuff */
	private static GameObject upgradeCounter;
	private static float joyMeter;
	private static float angerMeter;
	private static bool joyPowerUp;
	private static bool angerPowerUp;

	private static bool biofeedback;

	void Awake() {
		FindPlayer();
		if (gm == null) {				//if gm is null, find the GameMaster object
			gm = this;
		}
		DontDestroyOnLoad(gm);

		useController = true;
		onLadder = false;

		healthTiles.Add(Instantiate(full, healthTileSpawn1) as Image);
		healthTiles.Add(Instantiate(full, healthTileSpawn2) as Image);
		healthTiles.Add(Instantiate(full, healthTileSpawn3) as Image);
		healthTiles.Add(Instantiate(full, healthTileSpawn4) as Image);
		healthTiles.Add(Instantiate(full, healthTileSpawn5) as Image);
		healthTiles.Add(Instantiate(full, healthTileSpawn6) as Image);
		healthTiles.Add(Instantiate(full, healthTileSpawn7) as Image);
		healthTiles.Add(Instantiate(full, healthTileSpawn8) as Image);

		maxHealth = healthContainers * 10;
		currentHealth = maxHealth;

		/* Instantiates all the necessary gem counter tiles */
		gemCount = 0;
		counterDigits = 4;
		gemCounterDigits = new Image[counterDigits];
		GameObject counter = GameObject.Find("Gem Counter");
		for(int i = 0; i < counterDigits; i++) {
			gemCounterDigits[i] = counter.transform.GetChild(i).GetComponent<Image>();
		}

		/* Puts number tiles into the array */
		numbers = new Sprite[numbersSprites.Length];
		for(int i = 0; i < numbersSprites.Length; i++) {
			numbers[i] = numbersSprites[i];
		}

		/* Gets the weapon counter */
		weaponCounter = GameObject.Find("Weapon Counter");
		blasterSprite = blaster;
		swordSprite = sword;

		/* Instantiates ammo counter tiles */
		GameObject ammoCounter = GameObject.Find("Ammo Counter");
		ammoDigits = 2;
		ammoCounterDigits = new Image[ammoDigits];
		for (int i = 0; i < ammoDigits; i++) {
			ammoCounterDigits[i] = ammoCounter.transform.GetChild(i).GetComponent<Image>();
		}

		/* upgrade counter stuff */
		upgradeCounter = GameObject.Find("Powerup_counter");
		neutralSprite = neutral;
		joySprite = joy;
		angerSprite = anger;
		joyG = joyGauge;
		joyG.fillAmount = 0;
		angerG =angerGauge;
		angerG.fillAmount = 0;

		upgradedSword = false;
		upgradedBlaster = false;

		audioManager = AudioManager.instance;
		if (audioManager == null) {
			Debug.LogError("No audio manager found.");
		}
		/* Pause menu and controller options */
		PauseMenu.isPaused = false;
		useController = false;
		swordAttack = true;
		shooter = false;
		foreach (string s in Input.GetJoystickNames()) {
			print("device name: " + s);
			if (s.Equals("Wireless Controller") || s.Equals("Sony Interactive Entertainment Wireless Controller")) {
				useController = true;
			}
		}
		print("controller? " + useController);
		ControllerInputManager.VerticalControl(useController);
		ControllerInputManager.HorizontalControl(useController);
		ControllerInputManager.JumpControl(useController);
		ControllerInputManager.GrabControl(useController);
		ControllerInputManager.FireControl(useController);
		ControllerInputManager.PauseControl(useController);
		ControllerInputManager.WeaponsControl(useController);
	}

	private void Start() {
		if (isMadForest) {
			currentSong = CurrentSong("MadForest");
			currentSong.Play ();

		} else if(isStE) {
			currentSong = CurrentSong("StrikeTheEarth");
			currentSong.Play ();

		} else if(flowersOfAntimony) {
			queuedSongs = new AudioSource[2];
			string[] titles = new string[queuedSongs.Length];
			titles[0] = "FlowersOfAntimonyIntro";
			titles[1] = "FlowersOfAntimony";

			for (int i = 0; i < titles.Length; i++) {
				queuedSongs[i] = CurrentSong (titles[i]);
			}

			queuedSongs[0].Play ();
			currentSong = queuedSongs[0];
			loop = false;
		}
	}

	void Update() {
		HealthBar();
		if(PauseMenu.isPaused) {
			findControls ();
		}
		joyG.fillAmount = joyMeter / 100;
		angerG.fillAmount = angerMeter / 100;
	}

	void FixedUpdate() {
		//play other song when the intro stops playing
		if (queuedSongs != null) {
			if (!queuedSongs[0].isPlaying && !loop) {
				queuedSongs[1].Play();
				currentSong = queuedSongs[1];
				loop = true;
			}
		}
	}

	/* Static methods */

	/*
	 * This method will kill the player object.
	 * After killing the player, will call the respawn coroutine.
	 */
	public static void KillPlayer(Player player) {
		PlayDeath();
		UpgradeBlaster(false);
		UpgradeSword(false);
		ResetJoy();
		ResetAnger();
		ResetUpgrade();
		Destroy(player.gameObject);				//destroys the player object, effectively killing the player
		PlayerIsRespawningCheck(true);
		gm.StartCoroutine(gm.RespawnPlayer());	//Will then call the respawn player coroutine.
	}

	/*************************
	 * Counter related methods
	 *************************/

	/****************************************************************************************
	 *																						*
	 *																						*
	 *										Gem_counter										*
	 *																						*
	 *																						*
	 ****************************************************************************************/
	/**
	 * Method that will increment the gem counter;
	 */
	public static void incGems(int count, MonoBehaviour instance) {
		incrementing = true;
		if (count < 10) {
			instance.StartCoroutine(updateGemCounter(count, 1, true, instance));
		} else if (count > 10 && count < 100) {
			int runs = count / 5;
			instance.StartCoroutine(updateGemCounter(5, runs, true, instance));
		} else {
			int runs = count / 10;
			instance.StartCoroutine(updateGemCounter(10, runs, true, instance));
		}
	}

	/**
	 * Method that will decrement the gem counter;
	 * This method removes gems faster
	 */
	public static void decGems(int count, MonoBehaviour instance) {
		incrementing = false;
		if (count < 10) {
			instance.StartCoroutine(updateGemCounter(count, 1, false, instance));
		} else if (count > 10 && count < 100) {
			int runs = count / 5;
			instance.StartCoroutine(updateGemCounter(5, runs, false, instance));
		} else if (count > 100 && count < 1000) {
			int runs = count / 10;
			instance.StartCoroutine(updateGemCounter(10, runs, false, instance));
		} else {
			int runs = count / 100;
			instance.StartCoroutine(updateGemCounter(100, runs, false, instance));
		}
	}

	public static int getGemCount() {
		return gemCount;
	}

	/**
	 * Updates the number on the gem counter.
	 * If incrementing or decrementing gets interrupted, will imediately stop incrementing
	 * and move on to the next invokation.
	 * 
	 */
	public static IEnumerator updateGemCounter(int count, int runs, bool inc, MonoBehaviour instance) {
		for (int index = 0; index < runs; index++) {
			int originalCount = gemCount;
			//if incrementing, start are 1s index, else start at 1000s index
			if (inc) {
				gemCount = originalCount + count;
				if (gemCount > 9999) {
					gemCount = 9999;
				}
				for (int j = 0, place = 1; j < counterDigits; j++, place *= 10) {
					int digit = (gemCount / place) % 10;
					int i = (originalCount / place) % 10;
					//if incrementing by a bigger count, skip some animation where applicable
					if (i == digit) {
						for (int k = 0; k < 10; k++) {
							i = Mathf.Abs((i + 1) % 10);
							gemCounterDigits[j].sprite = numbers[i % 10];
						}
					} else {
						while (i != digit) {
							if (!incrementing) {
								gemCounterDigits[j].sprite = numbers[digit % 10];
								break;
							}

							i = Mathf.Abs((i + 1) % 10);
							gemCounterDigits[j].sprite = numbers[i % 10];
							yield return new WaitForSeconds(0);

						}
					}
					if (!incrementing) {
						break;
					}
				}
				if (!incrementing) {
					break;
				}
			} else {
				gemCount = originalCount - count;
				if (gemCount < 0) {
					gemCount = 0;
				}
				for (int j = counterDigits - 1, place = 1000; j >= 0; j--, place /= 10) {
					int digit = (gemCount / place) % 10;
					int i = (originalCount / place) % 10;

					while (i != digit) {
						if (incrementing) {
							gemCounterDigits[j].sprite = numbers[digit % 10];
							break;
						}
						i = (i + 9) % 10;
						gemCounterDigits[j].sprite = numbers[i % 10];
						yield return new WaitForSeconds(0.001f);
					}
					if (incrementing) {
						break;
					}
				}
				if (incrementing) {
					break;
				}
			}
		}
	}

	/****************************************************************************************
	 *																						*
	 *																						*
	 *										Ammo_counter									*
	 *																						*
	 *																						*
	 ****************************************************************************************/

	/**
	 * Method that will increment the gem counter;
	 */
	public static void incAmmo(int count, MonoBehaviour instance) {
		incrementingAmmo = true;
		if (count < 10) {
			instance.StartCoroutine(updateAmmoCounter(count, 1, true, instance));
		} else {
			int runs = count / 5;
			instance.StartCoroutine(updateAmmoCounter(5, runs, true, instance));
		}
	}

	/**
	 * Method that will decrement the gem counter;
	 * This method removes gems faster
	 */
	public static void decAmmo(int count, MonoBehaviour instance) {
		incrementingAmmo = false;
		if (count < 10) {
			instance.StartCoroutine(updateAmmoCounter(count, 1, false, instance));
		} else {
			int runs = count / 5;
			instance.StartCoroutine(updateAmmoCounter(5, runs, false, instance));
		}
	}

	/**
	 * Updates the number on the gem counter.
	 * If incrementing or decrementing gets interrupted, will imediately stop incrementing
	 * and move on to the next invokation.
	 * 
	 */
	public static IEnumerator updateAmmoCounter(int count, int runs, bool inc, MonoBehaviour instance) {
		for (int index = 0; index < runs; index++) {
			int originalCount = ammoCount;
			//if incrementing, start are 1s index, else start at 10s index
			if (inc) {
				ammoCount = originalCount + count;
				if (ammoCount > 99) {
					ammoCount = 99;
				}
				for (int j = 0, place = 1; j < ammoDigits; j++, place *= 10) {
					int digit = (ammoCount / place) % 10;
					int i = (originalCount / place) % 10;
					//if incrementing by a bigger count, skip some animation where applicable
					if (i == digit) {
						for (int k = 0; k < 10; k++) {
							i = Mathf.Abs((i + 1) % 10);
							ammoCounterDigits[j].sprite = numbers[i % 10];
						}
					} else {
						while (i != digit) {
							if (!incrementingAmmo) {
								ammoCounterDigits[j].sprite = numbers[digit % 10];
								break;
							}

							i = Mathf.Abs((i + 1) % 10);
							ammoCounterDigits[j].sprite = numbers[i % 10];
							yield return new WaitForSeconds(0.02f);

						}
					}
					if (!incrementingAmmo) {
						break;
					}
				}
				if (!incrementingAmmo) {
					break;
				}
			} else {
				ammoCount = originalCount - count;
				if (ammoCount < 0) {
					ammoCount = 0;
				}
				for (int j = ammoDigits - 1, place = 10; j >= 0; j--, place /= 10) {
					int digit = (ammoCount / place) % 10;
					int i = (originalCount / place) % 10;

					while (i != digit) {
						if (incrementingAmmo) {
							ammoCounterDigits[j].sprite = numbers[digit % 10];
							break;
						}
						i = (i + 9) % 10;
						ammoCounterDigits[j].sprite = numbers[i % 10];
						yield return new WaitForSeconds(0.02f);
					}
					if (incrementingAmmo) {
						break;
					}
				}
				if (incrementingAmmo) {
					break;
				}
			}
		}
	}

	/****************************************************************************************
	 *																						*
	 *																						*
	 *									Player_state_list									*
	 *																						*
	 *																						*
	 ****************************************************************************************/
	/*------------------------------------------------------------------
	 * These are meant to be static	as these settings will be accessed *
	 * by many different scripts									   *
	 *-----------------------------------------------------------------*/

	/* 
	 * Current health 
	 */
	public static void CurrentPlayerHealthUpdate(float currHealth) {
		currentHealth = currHealth;
	}

	public static float CurrentPlayerHealth() {
		return currentHealth;
	}

	/* 
	 * Max player health 
	 */
	public static void MaxPlayerHealthUpdate(float update) {
		maxHealth = update;
	}

	public static float MaxPlayerHealth() {
		return maxHealth;
	}

	/*
	 * Health container
	 */ 
	public static void HealthContainersUpdate(int count) {
		healthContainers = count;
	}

	public static int HealthContainers() {
		return healthContainers;
	}

	/*
	 * Method to check if player is transitioning. Only meant for the player to change.
	 */
	public static void PlayerTransitionStateCheck(bool boolean) {
		isPlayerTransitioning = boolean;
	}

	/*
	 * Returns the player's transitioning state. Useful for when rooms need to be reset, etc.
	 */
	public static bool PlayerTransitionState() {
		return isPlayerTransitioning;
	}

	/*
	 * Checks if the player is respawning
	 */
	public static void PlayerIsRespawningCheck(bool isRespawning) {
		isPlayerRespawning = isRespawning;
	}

	public static bool PlayerRespawnState() {
		return isPlayerRespawning;
	}

	/*
	 * Checks if the player is invulnerable
	 */ 
	public static void PlayerIsInvulnerableCheck(bool isInvulnerable) {
		isPlayerInvulnerable = isInvulnerable;
	}

	public static bool PlayerIsInvulnerable() {
		return isPlayerInvulnerable;
	}

	/*
	 * Checks if the player is being knocked back
	 */ 

	public static void PlayerIsKnockedBackCheck(bool isKnockedBack) {
		isPlayerHurting = isKnockedBack;
	}

	public static bool PlayerIsKnockedBack() {
		return isPlayerHurting;
	}

	/*
	 * Checks if the player paused the game
	 */ 

	public static void PauseCheck(bool isPaused) {
		PauseMenu.isPaused = isPaused;
	}

	public static bool IsPaused() {
		return PauseMenu.isPaused;
	}

	/*
	 * Checks if the current menu is the weapon select menu
	 */ 

	public static void WeaponMenuCheck(bool isWeaponMenu) {
		PauseMenu.isWeaponMenu = isWeaponMenu;
	}

	public static bool WeaponMenu() {
		return PauseMenu.isWeaponMenu;
	}

	public static void ControllerCheck(bool usingController) {
		useController = usingController;
	}

	public static bool UsingController() {
		return useController;
	}

	public static void LadderCheck(bool climbing) {
		onLadder = climbing;
	}

	public static bool OnLadder() {
		return onLadder;
	}

	/* Checks if the player is using their sword */

	public static void CheckSword(bool usingSword) {
		if(usingSword == true) {
			weaponCounter.GetComponent<Image>().sprite = swordSprite;
			weapon = "sword";
		}
		swordAttack = usingSword;
	}

	public static bool SwordAttack() {
		return swordAttack;
	}

	public static void UpgradeSword(bool upgrade) {
		upgradedSword = upgrade;
	}

	public static bool UpgradedSword() {
		return upgradedSword;
	}

	public static void CheckSwingingSword(bool swing) {
		swinging = swing;
	}

	public static bool Swinging() {
		return swinging;
	}

	/* Checks if weapon is a shooter */

	public static void CheckShooter(bool shooting) {
		if (shooting == true) {
			weaponCounter.GetComponent<Image>().sprite = blasterSprite;
			weapon = "blaster";
		}
		shooter = shooting;
	}

	public static bool Shooter() {
		return shooter;
	}

	public static void UpgradeBlaster(bool upgrade) {
		upgradedBlaster = upgrade;
	}

	public static bool UpgradedBlaster() {
		return upgradedBlaster;
	}

	/* Checks weapon index and ammo */
	public static void ChangeWeaponIndex(int next) {
		weaponIndex = next;
	}

	public static int WeaponIndex() {
		return weaponIndex;
	}

	public static void UseAmmo(int count) {
		ammoCount -= count;
	}

	public static void GainAmmo(int count) {
		ammoCount += count;
	}

	public static int Ammo() {
		return ammoCount;
	}

	public static void SetWeapon(string newWeapon) {
		weapon = newWeapon;
	}

	public static string Weapon() {
		return weapon;
	}

	/* affectiva */
	public static void AddToJoy(float amount) {
		joyMeter += amount;
		if(joyMeter >= 100) {
			//print("joy powerup ready.");
			joyMeter = 100;
			joyPowerUp = true;
		} else {
			joyPowerUp = false;
		}
		//print("joy meter = " + joyMeter);
	}

	public static void ResetJoy() {
		joyMeter = 0;
		joyPowerUp = false;
	}

	public static bool CanUseJoy() {
		return joyPowerUp;
	}

	public static float Joy() {
		return joyMeter;
	}

	public static void AddToAnger(float amount) {
		angerMeter += amount;
		if (angerMeter >= 100) {
			//print("anger powerup ready.");
			angerMeter = 100;
			angerPowerUp = true;
		} else {
			angerPowerUp = false;
		}
		//print("anger meter = " + angerMeter);
	}

	public static void ResetAnger() {
		angerMeter = 0;
		angerPowerUp = false;
	}

	public static bool CanUseAnger() {
		return angerPowerUp;
	}

	public static float Anger() {
		return angerMeter;
	}

	public static void UpgradeJoy() {
		upgradeCounter.GetComponent<Image>().sprite = joySprite;
		UpgradeBlaster(true);
		UpgradeSword(true);
	}

	public static void UpgradeAnger() {
		upgradeCounter.GetComponent<Image>().sprite = angerSprite;
	}

	public static void ResetUpgrade() {
		upgradeCounter.GetComponent<Image>().sprite = neutralSprite;
	}

	/* Biofeedback check */

	public static void UsingBiofeedback(bool bio) {
		biofeedback = bio;
	}

	public static bool Biofeedback() {
		return biofeedback;
	}

	/* Room to respawn in */
	public static void SetSpawningRoom(GameObject spawnRoom) {
		spawningRoom = spawnRoom;
	}

	public static GameObject SpawningRoom() {
		return spawningRoom;
	}

	/****************************************************************************************
	 *																						*
	 *																						*
	 *									Sound_effect_list									*
	 *																						*
	 *																						*
	 ****************************************************************************************/

	public static void PlayJump() { AudioManager.instance.PlaySound("Jump"); }

	public static void PlayCheckpoint() { AudioManager.instance.PlaySound("Checkpoint"); }

	public static void PlayHeal() { AudioManager.instance.PlaySound("Heal"); }

	public static void PlayDeath() { AudioManager.instance.PlaySound("Death"); }

	public static void PlayHurt() { AudioManager.instance.PlaySound("Hurt"); }

	public static void PlayShoot() { AudioManager.instance.PlaySound("Shoot"); }

	public static void PlayShooterSFX() { AudioManager.instance.PlaySound("ShooterSFX");}

	public static void PlayPauseSFX() { AudioManager.instance.PlaySound("MegamanPause");}

	public static void PlayUnpauseSFX() { AudioManager.instance.PlaySound("Unpause");}

	public static void SelectSFX() { AudioManager.instance.PlaySound("Select");}

	public static void GateOpen() { AudioManager.instance.PlaySound("GateOpen");}

	public static void TileExplosionSFX() { AudioManager.instance.PlaySound("Explosion");}

	public static void SwordSFX() { AudioManager.instance.PlaySound ("Sword");}

	public static void CoinSFX() { AudioManager.instance.PlaySound("Coin");}

	public static void GemSFX() { AudioManager.instance.PlaySound("Gem");}

	public static void UShotSFX() { AudioManager.instance.PlaySound("UpgradedShot");}

	public static void CancelSFX() { AudioManager.instance.PlaySound("Cancel");}

	public static void UpgradeSFX() { AudioManager.instance.PlaySound("ChooseUpgrade");}

	public static void EndUpgradeSFX() { AudioManager.instance.PlaySound("EndUpgrade");}

	public static void SwordBeamSFX() { AudioManager.instance.PlaySound("SwordBeam");}

	public static void EnemyDeathSFX() { AudioManager.instance.PlaySound("EnemyDeath");}

	public static void EnemyHurtSFX() { AudioManager.instance.PlaySound("EnemyHurt"); }

	/****************************************************************************************
	 *																						*
	 *																						*
	 *										Music_list										*
	 *																						*
	 *																						*
	 ****************************************************************************************/

	public static AudioSource CurrentSong(string songName) {
		AudioSource currentSong = AudioManager.instance.getCurrentSong(songName);
		return currentSong;
	}

	public static AudioSource getCurrentSong() {
		return currentSong;
	}

	/****************************************************************************************
	 *																						*
	 *																						*
	 *										Health_bar										*
	 *																						*
	 *																						*
	 ****************************************************************************************/

	/**
	 * Method to instantiate a new healthbar for the current player object.
	 * Will update the tiles on the UI depending on the player's health values.
	 * Takes care of all cases of healing and damage.
	 * 
	 * Note: make sure to put the in-game health tiles or else they will not
	 * 		 update correctly.
	 */
	private void HealthBar() {
		FindPlayer();

		int index = (int) Mathf.Ceil(currentHealth / 10.0f % 10) - 1;	//gets the index of the current healthbar tile
		if (index <= 0) {
			index = 0;													//makes sure index doesn't go below 0
		}

		if (currentHealth == maxHealth) {								//create a full healthbar
			for (int i = 0; i < healthTiles.Count; i++) {
				healthTiles[i].sprite = fullTile;
			}
		} else if (currentHealth <= 0 || player == null) {				//empty the healthbar if health == 0 or player is
			for (int i = 0; i < healthTiles.Count; i++) {
				healthTiles[i].sprite = emptyTile;
			}
		} else if (currentHealth != maxHealth && currentHealth % 10 == 0 && player != null) {
																		//if taking 10+ damage, makes all previous(right) tiles
			if (index + 1 < healthTiles.Count) {						//	empty tiles, and the current tile full or half
				for (int i = healthTiles.Count - 1; i > index; i--) {	//	depending on the player's current health.
					healthTiles[i].sprite = emptyTile;
				}
				for (int i = 0; i <= index; i++) {
					healthTiles[i].sprite = fullTile;					//makes all full indices full tiles (takes care of healing damage)
				}
			}
		} else if (currentHealth != maxHealth && currentHealth % 10 != 0) {
			if (index + 1 < healthTiles.Count) {
				for (int i = healthTiles.Count - 1; i > index; i--) {
					healthTiles[i].sprite = emptyTile;
				}
				healthTiles[index].sprite = halfTile;
				for (int i = 0; i < index; i++) {
					healthTiles[i].sprite = fullTile;					//makes previous indexes full tiles (takes care of healing damage)
				}
			} else {
				healthTiles[index].sprite = halfTile;
				for (int i = 0; i < index; i++) {
					healthTiles[i].sprite = fullTile;					//makes previous indexes full tiles (takes care of healing damage)
				}
			}
		}
	}

	/****************************************************************************************
	 *																						*
	 *																						*
	 *									Coroutine_list										*
	 *																						*
	 *																						*
	 ****************************************************************************************/

	/*
	 * A coroutine that will respawn the player after it dies.
	 * Will wait a few seconds before respawning the player.
	 * 		This value can be changed in the editor.
	 * After instantiating a new player object, it can play a
	 * respawn animation as well.
	 */
	public IEnumerator RespawnPlayer() {
		Debug.Log("You alive.");
		Debug.Log("TODO: spawn sound?");
		PlayerIsRespawningCheck(true);
		yield return new WaitForSeconds(spawnDelay);                            //yield execution of function. will resume after spawnDelay seconds

		PlayerIsRespawningCheck(true);
		currentHealth = maxHealth;
		Instantiate(playerPrefab, spawnPoint.position, spawnPoint.rotation);    //will instantiate a new Player object at the specified position
		Debug.Log("TODO: spawn animation?");
	}

	/*
	 * Lets the game know when the player is done respawning
	 */ 
	private IEnumerator doneSpawning() {
		yield return new WaitForSeconds(3);
		GameMaster.PlayerIsRespawningCheck(false);
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
	/****************************************************************************************
	 *																						*
	 *																						*
	 *									Additional_methods									*
	 *																						*
	 *																						*
	 ****************************************************************************************/

	void findControls() {
		ControllerInputManager.VerticalControl (useController);
		ControllerInputManager.HorizontalControl (useController);
		ControllerInputManager.JumpControl (useController);
		ControllerInputManager.GrabControl (useController);
		ControllerInputManager.FireControl (useController);
		ControllerInputManager.PauseControl (useController);
		ControllerInputManager.WeaponsControl (useController);
	}

	private class PauseMenu : MonoBehaviour {
		public static bool isPaused;
		public static bool isWeaponMenu;
	}
}

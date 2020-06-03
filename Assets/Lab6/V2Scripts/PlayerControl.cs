using UnityEngine;
using System;
using System.Collections;
using System.Collections.Generic;

namespace Lab6 {

	// This code is mostly code I had written for a game I had worked on.
	// It is heavily modified to strip out things that I did not want to port to this example.
	/// <summary> Player control code. </summary>
	public class PlayerControl : MonoBehaviour {
		/// <summary> Player's mobility ability numbers </summary>
		[Serializable] public class Settings {
			/// <summary> Walk speed </summary>
			public float speed = 11f;
			/// <summary> Swim speed. </summary> // Unused for this example.
			public float waterSpeed = 6f;
			/// <summary> Sprint speed multiplier. </summary>
			public float sprintBoost = 1.5f;
			/// <summary> Crouch speed multiplier </summary>
			public float crouchPenalty = .5f;
			/// <summary> Acceleration </summary>
			public float accel = 5f;
			/// <summary> Control in the air . </summary>
			public float airControl = 1f;
			/// <summary> Acceleration in the water. </summary> // Unused for this example 
			public float waterAccel = 3f;

			/// <summary> Sensitivity to stick/mouse movements </summary>
			public Vector2 sensitivity = 3 * Vector2.one;
			/// <summary> Camera offset from center of controller </summary>
			public Vector3 cameraOffset = .5f * Vector3.up;

			/// <summary> Power of a jump </summary>
			public float jumpForce = 11f;
			/// <summary> Force of gravity </summary>
			public float gravity = 15f;
			/// <summary> Maximum downward speed </summary>
			public float terminalVelocity = 30f;
			/// <summary> Distance to stick to ground when moving quickly </summary>
			public float snapDistance = .2f;
			/// <summary> Speed to sink when in water </summary> // Unused for this example
			public float waterSinking = 60f;
		}
		/// <summary> Current state of the player. </summary>
		[Serializable] public class State {
			/// <summary> Look yaw from identity </summary>
			public float yaw = 0;
			/// <summary> Look pitch from identity </summary>
			public float pitch = 0;
			/// <summary> Current physics velocity </summary>
			public Vector3 physicsVelocity = Vector3.zero;
			/// <summary> Current self movement velocity </summary>
			public Vector3 moveVelocity = Vector3.zero;
		}

		// We used the script for enemies and networked players...
		// This flag was only set to true for the player being controlled by the user...
		public bool IS_PLAYER = false;

		/// <summary> Source of bullets </summary>
		public Transform lookAnchor;

		/// <summary> Mainhand weapon </summary>
		public AWeapon weapon;
		/// <summary> Offhand weapon </summary>
		public AWeapon altWeapon;
		/// <summary> Viewmodel for mainhand weapon </summary>
		public ViewModel weaponViewModel;
		/// <summary> Viewmodel for offhand weapon </summary>
		public ViewModel altWeaponViewModel;

		/// <summary> Self hitbox </summary>
		public CapsuleCollider hitbox;
		/// <summary> Movement controller </summary>
		public CharacterController controller;
		/// <summary> Ammo inventory </summary>
		public Dictionary<string, float> ammoBox;

		/// <summary> Initial mainhand weapon </summary>
		public AWeapon initialWeaponPrefab;
		/// <summary> Initial offhand weapon </summary>
		public AWeapon initialAltWeaponPrefab;

		/// <summary> Current settings </summary>
		public Settings settings;
		/// <summary> Current state </summary>
		public State state;

		/// <summary> Spring to modify positions of things when in air </summary>
		public Spring airSpring;
		/// <summary> Spring to modify positions of things when sprinting </summary>
		public Spring sprintSpring;
		/// <summary> Spring to modify positions of things when reloading </summary>
		public Spring reloadSpring;
		/// <summary> Spring to modify positions of things when disabled </summary>
		public Spring disableWeaponSpring;

		/// <summary> How quickly crouch input is responded to </summary>
		public float crouchResponse = 4f;

		// Inputs- these were overridden by AI, so that an AI bot script could drive our controller.
		// By default, these are all set to dummy inputs that never do anything 
		/// <summary> Input for jump press </summary>
		public Func<bool> jumpInput = () => { return false; };
		/// <summary> Input for jump held </summary>
		public Func<bool> jumpInputHeld = () => { return false; };
		/// <summary> Input for trigger held </summary>
		public Func<bool> shootInput = () => { return false; };
		/// <summary> Input for grenade held </summary>
		public Func<bool> grenadeInput = () => { return false; };
		/// <summary> Input for use key press </summary>
		public Func<bool> useInput = () => { return false; };
		/// <summary> Input for use key held</summary>
		public Func<bool> useInputHeld = () => { return false; };
		/// <summary> Input for sprint held</summary>
		public Func<bool> sprintInput = () => { return false; };
		/// <summary> Input for reload key press </summary>
		public Func<bool> reloadInput  = () => { return false; };
		/// <summary> Input for crouch key press </summary>
		public Func<bool> crouchInput = () => { return false; };
		/// <summary> Input for crouch key held </summary>
		public Func<bool> crouchInputHeld = () => { return false; };

		/// <summary> Input for altfire key </summary>
		public Func<bool> altFireInput = () => { return false; };
		/// <summary> Input for altfire reset </summary>
		public Func<bool> altFireReset = () => { return false; };

		/// <summary> Input for left stick </summary>
		public Func<Vector2> leftStickInput = () => { return Vector2.zero; };
		/// <summary> Input for right stick </summary>
		public Func<Vector2> rightStickInput= () => { return Vector2.zero; };
		/// <summary> Input for wasd </summary>
		public Func<Vector2> keyboardInput = () => { return Vector2.zero; };
		/// <summary> Input for mouse look </summary>
		public Func<Vector2> mouseInput = () => { return Vector2.zero; };

		/// <summary> Have initial weapons been given? </summary>
		bool spawnedInitialWeapon = false;
		/// <summary> last frame's mainhand weapon </summary>
		string lastWeapon;
		/// <summary> last frame's offhand weapon </summary>
		string lastAltWeapon;

		/// <summary> Given to any equipped weapon to tell it where to shoot from </summary>
		Transform[] barrels;
		
		/// <summary> Last frame's input </summary>
		Vector3 input;
		/// <summary> Currently sprinting ?</summary>
		bool sprint;
		/// <summary> Last time the trigger was pulled. </summary>
		float lastFireTime;
		/// <summary> Current crouch value </summary>
		float crouchAmount = 0;

		/// <summary> Currently considered crouching? </summary>
		bool crouch { get { return crouchAmount > .5f; } }

		/// <summary> Initial center of character controller </summary>
		private Vector3 initialControllerCenter;
		/// <summary> Initial height of character controller </summary>
		private float initialControllerHeight;
		/// <summary> Initial camera's offset </summary>
		private Vector3 initialCameraOffset;

		/// <summary> Timer to disable weapons (for example, when drawing weapons, or picking up physics objects ) </summary>
		[NonSerialized] public float weaponDisableTimer = 0;
		/// <summary> weapon disabled? </summary>
		public bool weaponDisabled { get { return weaponDisableTimer > 0; } }
		/// <summary> Used to animate disabling of weapons </summary>
		private float disableWeaponPosition = 0;

		/* Removed stuff that would add a lot more complexity...
		/// <summary> current inventory </summary>
		public ValueMap inventoryData;
		/// <summary> checkpoint inventory </summary>
		private ValueMap _inventoryAtLevelStart;
		/// <summary> Initial hitbox center </summary>
		private Vector3 initialHitboxCenter;
		/// <summary> Initial hitbox height</summary>
		private float initialHitboxHeight;
		/// <summary> Initial hitbox radius </summary>
		private float initialHitboxRadius;

		/// <summary> Ladder currently being used </summary> 
		Ladder ladder;
		/// <summary> Use-target over crosshair or nearby </summary>
		public UseTrigger useTrigger = null;
		/// <summary> Layermask for use triggers. </summary>
		private readonly int useTriggerMask = 1 << 26 | 1 << 29;
		/// <summary> Distance to be able to use objects </summary>
		public float useDistance = 2;
		private UseTrigger _currentlyUsing;
		private UseTrigger _lastUsable;
		
		/// <summary> Underwater? </summary>
		public bool underWater = false;
		
		/// <summary> Currently held physics object </summary>
		public UsablePhysicsObject holding = null;
		/// <summary> How far to place object when dropped </summary>
		public float dropDistance = 2.5f;
		/// <summary> Maximum mass to be holdable by this unit </summary>
		public float maxPickUpMass = 10.0f;
		*/ //


		void Awake() {
			controller = transform.GetComponent<CharacterController>();
			if (controller == null) { controller = gameObject.AddComponent<CharacterController>(); }
			barrels = new Transform[1];

			// If we are actually the player...
			if (IS_PLAYER) {
				// Take control of the main camera
				lookAnchor = Camera.main.transform;

				// Assign input methods (otherwise driven by AI script)
				// (We had a custom control rebinding system
				// it has been gutted, and replaced with `Input` methods.)
				jumpInput = () => Input.GetKeyDown(KeyCode.Space);
				jumpInputHeld = () => Input.GetKey(KeyCode.Space);
				shootInput = () => Input.GetMouseButton(0);
				grenadeInput = () => Input.GetMouseButton(1);
				altFireReset = () => Input.GetMouseButtonUp(2);
				altFireInput = () => Input.GetMouseButtonDown(2);
				useInput = () => Input.GetKeyDown(KeyCode.E);
				useInputHeld = () => Input.GetKeyDown(KeyCode.E);
				sprintInput = () => Input.GetKey(KeyCode.LeftShift);
				reloadInput = () => Input.GetKeyDown(KeyCode.R);
				crouchInputHeld = () => Input.GetKey(KeyCode.C);
				leftStickInput = () => { return Vector2.zero; };
				rightStickInput = () => { return Vector2.zero; };

				// Take some measurements for crouching
				initialControllerCenter = controller.center;
				initialControllerHeight = controller.height;
				initialCameraOffset = settings.cameraOffset;
				// Self hitbox disabled...
				// hitbox = GetComponentInChildren<Hitbox>().GetComponent<CapsuleCollider>();
				//initialHitboxCenter = hitbox.center;
				//initialHitboxHeight = hitbox.height;
				//initialHitboxRadius = hitbox.radius;

				keyboardInput = () => new Vector2(
					(Input.GetKey(KeyCode.D) ? 1 : 0) + (Input.GetKey(KeyCode.A) ? -1 : 0),
					(Input.GetKey(KeyCode.W) ? 1 : 0) + (Input.GetKey(KeyCode.S) ? -1 : 0)
				);
				mouseInput = () => { return new Vector2(Input.GetAxis("Mouse X"), Input.GetAxis("Mouse Y")); };
			}

			ammoBox = new Dictionary<string, float>();
			// Ammo for demo....
			ammoBox["energy"] = 100;

		}
	
		void Start() {
		
		}

		void Update() {
			// This was changed by other things (eg, taking control of something remotely)
			barrels[0] = lookAnchor;
			if (!spawnedInitialWeapon) {
				SpawnInitialWeapons();
				// Bootstrap(inventoryData);
			}

			// Inventory removed to simplify.
			// UpdateInventory(inventoryData);

			// if (ladder != null) {
			//	UpdateLadderMovement();
			// } else if (underWater) {
			//	UpdateUnderwaterGravity();
			//	UpdateUnderwaterMovement();
			// } else {
			UpdateCrouch();
			UpdateGravity();
			UpdateMovement();
			// }
			// if (holding != null) {
			//	weaponDisableTimer = .25f;
			// }
			UpdateOrientation();
			UpdateWeapons();
#if UNITY_EDITOR
			// Some testing code for editor-only use.
			if (Input.GetKeyDown("g")) { weaponDisableTimer = .53f; }
			if (Input.GetKeyDown("h")) { weaponDisableTimer = 0f; }
			UpdateCursorLock();
#endif
			UpdateViewmodel();
			// if (holding != null) {
			//	UpdateHolding();
			// } else {
			//	UpdateUseTriggerUI();
			// }
		}

		/// <summary> Update cursor lock state</summary>
		private void UpdateCursorLock() {
			if (IS_PLAYER && Input.GetMouseButtonDown(0)) {
				Cursor.lockState = CursorLockMode.Locked;
				Cursor.visible = false;

			}

			if (IS_PLAYER && Input.GetKeyDown(KeyCode.LeftControl)) {
				if (Cursor.lockState == CursorLockMode.Locked) {
					Cursor.lockState = CursorLockMode.None;
					Cursor.visible = true;
				} else {
					Cursor.lockState = CursorLockMode.Locked;
					Cursor.visible = false;
				}
			}
		}


		/// <summary> Spawn initial weapons </summary>
		private void SpawnInitialWeapons() {
			SpawnWeapon(initialWeaponPrefab, ref weapon, ref weaponViewModel);
			// We had dual-wielding. Removed for this example.
			// SpawnWeapon(initialAltWeaponPrefab, ref altWeapon, ref altWeaponViewModel);

			spawnedInitialWeapon = true;
		}

		/// <summary> Spawn a weapon and assign it to be the current weapon </summary>
		/// <param name="prefab"> New weapon to use </param>
		/// <param name="weapon"> weapon reference to update </param>
		/// <param name="viewModel"> viewmodel reference to update </param>
		private void SpawnWeapon(AWeapon prefab, ref AWeapon weapon, ref ViewModel viewModel) {
			if (weapon != null) { Destroy(weapon.gameObject); }
			if (viewModel != null) { Destroy(viewModel.gameObject); }

			if (prefab != null) {
				weapon = Instantiate(prefab);
				weapon.owner = transform;
				weapon.ammoBox = ammoBox;

				if (IS_PLAYER) {
					ViewModel vm = weapon.GetComponent<ViewModel>();
					if (vm != null) { viewModel = vm; vm.owner = lookAnchor; }
				}
			}
		}

		/// <summary> Logic to update weapons every frame </summary>
		private void UpdateWeapons() {
			// Update timers
			lastFireTime += Time.deltaTime;
			weaponDisableTimer -= Time.deltaTime;
			if (weaponDisableTimer < 0) { weaponDisableTimer = 0; }

			// if (holding != null) {
			//	return;
			// }

			// If we have a mainhand weapon
			if (weapon != null) {
				if (weaponDisabled) { 
					// force trigger off
					// and disallow any behaviour if disabled
					weapon.trigger = false; 
				} else {
					if (weapon is ProjectileWeapon) { 
						// If projectile weapon, update barrels for first person
						((ProjectileWeapon)weapon).barrels = barrels;
					} 
					// (other weapon types removed)
					// Reload if requested
					if (reloadInput()) {
						weapon.StartReload();
					}
					// Set trigger to shootInput function
					weapon.trigger = shootInput();
					// Broadcast messages for alt fire inputs (does nothing here)
					if (altFireInput()) { weapon.BroadcastMessage("AltFireDown", SendMessageOptions.DontRequireReceiver); }
					if (altFireReset()) { weapon.BroadcastMessage("AltFireUp", SendMessageOptions.DontRequireReceiver); }
				}

			}
			// If we have an offhand weapon 
			if (altWeapon != null) {
				if (weaponDisabled) {
					// force trigger off
					// and disallow any behaviour if disabled
					altWeapon.trigger = false;
				} else {
					if (altWeapon is ProjectileWeapon) {
						// If projectile weapon, update barrels for first person
						((ProjectileWeapon)altWeapon).barrels = barrels;
					}
					// (other weapon types removed)
					// Set trigger to grenadeInput function
					altWeapon.trigger = grenadeInput();

				}
			}
		}

		/// <summary> Logic to update attached viewmodels every frame </summary>
		private void UpdateViewmodel() {

			if (weaponViewModel != null) {

				// Calculate bob target
				float bobTarget = ((input.magnitude > 0) ? 1f : 0f);
				if (crouch) { bobTarget *= .5f; }
				// Move bob towards target
				var bob = Mathf.Lerp(weaponViewModel.bob, bobTarget, Time.deltaTime * 5f);
				// Calculate sprint target
				float sprintTarget = (controller.isGrounded ? (sprint ? 1 : 0) : 0);
				if (lastFireTime < .2f) {
					sprintTarget = 0;
				}

				// Update air spring
				airSpring.target = ((!controller.isGrounded) ? 1f : 0f);
				var airVal = airSpring.Animate(weaponViewModel.air);

				// Update sprint spring
				sprintSpring.target = sprintTarget;
				var sprintVal = sprintSpring.Animate(weaponViewModel.sprint);

				// Update reload spring
				reloadSpring.target = weapon.reloading ? 1 : 0;
				var reloadVal = reloadSpring.Animate(weaponViewModel.reload);

				// Update disable spring
				disableWeaponSpring.target = weaponDisableTimer > 0 ? 1 : 0;
				// Weapon is disabled when sprinting, but we don't want both anims to overlap.
				if (sprint) { disableWeaponSpring.target = 0; }
				var disablePos = disableWeaponSpring.Animate(disableWeaponPosition);
				disableWeaponPosition = disablePos;
				 
				// Update all values in mainhand viewmodel
				weaponViewModel.bob = bob;
				weaponViewModel.bobSpeed = 1.0f;
				weaponViewModel.crouch = crouch ? 1 : 0;
				weaponViewModel.air = airVal;
				weaponViewModel.reload = reloadVal;
				weaponViewModel.disabled = disablePos;
				weaponViewModel.sprint = sprintVal;

				// Update values in offhand viewmodel
				if (altWeaponViewModel != null) {
					altWeaponViewModel.bob = bob;
					altWeaponViewModel.bobSpeed = 1.0f;
					altWeaponViewModel.crouch = crouch ? 1 : 0;
					altWeaponViewModel.air = airVal;
					altWeaponViewModel.disabled = disablePos;
					altWeaponViewModel.sprint = sprintVal;
				}

			}

		}
		/// <summary> Logic to update gravity every frame </summary>
		private void UpdateGravity() {

			// If we bump our head, start moving down
			if (state.physicsVelocity.y > 0 && (controller.collisionFlags & CollisionFlags.Above) != 0) {
				state.physicsVelocity.y = 0;
			}
			// If we're grounded
			if (controller.isGrounded) {

				// We can jump
				if (jumpInput()) {
					state.physicsVelocity.y = settings.jumpForce;
				} else {
					// Otherwise, stick to ground with a small negative y velocity (snap distance)
					// we need to increase snap distance when sprinting, to stick to ground at the same angle when moving faster.
					state.physicsVelocity.y = -Mathf.Abs(settings.snapDistance * Mathf.Max(1, settings.speed * (sprint ? settings.sprintBoost : 1f)));
				}

			} else {
				// Otherwise just update velocity if in the air.
				state.physicsVelocity.y -= Time.deltaTime * settings.gravity;
				if (state.physicsVelocity.y < -settings.terminalVelocity) {
					state.physicsVelocity.y = -settings.terminalVelocity;
				}
			}

		}

		/// <summary> Logic to update movement every frame </summary>
		private void UpdateMovement() {
			// Sample movement functions (may be driven by AI)
			Vector2 leftStick = leftStickInput();
			Vector2 rightStick = rightStickInput();
			Vector2 keyboard = keyboardInput();

			input = Vector3.zero;
			// Accumulate inputs (controller inputs disabled)
			// if (GlobalSettings.southpaw) {
			//	if (GlobalSettings.legacy) {
			//		input = new Vector3(leftStick.x, 0, rightStick.y);
			//	} else {
			//		input = new Vector3(rightStick.x, 0, rightStick.y);
			//	}
			// } else {
			//	if (GlobalSettings.legacy) {
			//		input = new Vector3(rightStick.x, 0, leftStick.y);
			//	} else {
			//		input = new Vector3(leftStick.x, 0, leftStick.y);
			//	}
			// }
			input += new Vector3(keyboard.x, 0, keyboard.y);

			// If input larger than length one, cap at length one.
			if (input.magnitude > 1) {
				input = input.normalized;
			}

			if (controller.isGrounded) {
				// If we're grounded, update movement normally
				state.moveVelocity = Vector3.MoveTowards(state.moveVelocity, Quaternion.Euler(0, state.yaw, 0) * input, Time.deltaTime * settings.accel);
				// and update sprint state
				sprint = sprintInput();

			} else {
				// If we're in air, only change velocity if the user gives input
				if (input.magnitude > 0) {
					state.moveVelocity = Vector3.MoveTowards(state.moveVelocity, Quaternion.Euler(0, state.yaw, 0) * input, Time.deltaTime * settings.airControl);
				}
			}

			// can't crouchsprint.
			if (crouch) { sprint = false; }
			// Disable weapons this and next frame if we're sprinting this frame 
			if (sprint && controller.isGrounded) { weaponDisableTimer = Time.deltaTime + .01f; }

			// Calculate actual movement distance for this frame
			float speedAdjust = (sprint ? settings.sprintBoost : (crouch ? settings.crouchPenalty : 1f));
			// movement velocity applied with current movement speed (crouch/walk/sprint)
			Vector3 movement = state.moveVelocity * settings.speed * speedAdjust;
			// add physics velocity directly (push player, gravity)
			movement += state.physicsVelocity;

			// Move by calculated movement
			controller.Move(movement * Time.deltaTime);
			// Update "lookAnchor" (MainCamera for player controlled, AI "head" for AI controlled)
			lookAnchor.position = transform.position + settings.cameraOffset;

		}
		/// <summary> Logic to update orientation (look direction) every frame </summary>
		private void UpdateOrientation() {
			// Sample movements (may be driven by AI)
			Vector2 leftStick = leftStickInput();
			Vector2 rightStick = rightStickInput();
			Vector2 mouse = mouseInput();

			// Accumulate inputs (controller inputs disabled)
			Vector2 lookInputs = Vector2.zero;
			// if (GlobalSettings.southpaw) {
			//	if (GlobalSettings.legacy) {
			//		lookInputs = new Vector2(rightStick.x, leftStick.y);
			//	} else {
			//		lookInputs = leftStick;
			//	}
			// } else {
			//	if (GlobalSettings.legacy) {
			//		lookInputs = new Vector2(leftStick.x, rightStick.y);
			//	} else {
			//		lookInputs = rightStick;
			//	}
			// }
			lookInputs += mouse;

			// Inversion (disabled)
			// if (GlobalSettings.invertX) {
			//	lookInputs = new Vector2(-lookInputs.x, lookInputs.y);
			// }
			// if (GlobalSettings.invertY) {
			//	lookInputs = new Vector2(lookInputs.x, -lookInputs.y);
			// }

#if UNITY_EDITOR || UNITY_STANDALONE
			if (!IS_PLAYER || Cursor.lockState == CursorLockMode.Locked) {
				state.pitch += lookInputs.y * -settings.sensitivity.y;// * GlobalSettings.sensitivityY;
				state.yaw += lookInputs.x * settings.sensitivity.x;// * GlobalSettings.sensitivityX;
			}
#else
		state.pitch += lookInputs.y * -settings.sensitivity.y;// * GlobalSettings.sensitivityY;
		state.yaw += lookInputs.x * settings.sensitivity.x;// * GlobalSettings.sensitivityX;
#endif
			// Clamp Pitch
			state.pitch = Mathf.Clamp(state.pitch, -89, 89);
			// Apply rotation to "lookAnchor" (MainCamera for player controlled, AI "head" for AI controlled)
			lookAnchor.rotation = transform.rotation;
			lookAnchor.rotation *= Quaternion.Euler(state.pitch, state.yaw, 0);
		}
		
		/// <summary> Logic to update crouch every frame </summary>
		private void UpdateCrouch() {
			if (crouchInputHeld()) {
				if (crouchAmount < 1) {
					crouchAmount += crouchResponse * Time.deltaTime;
					Mathf.Clamp01(crouchAmount);
					ProcessCrouch();
				}
			} else {
				if (crouchAmount > 0) {
					crouchAmount -= crouchResponse * Time.deltaTime;
					Mathf.Clamp01(crouchAmount);
					ProcessCrouch();
				}
			}
		}
		/// <summary> Logic to adjust hitboxes/head when crouching</summary>
		private void ProcessCrouch() {
			controller.height = Mathf.Lerp(initialControllerHeight, initialControllerHeight / 2f, crouchAmount);
			controller.center = Vector3.Lerp(initialControllerCenter, initialControllerCenter / 2f, crouchAmount);
			settings.cameraOffset = Vector3.Lerp(initialCameraOffset, initialCameraOffset / 2f, crouchAmount);
			// hitbox.height = Mathf.Lerp(initialHitboxHeight, initialHitboxHeight / 2f, crouchAmount);
			// hitbox.center = Vector3.Lerp(initialHitboxCenter, initialHitboxCenter / 2f, crouchAmount);
			// hitbox.radius = Mathf.Lerp(initialHitboxRadius, initialHitboxRadius * 1.5f, crouchAmount);
		}
		
	}
}

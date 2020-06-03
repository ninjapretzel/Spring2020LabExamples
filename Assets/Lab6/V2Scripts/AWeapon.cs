using UnityEngine;
using System;
using System.Collections;
using System.Collections.Generic;

namespace Lab6 {
	/// <summary> Abstract weapon type. </summary>
	/// <remarks> From the same game as <see cref="ViewModel"/> was. 
	/// Similarly, slightly modified it to disconnect it from the game it is from. </remarks>
	public abstract class AWeapon : MonoBehaviour {

		/// <summary> Private field for trigger held. </summary>
		private bool _trigger;
		/// <summary> Primary interface into the weapon. 
		/// Press the trigger to fire, hold it to keep firing. Release to stop. </summary>
		public bool trigger {
			get {
				return _trigger;
			}
			set {
				if (reloading && value) {
					if (!TryInterruptReload()) {
						return;
					}
				}
				justTrigger = !_trigger && value;
				_trigger = value;
			}
		}

		/// <summary> Linked ammo box.</summary>
		public Dictionary<string, float> ammoBox;

		// These tooltips were for my designers/artists working on the weapons: 
		[Tooltip("Type of ammo this weapon is currently using.")]
		public string ammoType = "9mm";
		[Tooltip("Ammo used per time this weapon fires.")]
		public float ammoUse = 1;
		[Tooltip("Maximum Mag Size, 0 or less to draw directly from ammoBox.")]
		public float magSize = -1;
		[Tooltip("Ammo currently in the weapon's magazine.")]
		public float ammoInMag = -1;

		[Tooltip("Time spent to start reloading")]
		public float reloadStartTime = .5f;
		[Tooltip("Time spent to reload once after starting reload process")]
		public float reloadTickTime = .15f;
		[Tooltip("Ammo reloaded per tick, 0 or less to reload whole mag instantly.")]
		public float reloadPerTick = -1;

		[Tooltip("Can the weapon's reload be interrupted after it has been started?")]
		public bool canInterruptReload = false;
		
		/// <summary> Current reload timeout </summary>
		[NonSerialized] public float reloadTimeout = 0;

		/// <summary> Is the weapon currently reloading? </summary>
		[NonSerialized] public bool reloading = false;

		/// <summary> When reloading, has the reloadStartTime elapsed? </summary>
		[NonSerialized] public bool reloadTicking = false;

		/// <summary> How much extra ammo is currently in the box for this weapon? </summary>
		public float ammoInBox { get { return ammoBox != null ? ammoBox[ammoType] : 0; } }

		/// <summary> Current ammo in magazine, or ammoBox, if magizine is disabled. </summary>
		public bool hasAmmoInMag { get { return magSize <= 0 || ammoInMag >= ammoUse; } }

		/// <summary> How much does the weapon kick (recoil) when fired? </summary>
		public virtual float kick { get { return 3f; } }
		/// <summary> Did the weapon fire last frame? </summary>
		public virtual bool didFire { get; protected set; }
		/// <summary> How much time has elapsed till the next shot? (0...1) </summary>
		public virtual float percentReady { get; }
		/// <summary> Is the weapon currently ready to fire? </summary>
		public virtual bool isReady { get; }


		public Transform owner { get; set; }

		public bool hasAmmoToFire {
			get {
				if (magSize > 0) { return hasAmmoInMag; }
				return ammoInBox >= ammoUse;
			}
		}
		public bool justTrigger { get; protected set; }
		
		/// <summary> Default Update for any weapons. </summary>
		protected void Update() {
			justTrigger = false;

			if (reloading) {
				reloadTimeout += Time.deltaTime;
				if (!reloadTicking) {
					if (reloadTimeout > reloadStartTime) {
						reloadTimeout -= reloadStartTime;
						if (reloadPerTick > 0) {
							reloadTicking = true;
						} else {
							reloading = false;
							reloadTicking = false;
						}
						ReloadMag();
					}
				} else {
					if (reloadTimeout > reloadTickTime) {
						reloadTimeout -= reloadTickTime;
						ReloadMag();
						if (ammoInBox <= 0 || ammoInMag >= magSize) {
							reloading = false;
							reloadTicking = false;
						}
					}
				}
			}
		}

		/// <summary> Use ammo from the magazine, or ammoBox, if the magazine is disabled. </summary>
		public void UseAmmo() {
			if (magSize > 0) {
				if (ammoInMag >= ammoUse) {
					ammoInMag -= ammoUse;
				}
			} else {
				if (ammoBox != null) {
					ammoBox[ammoType] -= ammoUse;
				}
			}
		}

		/// <summary> Begins the reload process if possible. </summary>
		public void StartReload() {
			if (!reloading && ammoInMag < magSize && ammoInBox > 0) {
				reloading = true;
				reloadTicking = false;
				reloadTimeout = 0;
			}
		}

		/// <summary> Try to reload the magazine by the given amount. </summary>
		/// <param name="amount"> Amount to reload by, or -1 to try to reload fully. </param>
		public void ReloadMag(float amount = -1) {
			if (magSize > 0 && ammoInMag < magSize && ammoBox != null) {
				float ammoRemaining = ammoBox[ammoType];
				float ammoNeeded = magSize - ammoInMag;

				float toReload = (amount > 0) ? amount : ammoNeeded;

				if (ammoRemaining >= toReload) {
					ammoRemaining -= toReload;
					ammoInMag += toReload;
				} else {
					ammoInMag += ammoRemaining;
					ammoRemaining = 0;
				}

				ammoBox[ammoType] = ammoRemaining;
			}
		}

		/// <summary> Tries to interrupt reloading. </summary>
		/// <returns> True if the weapon is not reloading. </returns>
		public bool TryInterruptReload() {
			if (reloading) {
				if (canInterruptReload || reloadTicking) {
					reloading = false;
					reloadTicking = false;
					return true;
				}
				return false;
			}
			return true;
		}
	} 
}

#if UNITY_EDITOR
using UnityEditor;
#endif
using UnityEngine;
using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine.Events;
using Random = UnityEngine.Random;
namespace Lab6 {

	/// <summary> Projectile weapon </summary>
	/// <remarks> From the same game as <see cref="ViewModel"/>, and <see cref="AWeapon"/>. 
	/// Similarly, slightly modified to remove reverences to specific things from those projects. 
	/// This was one variant of our weapon types, which launched projectile objects.
	/// We also had types that would use various raycasts/shapecasts for lasers,
	/// or would automatically do some effect, for example drain health from the nearest hostile.
	/// This was also used to set up enemies like turrets, that had fixed weapon systems.
	/// </remarks>
	public class ProjectileWeapon : AWeapon {


		/// <summary> Settings for creating projectiles. </summary>
		[Serializable] public class ProjectileSettings {
			/// <summary> Projectile speed (units per second) </summary>
			public float speed = 30f;
			/// <summary> Projectile radius (solid collision) </summary>
			public float radius = .1f;
			/// <summary> Projectile proximity (hurtbox collision) </summary>
			public float proxy = .01f;
			/// <summary> Distance to push projectile forward by </summary>
			public float pushForward = .1f;
		}	

		/// <summary> All of the barrel locations. For single barrel weapons, just set one. </summary>
		public Transform[] barrels;

		/// <summary> Projectile to create when shooting </summary>
		public Transform shotProjetile;

		/// <summary> Cosmetic (or gameplay) stuff to attach to created projectile </summary>
		public Transform[] shotAttachments;
		
		/// <summary> Damage range </summary>
		public BMM damageRange;

		/// <summary> Number of bursts per trigger pull </summary>
		public int bursts = 3;
		/// <summary> Number of pellets per shot </summary>
		public int pellets = 1;
		/// <summary> Time in seconds to be able to fire while trigger is held </summary>
		public float holdFireTime = .5f;
		/// <summary> Time in seconds to be able to fire when trigger is spammed </summary>
		public float tapFireTime = .2f;
		/// <summary> Time in seconds between each burst shot. </summary>
		public float burstTime = .1f;

		/// <summary> Pellet spread </summary>
		public float spread = 0;

		/// <summary> Axis to use for spread, or null for cone spread.</summary>
		public Vector3 fanAxis = Vector3.zero;

		/// <summary> Current recoil value </summary>
		public float curRecoil = 0;
		/// <summary> How quickly the weapon absorbs recoil </summary>
		public float absorbRecoil = 7;

		/// <summary> How much the weapon kicks the viewmodel when firing </summary>
		public float kickAmount = 5;
		/// <summary> How much each fire adds to the recoil value </summary>
		public float recoilAmount = 3;

		/// <summary> set true to only play sound on first shot during burstfire mode. </summary>
		public bool playSoundOnlyOnFirst = false;
		/// <summary> Projectile settings to use when creating projectiles. </summary>
		public ProjectileSettings projectileSettings;

		/// <summary> Delay before weapon may fire again </summary>
		float refireTimeout = 0;
		/// <summary> Delay before next burst shot </summary>
		float burstTimeout = 0;
		/// <summary> Currently firing? (1 or more bursts remaining) </summary>
		bool firing { get { return burst >= 1; } }
		/// <summary> Current burst count </summary>
		int burst;
		/// <summary> Sequence of burst fire for multi-barrel weapons </summary>
		int sequence;

		/// <summary> Callback for when the weapon is fired. </summary>
		public UnityEvent onFireEvent;
		/// <summary> Callback for when the weapon is ready to be fired. </summary>
		public UnityEvent onReadyToFireEvent;

		/// <summary> Abstract weapon kick property </summary>
		public override float kick { get { return kickAmount; } }
		/// <summary> Abstract weapon ready property </summary>
		public override bool isReady { get { return refireTimeout < Mathf.Min(holdFireTime, tapFireTime); } }
		/// <summary> Abstract weapon percent ready property </summary>
		public override float percentReady { get { return refireTimeout / Mathf.Min(holdFireTime, tapFireTime); } }

		/// <summary> Attached sound </summary>
		SoundCue snd;
		/// <summary> Original volume of sound </summary>
		float sndVolume;
		
		void Awake() {
			snd = GetComponent<SoundCue>();
			refireTimeout = tapFireTime - .01f;
		}
		
		new void Update() {
			didFire = false;

			if (!firing) {
				if (refireTimeout < Mathf.Max(tapFireTime, holdFireTime)) {
					refireTimeout += Time.deltaTime;
					if (refireTimeout > Mathf.Max(tapFireTime, holdFireTime)) {
						onReadyToFireEvent?.Invoke();
					}
				}
				if ((trigger && refireTimeout >= holdFireTime) || (justTrigger && refireTimeout >= tapFireTime)) {
					if (hasAmmoToFire) {
						refireTimeout = 0;
						burstTimeout = 0;
						burst = 1;
						Fire(burst);

						didFire = true;
						if (bursts == 1) { burst = 0; }
					} else {

					}
				}
			} else {
				burstTimeout += Time.deltaTime;

				if (burstTimeout >= burstTime) {
					if (hasAmmoToFire) {
						burstTimeout = 0;
						burst = (++burst % bursts);
						Fire(burst);
						didFire = true;

					} else {
						burst = 0;
					}
				}

			}

			curRecoil = Mathf.Lerp(curRecoil, 0, Time.deltaTime * absorbRecoil);

			base.Update();

		}

		void Fire(int i) {
			if (snd != null) {
				if (!playSoundOnlyOnFirst || (playSoundOnlyOnFirst && i == 1)) {
					snd.Play();
				}
			}
			UseAmmo();

			transform.BroadcastMessage("OnFire", kick, SendMessageOptions.DontRequireReceiver);
			
			onFireEvent.Invoke();
			Fire();
		}

		void Fire() {
			for (int i = 0; i < pellets; i++) {
				sequence = (sequence + 1) % barrels.Length;
				Transform t = barrels[sequence];
				Vector3 pos = t.position;
				Quaternion rot = t.rotation;

				Transform proj = Instantiate(shotProjetile, pos, rot);
				foreach (var attach in shotAttachments) {
					Transform at = Instantiate(attach, pos, rot);
					at.SetParent(proj);
				}

				float dmg = damageRange.value;

				BallisticProjectile bp = proj.GetComponent<BallisticProjectile>();
				if (bp == null) { bp = proj.gameObject.AddComponent<BallisticProjectile>(); }
				
				// Pass source object to ALL scripts that can accept it...
				bp.BroadcastMessage("SetSourceObject", owner);

				bp.transform.position += t.forward * projectileSettings.pushForward;
				
				bp.force = projectileSettings.speed;
				bp.radius = projectileSettings.radius;
				bp.proxy = projectileSettings.proxy;

				bp.damage = dmg;

				float cone = spread + curRecoil;
				if (cone > 0) {
					Vector3 coneAxis = Random.onUnitSphere;
					Vector3 totalRot = fanAxis * (-1f + (2f * Random.value))
						+ coneAxis * cone * Random.value;


					proj.Rotate(totalRot);
				}

			}

			curRecoil += recoilAmount;
		}

	}
}

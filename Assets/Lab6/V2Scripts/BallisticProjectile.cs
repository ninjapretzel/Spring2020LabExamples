using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

#if UNITY_EDITOR
using UnityEditor;
#endif

namespace Lab6 {

	// This code is mostly code I had written for a game I had worked on.
	// It is heavily modified to strip out things that I did not want to port to this example.
	/// <summary> Ballistic simulation for more control over bouncing and other qualities </summary>
	public class BallisticProjectile : MonoBehaviour {
		
		/// <summary> Debug gizmos </summary>
		public bool drawDebug = false;
		/// <summary> Should the projectile always face forward? </summary>
		public bool faceAlongVelocity = true;
		/// <summary> Is the projectile affected by timezones? </summary> // Disabled. Chronos plugin not brought over.
		public bool useChronos = true;

		/// <summary> Debug gizmo color </summary>
		static readonly Color lineColor = new Color(1, 1, 1, .05f);
		/// <summary> Debug gizmo color </summary>
		static readonly Color normalColor = new Color(1, 0, 0, .05f);

		/// <summary> Minimum allowed projectile radius </summary>
		public const float MIN_RADIUS = .01f;

		/// <summary> Distance the projectile may travel before being poof'd</summary>
		public float maxDistance = 10;
		/// <summary> Minimum angle to do a bounce. If this is >0, projectile will not reflect its direction if impact angle is too low. </summary>
		public float minBounceAngle = -1;
		/// <summary> Bounciness. </summary>
		public float restitution = .98f;
		/// <summary> Gravity force </summary>
		public float gravity = .2f;
		/// <summary> Projectile solid collision radius </summary>
		public float radius = .01f;
		/// <summary> Projectile hurtbox collision radius </summary>
		public float proxy = .01f;

		/// <summary> Force applied to projectile on start </summary>
		public float force = 5;
		/// <summary> Projectile mass (used to apply physics forces to hit objects) </summary>
		public float mass = .1f;

		/// <summary> Distance to ignore source hurtboxes </summary>
		public float ignoreSourceRadius = 0;
		/// <summary> Minimum velocity, projectile dies if it moves slower </summary>
		public float killVelocity = .1f;
		/// <summary> Minium step velocity. Projectile will refuse to move if it travels a smaller distance </summary>
		// Note: This was used to cause projectiles to 'pause' within timezones if they would cause problems when slowed down.
		public float minStepVelocity = -1f;
		/// <summary> Maximum time to stay alive </summary>
		public float maxTime = 5;
		/// <summary> Maximum number of bounces before being forced to die </summary>
		public int maxBounces = 5;

		/// <summary> Damage to cause upon impact </summary>
		public float damage = 1;
		/// <summary> Time the decal object may stay alive </summary>
		public float decalTime = 10;
		/// <summary> Decal object to create on touch </summary>
		public Transform decal;
		/// <summary> Object that "owns"/shot this projectile </summary>
		public Transform sourceObject;

		/// <summary> Layers to treat as hit/hurtboxes </summary>
		public LayerMask hitboxLayerMask;
		/// <summary> Layers to treat as solid surfaces </summary>
		public LayerMask surfaceLayerMask;

		// <summary> Attached chronos clock</summary>
		//public Clock clock;

		/// <summary> Attached rigidbody (if any) </summary>
		Rigidbody rigid;

		/// <summary> Current velocity </summary>
		[NonSerialized] public Vector3 velocity;
		/// <summary> Current count of bounces </summary>
		[NonSerialized] public int bounces;
		/// <summary> Currently waiting a frame </summary>
		[NonSerialized] public bool waitFrame = false;

		/// <summary> Parent to keep scene clean of spammy decal objects </summary>
		private static GameObject _decalRoot;
		/// <summary> Parent to keep scene clean of spammy decal objects </summary>
		public static GameObject decalRoot {
			get {
				if (!_decalRoot) {
					_decalRoot = new GameObject("DecalRoot");
					DontDestroyOnLoad(_decalRoot);
				}
				return _decalRoot;
			}

		}

		/// <summary> Debug position </summary>
		Vector3 pt;
		/// <summary> Remaining distance before death </summary>
		float remainingDistance;
		/// <summary> Remaining time before death </summary>
		float timeout;
		/// <summary> Distance over last frame </summary>
		float frameDistance;

		/// <summary> Last calculated timescale </summary>
		/// <remarks> Was changed, as chronos is now not used. </remarks>
		public float timeScale { get; private set; } = 1;
		/// <summary> Physics deltatime. </summary>
		float fixedDeltaTime = 0;

		/// <summary> 
		///		Intended to be called via <see cref="Component.BroadcastMessage(string, object)"/>
		///		to pass source information to a projectile on creation. 
		/// </summary>
		/// <param name="t"> Transform to set source to </param>
		public void SetSourceObject(Transform t) {
			sourceObject = t;
		}

		void Start() {
			pt = transform.position;
			velocity = transform.forward * force;
			remainingDistance = maxDistance;

			// timeline = GetComponent<Timeline>();
			//clock = GetComponent<Clock>();
			/*
			if (col == null) {
				col = GetComponent<Collider>();
			}
			if (col != null) {
				col.enabled = false;
			}
			*/

		}

		void FixedUpdate() {
			// Delay first frame, or when flag set externally
			if (!waitFrame) { waitFrame = true; return; }
			// Grab rigidbody and use rigidbody if present
			if (rigid == null) { rigid = GetComponent<Rigidbody>(); }
			if (rigid != null) {
				Vector3 rigidVelocity = rigid.velocity;
				rigid.velocity = Vector3.zero;
				velocity += rigidVelocity;
			}
			
			// Update fixed deltatime 
			// (note, this also used chronos, changed to be more direct) 
			fixedDeltaTime = Time.fixedDeltaTime * timeScale;
			// Distance to move per frame, up to remaining if it is smaller than velocity.
			frameDistance = Mathf.Min(velocity.magnitude, remainingDistance) * fixedDeltaTime;
			
			timeout += fixedDeltaTime;
			
			// Check for timeout/slower than required movement.
			if (timeout > maxTime || frameDistance <= 0 || (killVelocity >= 0 && velocity.magnitude <= killVelocity)) {
				Die();
				return;
			}

			// Don't move if paused within slowdown volume
			if (minStepVelocity >= 0 && velocity.magnitude < minStepVelocity) {
				return;
			}

			// Clamp proxy/radius 
			if (radius < MIN_RADIUS) { radius = MIN_RADIUS; }
			if (proxy < MIN_RADIUS) { proxy = MIN_RADIUS; }

			// Update timescale from chronos system 
			UpdateTimeScale();
			// Safety wall, as there are some edge cases that cause this to get stuck (get it? EDGE CASES?)
			int wall = 55;
			while (frameDistance > 0 && (maxBounces < 0 || bounces < maxBounces)) {
				if (--wall <= 0) {
					Debug.Log($"BallisticProjectile Safety Wall Was Triggered! pos:{transform.position} fd:{frameDistance} rd:{remainingDistance} v:{velocity}");
					break;
				}
				// Do two casts per step 
				RaycastHit hitboxRayhit;
				RaycastHit solidRayhit;
				bool hitboxHit;
				bool solidHit;
				// SphereCast is like Raycast, but it has thickness.
				hitboxHit = Physics.SphereCast(transform.position, proxy, velocity, out hitboxRayhit, frameDistance, hitboxLayerMask);
				solidHit = Physics.SphereCast(transform.position, radius, velocity, out solidRayhit, frameDistance, surfaceLayerMask);

				// Prioritise whichever has a shorter distance.
				if (solidHit && hitboxHit) {
					if (solidRayhit.distance < hitboxRayhit.distance) {
						//Debug.Log($"Hit both {solidRayhit.collider} and {hitboxRayhit.collider}, Skipping hitbox and hitting solid {solidRayhit.distance} < {hitboxRayhit.distance}");
						goto doSolid; // This turned out to be the simplest way to do this. Not evil.
						// At the time we originally worked on the game, C# didn't have inner functions
						// I would write it with those if I was to redo this...
					} else {
						//Debug.Log($"Hit both {solidRayhit.collider} and {hitboxRayhit.collider}, hitting hitbox {solidRayhit.distance} > {hitboxRayhit.distance}");
					}	
				}

				// doHitbox:
				// Check for hitbox hits...
				if (hitboxHit) {
					// If we hit owner after first bounce, skip past them...
					if (bounces == 0 && sourceObject == hitboxRayhit.transform) {
						float skipDistance = (hitboxRayhit.distance + ignoreSourceRadius);
						transform.position = transform.position + velocity.normalized * skipDistance;
						frameDistance -= skipDistance;
						continue;
					}
					// otherwise we hit something!
					transform.position = hitboxRayhit.point;
					// Send self over to an 'OnHit' method on the other object
					hitboxRayhit.collider.SendMessage("OnHit", this, SendMessageOptions.DontRequireReceiver);
					Die();
					return;
				}

				doSolid:
				// Check for solid hits...
				if (solidHit) {
					Collider hitbox = solidRayhit.collider;
					Transform hitobj = hitbox.transform;
					if (hitobj != null && sourceObject != null) {
						//Debug.Log($"{sourceObject}'s {gameObject} hit {hitobj}!");
						if (hitobj == sourceObject && bounces == 0) {
							// Ignore solid collisions with owner on first bounce... 
							continue;
						} else {
							// Otherwise update with solid collision logic.
							bool done = HitWall(solidRayhit);
							if (done) {
								Die();
								return;
							}

						}

					}
				} else {
					// Move to next position
					transform.position += velocity.normalized * frameDistance;
					frameDistance = 0;
				}

			} // End while loop

			// Update facing direction 
			if (faceAlongVelocity) {
				transform.forward = velocity;
			}

			// draw debug gizmos if needed
			if (drawDebug) {
				Debug.DrawLine(pt, transform.position, lineColor);
			}

			// Adjust remaining distance and velocity.
			remainingDistance -= velocity.magnitude * fixedDeltaTime;
			velocity.y -= gravity * timeScale * fixedDeltaTime;

		}

		/// <summary> Logic called whenever this projectile has finished its job. </summary>
		public void Die() {
			SendMessage("Detonate", SendMessageOptions.DontRequireReceiver);
			Destroy(gameObject);
		}

		void OnDestroy() {
			//if (EditorUtils.IsPlayMode) {
			//	Transform trail = transform.Find("Trail");
			//	if (trail != null) {
			//		trail.parent = transform.parent;
			//		var tr = trail.GetComponent<TrailRenderer>();
			//		if (tr != null) {
			//			tr.autodestruct = true;
			//		}
			//	}
			//}
		}

		void UpdateTimeScale() {
			// Was sampled from a time manager, fixed at set timeScale for Unity...
			timeScale = Time.timeScale;
		}

		/// <summary> Logic to check for and handle wall collisions. </summary>
		/// <param name="rayhit"> <see cref="RaycastHit"/> information </param>
		/// <returns> true if the projectile should be deleted, false otherwise. </returns>
		bool HitWall(RaycastHit rayhit) {
			
			// calculate next point to be at
			float rad = (radius > 0) ? radius : 0;
			Vector3 point = transform.position + velocity.normalized * (rayhit.distance - rad / 2f);

			// Get rigidbody, if we hit one
			Rigidbody r = rayhit.collider.GetComponentInParent<Rigidbody>();
			if (r != null) {

				// Left this comment section- we needed to slow bullets down if they were inside of time zones.
				// Adjust the velocity of the impact based on the time scaling of the object hit
				// This will prevent physics objects in timezones from being given an absurd velocity when hit by a bullet in the timezone
				// Timeline timeline = rayhit.collider.GetComponentOnOrAbove<Timeline>();
				//float scaling = timeline == null ? timeScale : timeline.timeScale;
				
				// Apply force to impacted rigidbody...
				Vector3 impactVelocity = velocity * mass;// * scaling;
				r.AddForceAtPosition(impactVelocity, point, ForceMode.Impulse);
			}

			// Bounce, reverse velocity based on impact direction
			Vector3 normal = rayhit.normal;
			Vector3 newDirection = Vector3.Reflect(velocity.normalized, normal);
			//Debug.Log("Hit " + rayhit.collider.name + " : " + velocity.normalized + " -> " + newDirection);
			if (normal == Vector3.zero) {
				normal = Vector3.forward;
			}

			// More debugs...
			if (drawDebug) {
				Debug.DrawLine(pt, point, lineColor, 3);
				Debug.DrawLine(point, point + normal, normalColor, 3);
				Debug.DrawLine(point, point+newDirection * 5, Color.yellow, 30);
			}

			Vector3 flippedVelocity = velocity * -1;
			if (Vector3.Angle(flippedVelocity, normal) < minBounceAngle) {
				//Prevent bouncing if angle is too low.
				bounces = maxBounces + 1;
			}

			// Move to hit point
			transform.position = pt = point;
			// Update direction 
			velocity = newDirection * velocity.magnitude * restitution;

			// Did a bounce
			bounces++;
			// Reduce the rest of the distance FOR THIS FRAME.
			frameDistance -= rayhit.distance;

			// Update timescale based on new position immediately 
			// (note - again, disconnected from chronos, so this isn't needed here
			// but this would prevent bullet impacts in slow-time regions from moving quickly 
			// for the rest of the frame after they hit. Very necessary with the gimmick we had)
			UpdateTimeScale();

			// Check whatever we hit for a script that specifically interacts with these bullets
			OnBallisticProjectileHit onHit = rayhit.collider.GetComponent<OnBallisticProjectileHit>();
			if (onHit != null && onHit.OnHit != null) {
				// Invoke the onhit, get back if we're interrupted by the impact (eg, shooting an explosive)
				bool interrupted = onHit.OnHit.Invoke(this);
				if (interrupted) { return true; }
			}

			// Create a decal at impact point 
			Transform decalOb = Instantiate(decal, point + normal * .001f, Quaternion.identity) as Transform;
			decalOb.forward = normal;
			decalOb.SetParent(rayhit.collider.transform);
			Destroy(decalOb.gameObject, decalTime);

			// Exit. Report we should be destroyed if we have done too many bounces.
			return bounces >= maxBounces;
		}

	}
}

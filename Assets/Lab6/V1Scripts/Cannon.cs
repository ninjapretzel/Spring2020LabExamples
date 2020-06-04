using UnityEngine;

namespace Lab6 { 
	/// <summary> Using Prefabs, V1 </summary>
	public class Cannon : MonoBehaviour {
		/// <summary> Pivot of cannon, used to aim by rotating </summary>
		public Transform pivot;
		/// <summary> Actual bullet source </summary>
		public Transform source;
		/// <summary> Cannonball prefab. Must be RigidBody so it can be given velocity. </summary>
		public Rigidbody cannonball;
		/// <summary> Amount of force to apply onto rigidbody </summary>
		public float force = 100;
		/// <summary> Minimum cannon rotation (left) </summary>
		public Vector3 minRotation = new Vector3(0, 0, -80);
		/// <summary> Maximum cannon rotation (right) </summary>
		public Vector3 maxRotation = new Vector3(0, 0, 80);
		/// <summary> How quickly the rotation responds to mouse movement </summary>
		public float rotationResponse = 5;

		/// <summary> Should this cannon autofire, or shoot with inputs? </summary>
		public bool autofire = true;
		/// <summary> Time before first shot in autofire mode </summary>
		public float fireWait = 2.0f;
		/// <summary> Time between shots in autofire mode </summary>
		public float fireDelay = .3f;
		
		void Start() {
			if (autofire) {
				InvokeRepeating(nameof(Shoot), fireWait, fireDelay);
			}
		}
	
		void Update() {
			// Get the mouse x position, and turn it into a percentage
			// where 0% is the left side, and 100% is the right side 
			float mousex = Input.mousePosition.x;
			float p = mousex / Screen.width;
		
			Vector3 eulers = Vector3.Lerp(minRotation, maxRotation, p);
			Quaternion rotation = Quaternion.Euler(eulers);
		
			// Ease the pivot rotation towards the target
			pivot.rotation = Quaternion.Lerp(pivot.rotation, rotation, Time.deltaTime * rotationResponse);

			// if not in autofire mode and mouse is clicked, shoot
			if (!autofire && Input.GetMouseButtonDown(0)) {
				Shoot();
			}
		}

		void Shoot() {
			// Instantiate a new cannonball
			Rigidbody rb = Instantiate(cannonball, source.position, Random.rotation);
			// and push it forward with the direction of the cannon source.
			rb.AddForce(force * source.forward, ForceMode.Impulse);

			// Also destroy it after some time (so we don't have too many cannonballs everywhere)
			Destroy(rb.gameObject, 10);
		}
	}
}

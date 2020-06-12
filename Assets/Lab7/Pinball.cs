using UnityEngine;

namespace Lab7 {
	/// <summary> Behaviour attached to the pinball. Applies custom gravity force, 
	/// tracks initial position, and resets when requested. </summary>
	public class Pinball : MonoBehaviour {
		
		/// <summary> Force of gravity to apply to pinball. </summary>
		public float gravity = 35f;
		/// <summary> Connected rigidbody </summary>
		public Rigidbody rigid;
		/// <summary> Velocity to use upon reseting. </summary>
		public Vector3 initialVelocity = Vector3.up * .5f + Vector3.forward * 5f;

		/// <summary> Initial position to resume when resetting. </summary>
		Vector3 initialPosition;

		/// <summary> Called by Unity upon loading </summary>
		void Awake() {
			rigid = GetComponent<Rigidbody>();
		}

		/// <summary> Called by Unity befor the first Update </summary>
		void Start() {
			initialPosition = transform.position;
			Reset();
		}
	
		/// <summary> Called by Unity every physics frame </summary>
		void FixedUpdate() {
			// Ignore default gravity (too slow!) and use a personal gravity force.
			rigid.useGravity = false;
			rigid.AddForce(Vector3.down * gravity, ForceMode.Acceleration);
		}

		/// <summary> Called to reset the pinball to its original position/velocities </summary>
		public void Reset() {
			transform.position = initialPosition;
			rigid.velocity = initialVelocity;
			rigid.angularVelocity = Vector3.zero;
		}

	}
}

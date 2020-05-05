using UnityEngine;
// C# `namespace`s are like java `package`s, 
// but don't actually require being in a folder of the same name.
namespace Lab1 {

	// Automatic movement example. 
	// This is intended to be more "real world", 
	// rather than what would be expected to be submitted by a student.
	/// <summary> Basic Lab1 example. </summary>
	public class Lab1V1 : MonoBehaviour {

		/// <summary> Position one </summary>
		public Vector3 start = new Vector3(-5, -5, 0);
		/// <summary> Position two </summary>
		public Vector3 end = new Vector3(8, 5, 0);

		/// <summary> Speed to move between positions </summary>
		public float movementSpeed = 3;
		/// <summary> Speed to rotate on the Z-axis at </summary>
		public float rotateSpeed = 123;

		/// <summary> Current target position </summary>
		Vector3 target;

		/// <summary> Called by Unity, just before this object's first <see cref="Update"/>. </summary>
		void Start() {
			// Initialize target and current position.
			target = end;
			transform.position = start;
		}

		/// <summary> Called by unity every frame. </summary>
		void Update() {
			// Subtract current position with target position 
			Vector3 toTarget = target - transform.position;
			// Get distance to target
			float distanceToTarget = toTarget.magnitude;
			// Distance the next step will take. 
			float stepDist = Time.deltaTime * movementSpeed;

			if (distanceToTarget < stepDist) {
				// If we will overshoot our target, just step there.
				// The ternary operator ?: is incredibly useful.
				// (condition ? valIfTrue : valIfFalse)
				transform.position = target;
				target = ((target - start).magnitude < .01f) ? end : start;
			} else {
				// If not, make a step towards the target.
				transform.position += toTarget.normalized * movementSpeed * Time.deltaTime;
			}

			transform.Rotate(0, 0, rotateSpeed * Time.deltaTime);
		}

	}

}

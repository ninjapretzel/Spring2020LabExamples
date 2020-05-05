using UnityEngine;
namespace Lab1 {

	public class Lab1V1 : MonoBehaviour {
		// Automatic movement example. 
		// This is intended to be more "real world", 
		// rather than what would be expected to be submitted by a student.

		public Vector3 start = new Vector3(-5, -5, 0);
		public Vector3 end = new Vector3(8, 5, 0);

		public float movementSpeed = 3;
		public float rotateSpeed = 123;

		Vector3 target;

		void Start() {
			// Initialize target and current position.
			target = end;
			transform.position = start;
		}

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

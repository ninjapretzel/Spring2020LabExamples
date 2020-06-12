using UnityEngine;

namespace Lab7 {
	/// <summary> Class placed on a trigger to make it bump any colliding rigidbodies away. </summary>
	public class Bumper : MonoBehaviour {
		/// <summary> Delay to prevent the bumper from being triggered rapidly </summary>
		public float debounceTime = .3f;
		/// <summary> Force to apply on colliding objects </summary>
		public float power = 3;
		/// <summary> Increase to scale for collision animation </summary>
		public float scaleIncrease = 1.2f;
		/// <summary> Speed at which scale resets after collision </summary>
		public float scaleResponse = 2f;
		/// <summary> Holds object's initial localScale </summary>
		Vector3 initialScale;
		/// <summary> Tracks delay since last collision </summary>
		float timeout = 0f;

		void Start() {
			// Capture scales and enable collision
			initialScale = transform.localScale;
			timeout = debounceTime;
		}
	
		void Update() {
			// Elapse time and move scale back towards initial scale
			timeout += debounceTime;
			transform.localScale = Vector3.Lerp(transform.localScale, initialScale, Time.deltaTime * scaleResponse);
		}

		void OnTriggerEnter(Collider other) {
			// If it is too soon, exit.
			if (timeout < debounceTime) { return; }

			// Otherwise, check colliding object for rigidbody 
			Rigidbody rigid = other.GetComponent<Rigidbody>();
			if (rigid != null) {
				// If it has one, reset timer, increase scales, and push it away.
				debounceTime = 0f;
				transform.localScale = initialScale * scaleIncrease;

				Vector3 toOther = other.transform.position - transform.position;
				rigid.AddForce(toOther.normalized * power, ForceMode.Impulse);

			}
		}
	
	}
}

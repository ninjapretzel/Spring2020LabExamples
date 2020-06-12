using UnityEngine;

namespace Lab7 {
	/// <summary> Behaviour for pinball flippers </summary>
	public class PinballFlipper : MonoBehaviour {
	
		/// <summary> Flipper activation state. Controlled externally. </summary>
		public bool activated = false;
		/// <summary> Target rotation when the flipper is at rest </summary>
		public Vector3 restRotation = new Vector3(0, 15, 0);
		/// <summary> Target rotation when the flipper is activated </summary>
		public Vector3 activeRotation = new Vector3(0, -33, 0);
		/// <summary> Speed at which the flipper activates </summary>
		public float activationResponse = 15;
		/// <summary> Speed at which the flipper deactivates </summary>
		public float deactivationResponse = 5;

		/// <summary> Called to activate the flipper </summary>
		public void Activate() { activated = true; }
		/// <summary> Called to deactivate the flipper </summary>
		public void Deactivate() { activated = false; }

		/// <summary> Called by unity every physics frame </summary>
		void FixedUpdate() {
			// Pick target/response rate based on activation state
			Quaternion target = Quaternion.Euler(activated ? activeRotation : restRotation);
			float response = activated ? activationResponse : deactivationResponse;

			// Move local rotation towards target rotation at given response rate.
			// Note `fixedDeltaTime` is used, as this is within `FixedUpdate()`
			transform.localRotation = Quaternion.Lerp(transform.localRotation, target, Time.fixedDeltaTime * response);
		}

	}
}

using UnityEngine;

namespace Lab7 {
	/// <summary> Script attached to make a kinematic object move </summary>
	public class KinematicMovement : MonoBehaviour {
		/// <summary> Center point </summary>
		public Vector3 center;
		/// <summary> Flag to grab the initial position of the object to use as <see cref="center"/>. </summary>
		public bool useStartPosAsCenter = true;
		/// <summary> Maximum distance to move along each axis </summary>
		public Vector3 moveDistance = Vector3.right * 3;
		/// <summary> Rate (frequency) to move along each axis </summary>
		public Vector3 moveRate = Vector3.one;

		void FixedUpdate() {
			Vector3 pos = center;
			pos.x += moveDistance.x * Mathf.Cos(moveRate.x * Time.time);
			pos.y += moveDistance.y * Mathf.Sin(moveRate.y * Time.time);
			pos.z += moveDistance.z * Mathf.Sin(moveRate.z * Time.time);
			transform.position = pos;
		}

	}
}

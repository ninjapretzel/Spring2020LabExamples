using UnityEngine;

namespace Lab7 {
	/// <summary> Class for the ends of the flippers </summary>
	public class Whacker : MonoBehaviour {

		/// <summary> Direction of force to apply to ball when struck </summary>
		public Vector3 forceDirection = new Vector3(0, .1f, 1f);
		/// <summary> Force power to apply per degree </summary>
		/// <remarks> Technically this is wrong, since we would also need to
		///	 take into account the distance along the flipper the ball was,
		///	 but this is close enough, and lets the ball rest in the flipper.</remarks>
		public float forcePowerPerDegree = 3;
		/// <summary> Previous frame's rotation </summary>
		Quaternion lastRotation;
		/// <summary> Current frame's rotation </summary>
		Quaternion nowRotation;

		/// <summary> Called by unity every physics frame </summary>
		void FixedUpdate() {
			// Update rotations 
			lastRotation = nowRotation;
			nowRotation = transform.rotation;
		}

		/// <summary> Called by unity every frame a collision exists </summary>
		/// <param name="c"> Collision information for this frame </param>
		void OnCollisionStay(Collision c) {
			// check for rigid body
			if (c.rigidbody != null) {
				// If present, add force based on the delta angle between the last two frames.
				c.rigidbody.AddForce(forceDirection * forcePowerPerDegree * Quaternion.Angle(lastRotation, nowRotation));
			}
		}
	
	}
}

using UnityEngine;

namespace Lab7 {
	/// <summary> Script to be attached to objects to log when they collide with things/touching triggers. </summary>
	public class LogCollisions: MonoBehaviour {
		/// <summary> Called by Unity when this object is involved in a trigger collision </summary>
		/// <param name="c"> Other collider involved in trigger collision </param>
		void OnTriggerEnter(Collider c) {
			Debug.Log($"{this} triggered by {c}.");
		}
		/// <summary> Called by Unity when this object is involved in a solid collision </summary>
		/// <param name="c"> Information about solid collision (involved object, contact points, velocities, etc) </param>
		void OnCollisionEnter(Collision c) {
			Debug.Log($"{this} collided with {c.collider}.");
		}

	}
}

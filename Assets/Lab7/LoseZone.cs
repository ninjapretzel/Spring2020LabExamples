using UnityEngine;
using UnityEngine.Events;

namespace Lab7 {
	public class LoseZone : MonoBehaviour {
		/// <summary> Stuff to do when the game is lost </summary>
		public UnityEvent callback;

		/// <summary> Called by unity when something enters this trigger </summary>
		/// <param name="other"> Collider that touched this trigger </param>
		void OnTriggerEnter(Collider other) {
			// Only trigger for the Pinball.
			if (other.GetComponent<Pinball>() != null) { callback.Invoke(); }
		}

	}
}

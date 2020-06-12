using UnityEngine;
using UnityEngine.Events;

namespace Lab7 {
	/// <summary> Little behaviour to make it easy to use the editor to hook up arbitrary stuff to a keypress. </summary>
	// @ATCHUNG: Normally, I only use this kind of thing for testing purposes.
	// A real control bind system would have more pieces to it than just this script.
	public class ControlBind : MonoBehaviour {
		/// <summary> Key to hit </summary>
		public KeyCode key;
		/// <summary> Stuff to do the frame when key is pressed </summary>
		public UnityEvent pressed;
		/// <summary> Stuff to do every frame when key is held </summary>
		public UnityEvent held;
		/// <summary> Stuff to do the frame when key is released </summary>
		public UnityEvent released;

		void Update() {
			if (Input.GetKeyDown(key)) { pressed.Invoke(); }
			if (Input.GetKey(key)) { held.Invoke(); }
			if (Input.GetKeyUp(key)) { released.Invoke(); }
		}
	
	}
}

using UnityEngine;

namespace Lab9 {

	public class Lab9V1 : MonoBehaviour {
		
		public void OnButtonClicked() {
			Debug.Log($"User Clicked button");
		}

		public void OnTextTyped(string text) {
			Debug.Log($"User changed text input: \"{text}\"");
		}

		public void OnTextFinished(string text) {
			Debug.Log($"User finished editing text input: \"{text}\"");
		}

		public void OnToggle1Changed(bool set) {
			Debug.Log($"Toggle 1 was set to {set}");
		}

		public void OnToggle2Changed(bool set) {
			Debug.Log($"Toggle 2 was set to {set}");
		}

		public void OnToggle3Changed(bool set) {
			Debug.Log($"Toggle 3 was set to {set}");
		}
	}
}

using UnityEngine;
namespace Lab1 {
	
	public class Lab1V2 : MonoBehaviour {
		// Controllable version + logging for achievements.

		public float moveSpeed = 3.33f;
		int frameCount = 0;

		public void Update() {
			// I dislike the "smoothing" that is applied to axes,
			// So I prefer to use the "Raw" value
			Vector3 movement = new Vector3(Input.GetAxisRaw("Horizontal"), Input.GetAxisRaw("Vertical"), 0);
			
			transform.position += movement * moveSpeed * Time.deltaTime;
			
			Debug.Log($"Stop bugging me! {++frameCount}");
		}

	}
}

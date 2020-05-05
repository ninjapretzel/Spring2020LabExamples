using UnityEngine;
// C# `namespace`s are like java `package`s, 
// but don't actually require being in a folder of the same name.
namespace Lab1 {
	
	// Controllable version + logging for achievements.
	/// <summary> Lab1 Achievements </summary>
	public class Lab1V2 : MonoBehaviour {

		/// <summary> Movement speed, exposed and editable in the inspector </summary>
		public float moveSpeed = 3.33f;
		/// <summary> Number of elapsed frames. Not exposed in the inspector, as it is not `public`. </summary>
		int frameCount = 0;

		/// <summary> Called by unity every frame. </summary>
		public void Update() {
			// I dislike the "smoothing" that is applied to axes,
			// So I prefer to use the "Raw" value
			Vector3 movement = new Vector3(Input.GetAxisRaw("Horizontal"), Input.GetAxisRaw("Vertical"), 0);

			// The above builds a vector with x/y in range [-1, 1] based on user input, and z of zero
			// Then we simply scale it by `moveSpeed` and `Time.deltaTime` to move at `moveSpeed` units per second.
			transform.position += movement * moveSpeed * Time.deltaTime;
			
			// Achievement.
			// There are also `Debug.LogWarning` and `Debug.LogError` for more severe logging.
			Debug.Log($"Stop bugging me! {++frameCount}");
		}

	}
}

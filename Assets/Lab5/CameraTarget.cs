using UnityEngine;
using System;
using System.Collections;
using System.Collections.Generic;

namespace Lab5 {

	/// <summary> Script for simple movement, for the object the camera will follow</summary>
	public class CameraTarget : MonoBehaviour {
		/// <summary> Adjustable speed </summary>
		public float speed = 15;

		void Update() {
			// Sample inputs, make a vector across X/Z
			Vector3 movement = new Vector3(Input.GetAxisRaw("Horizontal"), 0, Input.GetAxisRaw("Vertical"));

			// Get the Transform of the main camera (that we assume is the viewport for the player)
			// We want to move relative to this viewport, which supports camera rotation and stuff.
			// This kind of thing is very common in 3d games.
			Transform camTransform = Camera.main.transform;

			// Get the vectors for the camera across X/Z, and flatten them (y = 0, still 1 unit long)
			Vector3 flatRight = camTransform.right;
			Vector3 flatForward = camTransform.forward;
			flatRight.y = 0; flatRight.Normalize();
			flatForward.y = 0; flatForward.Normalize();

			// Adjust the sampled input be in the directions relative to the camera.
			movement = movement.x * flatRight + movement.z * flatForward;

			// And finally move object's position by adding adjusted movement.
			transform.position += movement * speed * Time.deltaTime;

		}
	
	}
}

using UnityEngine;
using System.Text;

namespace Lab6 {
	/// <summary> Single script that handles checking for clicked objects. </summary>
	/// <remarks> This is attached to both the MainCamera and MinimapCamera objects in the Lab5 scene. </remarks>
	public class Spawner : MonoBehaviour {

		/// <summary> Prefab to spawn </summary>
		public Transform prefab;
		/// <summary> Offset along normal of mouse to place object at surface</summary>
		public float spawnOffset = .5f;

		/// <summary> Attached <see cref="Camera"/> reference. </summary>
		Camera cam;

		void Awake() {
			// Find attached camera
			cam = GetComponent<Camera>();
		}

		void Update() {
			// Only need to check if the user clicks
			if (Input.GetMouseButtonDown(0) && cam != null) {

				// for a 3d game, I would use a ray instead of a single point
				// this would be the ray for the pixel that was clicked!
				Ray mouseRay = cam.ScreenPointToRay(Input.mousePosition);

				// Struct to store output from raycast
				RaycastHit rayhit;
				// We cast along the ray, seeing if we hit anything.
				// only checks against solids, 
				// since we did not provide a distance, it will extend the ray infinitely. 
				// since we did not provide a layermask, it will automatically ignores the "Ignore Raycast" layer.
				// those are two common factors you can easily override with extra parameters
				// you can also use two `Vector3`'s place of a `Ray`, as a `Ray` is just two `Vector3`s: `origin` and `direction`
				// and lastly, there is also a 2D version: `Physics2D.Raycast`
				if (Physics.Raycast(mouseRay, out rayhit)) {
					// if raycast returns true, it hit something,
					// and wrote details into `rayhit`

					// Spawn the thing at the point that was clicked
					// Only works if we actually click something! if we click the sky, nothing happens!
					Instantiate(prefab, rayhit.point + rayhit.normal, Quaternion.identity);
				}
			}
		}

	}
}

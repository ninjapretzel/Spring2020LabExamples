using UnityEngine;
using System.Text;

namespace Lab5 {
	/// <summary> Single script that handles checking for clicked objects. </summary>
	/// <remarks> This is attached to both the MainCamera and MinimapCamera objects in the Lab5 scene. </remarks>
	public class Clickmaster : MonoBehaviour {

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

					// We can't bake ahead of time what will get clicked,
					// so we _have_ to do a `GetComponent` here to see if it had a `Clickable` on it.
					Clickable check = rayhit.collider.GetComponent<Clickable>();

					// If we did hit something, talk about it.
					if (check != null) {
						Debug.Log($"{rayhit.transform.Path()} was clicked from the perspective of {gameObject.name}");
					}
					
				}
				
			}

		}
		
	}

	/// <summary> Helper class to hold an "Extension Method" for getting the path.</summary>
	/// <remarks> Extension methods are a very useful C# feature, which allows one to bind 
	/// static methods as if they are members of a type, even if you don't have 
	/// source access to that type, the type is sealed, or generally not modifiable.
	/// Note: They are not "real" member methods, but can be used as if they were.</remarks>
	public static class Extensions {
		/// <summary> Function to build the full path to a scene object </summary>
		/// <param name="transform"> Scene object to create the path of </param>
		/// <returns> Path to the given scene object from scene root. </returns>
		public static string Path(this Transform transform) {
			/// Inner helper function. In C#, you can actually do this.
			/// This function is only visible inside the "Path" method 
			void Recurse(Transform t, StringBuilder str = null) {
				if (t == null) { return; }
				Recurse(t.parent, str);
				str.Append("/");
				str.Append(t.gameObject.name);
			}
			StringBuilder s = new StringBuilder();
			Recurse(transform, s);
			
			return s.ToString();
		}

	}

}

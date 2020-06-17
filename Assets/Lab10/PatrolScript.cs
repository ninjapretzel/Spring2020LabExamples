using System.Collections;
using System.Collections.Generic;
using UnityEngine;
namespace Lab9 {
	/// <summary> Example using coroutines to insert delays inside actions. </summary>
	public class PatrolScript : MonoBehaviour {
		/// <summary> Speed to patrol at </summary>
		public float moveSpeed = 5f;
		/// <summary> Time to wait at each point for... </summary>
		public float moveWait = 7f;
		/// <summary> Speed to change colors at </summary>
		public float colorSpeed = 2f;
		/// <summary> Time to stay at each color for. </summary>
		public float colorWait = 1f;
		/// <summary> Waypoints to hit </summary>
		public List<Vector3> waypoints;
		/// <summary> Colors to change between </summary>
		public List<Color> colors;
		/// <summary> Coroutine tracked</summary>
		private Coroutine path;

		/// <summary> Very simple rebindable keycode </summary>
		public KeyCode stopKey = KeyCode.Space;
		/// <summary> Very simple rebindable keycode </summary>
		public KeyCode restartKey = KeyCode.Return;

		/// <summary> Called by unity when this script is first attached to an object, 
		/// or when we intentionally invoke it via the inspector. This is a safe place 
		/// to add default data to collections, and still allow them to be edited via 
		/// inspector, without clobbering their data when hitting play.. </summary>
		void Reset() {
			waypoints = new List<Vector3>() {
				new Vector3(1, 0, 3),
				new Vector3(4, 0, 4),
				new Vector3(7, 0, 2),
				new Vector3(-5, 0, 6),
				new Vector3(-3, 0, 8),
				new Vector3(-1, 0, -1),
			};
			colors = new List<Color>() {// roygbiv
				Color.red,
				Color.Lerp(Color.red, Color.yellow, .5f),
				Color.yellow,
				Color.green,
				Color.cyan,
				Color.blue,
				Color.magenta,
			};
		}

		void Start() {
			// start patrolling immediately, but we could also make a method to trigger this later
			// save the coroutine, so we can stop it if we need to
			path = StartCoroutine(PatrolWaypoints());
			StartCoroutine(ChangeColors());
		}

		void Update() {
			if (Input.GetKeyDown(stopKey)) {
				StopPatrolling();
			}

			if (Input.GetKeyDown(restartKey)) {
				// Stop patrolling if we were, 
				// to make sure we only have one of the patrol coroutine running at once.
				StopPatrolling();
				// Restart patrolling 
				// Note this will always start moving to the first point again.
				path = StartCoroutine(PatrolWaypoints());
			}
		}

		/// <summary> Code called to stop the patrolling coroutine. </summary>
		public virtual void StopPatrolling() {
			if (path != null) { StopCoroutine(path); }
		}

		/// <summary> Coroutine for moving between waypoints </summary>
		/// <returns> Coroutine IEnumerator managing this coroutine's execution </returns>
		IEnumerator PatrolWaypoints() {
			// path forever, unless StopPatrolling is called
			while (true) {
				// iterate through all points
				foreach (Vector3 point in waypoints) {
					// now with each point, I want to interpolate to it, so I'll use MoveTowards
					// which will at most move me at the given speed
					while (transform.position != point) {
						transform.position = Vector3.MoveTowards(transform.position, point, moveSpeed * Time.deltaTime);
						yield return null; // Wait for next frame.
					}
					// We are at the target point, so wait some time before moving to the next one.
					yield return new WaitForSeconds(moveWait);
				}
			}
		}
	
		/// <summary> Coroutine for changing between colors </summary>
		/// <returns> Coroutine IEnumerator managing this coroutine's execution </returns>
		IEnumerator ChangeColors() {
			SpriteRenderer sprite = GetComponent<SpriteRenderer>();
			Renderer renderer = GetComponent<Renderer>();
			while (true) {
				if (sprite == null && renderer == null) {
					Debug.LogWarning("PatrolScript: In order to see colors change, there must be a renderer attached.");
					break;
				}
				// Colors are just a Vector4, and Vector4 has a MoveTowards!
				Vector4 current = (sprite != null) ? sprite.color : renderer.material.color;

				foreach (Color color in colors) {
					Vector4 target = color;
					while (current != target) {
						current = Vector4.MoveTowards(current, target, colorSpeed * Time.deltaTime);
						if (sprite != null) {
							sprite.color = current; // Implicitly converts back to Color from Vector4!
						} else {
							renderer.material.color = current; // Implicitly converts back to Color from Vector4!
						}

						yield return null; // Wait for next frame.
					}
					// We are at the target color, so wait some time before moving to the next one.
					yield return new WaitForSeconds(colorWait);
				}
			}
		}

	}
}

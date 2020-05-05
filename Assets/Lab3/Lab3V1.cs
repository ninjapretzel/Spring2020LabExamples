using UnityEngine;
// C# `namespace`s are like java `package`s, 
// but don't actually require being in a folder of the same name.
namespace Lab3 {
	/// <summary> Implementation of basic stuff, and Achievement 1.
	/// Achievement 1 is fairly trivial, and works for both 3d meshes and 2d sprites.</summary>
	public class Lab3V1 : MonoBehaviour {
		/// <summary> Position one for moving. </summary>
		public Vector3 startPos = new Vector3(-3, 0, 0);
		/// <summary> Position two for moving. </summary>
		public Vector3 endPos = new Vector3(6, 0, 0);
		/// <summary> Size one of scaling. </summary>
		public Vector3 startScale = Vector3.one * 0.1f;
		/// <summary> Size two of scaling. </summary>
		public Vector3 endScale = Vector3.one * 2f;
		/// <summary> Color one for blending </summary>
		public Color startColor = Color.white;
		/// <summary> Color two for blending </summary>
		public Color endColor = Color.red;

		/// <summary> Since the scaling is updated with <see cref="Time.deltaTime"/>, 
		/// we need to keep track of what direction it is moving in. </summary>
		bool growing;
		/// <summary> Reference to V1 <see cref="GameManager"/> type. </summary>
		GameManager gameManager;

		/// <summary> Called by Unity, just before this object's first <see cref="Update"/>. </summary>
		void Start() {
			transform.position = startPos;
		}

		/// <summary> Called by unity every frame. </summary>
		void Update() {
			// if we don't have a GameManager, try to find one:
			if (gameManager == null) {
				GameObject obj = GameObject.FindGameObjectWithTag("GameManager");
				// If we found an object, try to get its GameManager
				if (obj != null) {
					gameManager = obj.GetComponent<GameManager>();
				}
			}
			// if we still don't have one, exit!
			// (Avoid causing Null Pointer Exceptions)
			if (gameManager == null) { return; }
			// We only need to access the game manager variables within update, 
			// no reason to hold onto them as member variables between frames and waste more memory.
			// so instead, we can just use local variables.
			float speed = gameManager.speed;
			float rotate = gameManager.rotate;
			float scale = gameManager.scale;
			float color = gameManager.color;

			// Just a note for all of this: `Time.time` holds:
			// the accumulated time from the start of the game till now (in seconds)
			// In some cases, it can be much easier to simply represent something 
			// as the function of time, as I do here with position and color.
			
			// We could do all of the position stuff with this one line:
			transform.position = Vector3.Lerp(startPos, endPos, Mathf.PingPong(Time.time * speed / (endPos-startPos).magnitude, 1.0f));

			// Could either apply rotation simply:
			//transform.Rotate(0, 0, Time.deltaTime * rotate);
			// Or derive it from `Time.time` like with position
			transform.rotation = Quaternion.Euler(0, 0, Time.time * rotate);
			// Both of the above modes of rotation will have very similar effects.

			// For an example here, I will use a different method of changing the scale
			// This will ease the scale in towards its target
			// it will move quickly when it is far from its target, and slowly when it is close.
			// This has a nice, visually appealing appearance.
			Vector3 targetScale = growing ? endScale : startScale;
			transform.localScale = Vector3.Lerp(transform.localScale, targetScale, Time.deltaTime * scale);
			// Using Lerp, like you noticed, will never let us reach the target- (similar to Zeno's paradox)
			// but we can measure how close we are to see if we need to change direction:
			if ((transform.localScale - targetScale).sqrMagnitude < .1f) {
				growing = !growing;
			}

			// A third approach for animating things over time would be to use `Lerp` in combination with `Sin` or `Cos`.
			// Remember- `Sin` and `Cos` have output range `[-1, 1]`, so we need to adjust to `[0, 1]` to work with `Lerp`.
			Renderer rend = GetComponent<Renderer>();
			if (rend != null) {
				// Only apply color if we actually have a renderer
				rend.material.color = Color.Lerp(startColor, endColor, .5f + .5f * Mathf.Sin(Time.time * color));
			}

		}
	}
}

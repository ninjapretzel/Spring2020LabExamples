using UnityEngine;

namespace Lab6 {

	/// <summary> "Juice" script for the plasma bolts at the front of the rifle </summary>
	public class Wobble : MonoBehaviour {

		/// <summary> How much the local position can move </summary>
		public Vector3 positionWobble = new Vector3(.1f, .1f, .1f);
		/// <summary> How quickly the local position moves </summary>
		public Vector3 positionWobbleSpeed = new Vector3(.123f, .231f, .111f);

		/// <summary> How much the local scale can move </summary>
		public Vector3 scaleWobble = new Vector3(.3f, 1f, 1f);
		/// <summary> How quickly the local scale moves </summary>
		public Vector3 scaleWobbleSpeed = new Vector3(1, 0, 0);
	
		/// <summary> How much the texture scale can move </summary>
		public Vector2 textureScaleWobble = new Vector2(.1f, .2f);
		/// <summary> How quickly the texture scale moves </summary>
		public Vector2 textureScaleWobbleSpeed = new Vector2(1.31f, 2.32f);

		/// <summary> How quickly the texture pans </summary>
		public Vector2 texturePanningSpeed = new Vector2(0.0f, 1.0f);

		/// <summary> Should the seed be randomized or fixed? </summary>
		public bool randomizeSeed = true;
		/// <summary> Random seed to use </summary>
		public int seed = 123012;
		/// <summary> Randomizer to use to spread animation values </summary>
		public BMM randomizer;
		/// <summary> Time offset of "starting time" </summary>
		public float timeOffset = 0;
	
		/// <summary> Initial scale </summary>
		Vector3 initialScale;
		/// <summary> Initial position </summary>
		Vector3 initialPosition;
		/// <summary> Initial texture scale </summary>
		Vector2 initialTextureScale;
		/// <summary> Material copy, so as not to polute the project material. </summary>
		Material mat;

		/// <summary> Helper to randomize a <see cref="Vector3"/> member </summary>
		void Randomize(ref Vector3 v) { v.x *= randomizer.value; v.y *= randomizer.value; v.z *= randomizer.value; }
		/// <summary> Helper to randomize a <see cref="Vector2"/> member </summary>
		void Randomize(ref Vector2 v) { v.x *= randomizer.value; v.y *= randomizer.value; }
	
		void Awake() {
			// Get the attached Renderer
			Renderer r = GetComponent<Renderer>();
			// Duplicate the material, so that it does not change the original.
			mat = new Material(r.sharedMaterial);
			r.sharedMaterial = mat;

			// Sample initial details:
			initialScale = transform.localScale;
			initialPosition = transform.localPosition;
			initialTextureScale = mat.mainTextureScale;

			// Randomize seed if required
			if (randomizeSeed) {
				seed = Random.Range(0, int.MaxValue);
			}
			// Save the current random state 
			var saved = Random.state;
			// Re-initialize random sequence with seed 
			Random.InitState(seed);
			// Randomize stuff
			timeOffset = Random.value * -1000;
			Randomize(ref positionWobble);
			Randomize(ref positionWobbleSpeed);
			Randomize(ref scaleWobble);
			Randomize(ref scaleWobbleSpeed);
			Randomize(ref textureScaleWobble);
			Randomize(ref textureScaleWobbleSpeed);
			Randomize(ref texturePanningSpeed);

			// Restore saved random state.
			Random.state = saved;
		}

		void Update() {
			// Get current time position 
			float time = Time.time + timeOffset;
			// Apply animations to position...
			Vector3 position = initialPosition;
			position.x += Mathf.Sin(time * positionWobbleSpeed.x) * positionWobble.x;
			position.y += Mathf.Sin(time * positionWobbleSpeed.y) * positionWobble.y;
			position.z += Mathf.Sin(time * positionWobbleSpeed.z) * positionWobble.z;
			transform.localPosition = position;
		
			// Apply animations to scale...
			Vector3 scale = initialScale;
			scale.x += Mathf.Sin(time * scaleWobbleSpeed.x) * scaleWobble.x;
			scale.y += Mathf.Sin(time * scaleWobbleSpeed.y) * scaleWobble.y;
			scale.z += Mathf.Sin(time * scaleWobbleSpeed.z) * scaleWobble.z;
			transform.localScale = scale;

			// Apply animations to texture...
			mat.mainTextureOffset += texturePanningSpeed * Time.deltaTime;
			Vector2 texScale = initialTextureScale;
			texScale.x += Mathf.Sin(time * textureScaleWobbleSpeed.x) * textureScaleWobble.x;
			texScale.y += Mathf.Sin(time * textureScaleWobbleSpeed.y) * textureScaleWobble.y;
			mat.mainTextureScale = texScale;
		}
	
	}
}

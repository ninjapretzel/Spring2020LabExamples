using UnityEngine;
using System.Runtime.CompilerServices;
using Random = UnityEngine.Random;

namespace Lab6 {
	// This code is mostly code I had written for a game I had worked on.
	// It is heavily modified to strip out things that I did not want to port to this example.
	/// <summary> Class representing a ViewModel (model in front of a first-person camera). </summary>
	/// <remarks> It contains code for many procedural animations, which saved our 2 animators tons of time
	/// animating simple animations. If you are interested, for more info about doing procedrual animations, 
	/// check out this GDC talk:  https://www.youtube.com/watch?v=LNidsMesxSE </remarks>
	public class ViewModel : MonoBehaviour {

		/// <summary> Default curve to use to evaluate various procedural animations </summary>
		static readonly AnimationCurve curve = Curves.SineCurve();

		///Helper class to hold animation information for a viewmodel
		[System.Serializable]
		public class AnimationInfo {
			///Offset from camera
			public Vector3 offset = new Vector3(.05f, -.05f, .1f);
			///Offset applied based on recoil amount
			public Vector3 recoilOffset = new Vector3(0, 0, -.05f);
			///Rotation applied based on recoil amount
			public Vector3 recoilRotation = new Vector3(-5, 0, 0);
			///Area that the shake effect takes place in
			public Vector3 shakeOffset = Vector3.zero;

			///How much does recoil scale rotation/offset?
			public float recoilScale = 1;
			///How much does movement affect weapon-bobbing?
			public float bobScale = .3f;
			///How much does the weapon shake each time it is fired?
			public float shakeScale = 1;

			///How fast does the weapon-bob animate speed? (Speed is per-component)
			public Vector3 bobPositionSpeed = new Vector3(1, 2, .6666f);
			///How fast does the weapon-bob animate rotation? (Per component)
			public Vector3 bobRotationSpeed = new Vector3(0, .75f, 0);
			///How far does the weapon-bob move the weapon, maxiumum
			public Vector3 bobPositionOffset = new Vector3(.02f, .01f, .02f);
			///How far does the weapon-bob rotate the weapon, maximum
			public Vector3 bobRotationOffset = new Vector3(0, .5f, 0);

		}

		[System.Serializable] public class Settings {
			///<summary>How quickly does recoil get removed?</summary>
			public float recoilDampening = 4;
			///<summary>How quickly does the viewmodel match the target's rotation?</summary>
			public float lookDampening = 15;
			///<summary>How quickly does the viewmodel animate its rotations?</summary>
			public float rotationDampening = 5;
			///<summary>How quickly does the viewmodel's offsets change (ex, unsighted -> sighted)</summary>
			public float offsetDampening = 5;
			///<summary>How quickly does the viewmodel's shakeing reset</summary>
			public float shakeDampening = 3;
			///<summary>How quickly does the viewmodel exit?</summary>
			public float exitDampening = 3;
			///<summary>How quickly does the movement-bob change?</summary>
			public float bobDampening = 5;
			///<summary>How fast is bobbing animating currently?</summary>
			public float bobSpeedScale = 1;
			///<summary>How fast does bobbing normally animate?</summary>
			public float normalBobSpeed = 1.4f;
			///<summary>How fast does bobbing animate during sprint?</summary>
			public float sprintBobSpeed = 2;
			/// <summary>How fast does bobbing animate during crouch?</summary>
			public float crouchBobSpeed = .75f;
			/// <summary>How fast does bobbing animate during airtime?</summary>
			public float airBobSpeed = .3f;
			/// <summary> How does the viewmodel rotate when the owner is in the air?</summary>
			public Vector3 airRotation = new Vector3(-20, 0, 0);

			/// <summary> How does the viewmodel rotate when the owner is sprinting? </summary>
			public Vector3 sprintRotation = new Vector3(15, -35, 15);
			/// <summary> How does the viewmodel move when the owner is sprinting? </summary>
			public Vector3 sprintOffset = new Vector3(-.05f, -.05f, .05f);

			/// <summary> How does the viewmodel rotate when disabled? </summary>
			public Vector3 disabledRotation = new Vector3(75, 0, 0);
			/// <summary> How does the viewmodel move when disabled? </summary>
			public Vector3 disabledOffset = new Vector3(0, -.02f, 0);

			/// <summary> How does the viewmodel rotate with "default" reload animations? </summary>
			public Vector3 reloadRotation = new Vector3(90, 0, 0);
			/// <summary> How long after shooting does the model hold before sprinting again? </summary>
			public float antiSprintTime = .2f;
		}

		/// <summary> deltaTime based on current Chrono </summary>
		/// Note: this game had a gimmick of being able to manipulate time.
		/// For this example, the relevant code that worked out the correct 
		/// deltaTime to use was removed in place of just using `Time.deltaTime`,
		/// as the other classes it relied on are not a part of the example. 
		public float deltaTime { get { return Time.deltaTime; } }

		/// <summary> Layer that the viewmodels are on. </summary>
		public static int LAYER_VIEWMODELS = 30;

		/// <summary> Viewmodel animation information  </summary>
		public AnimationInfo info;
		/// <summary> Viewmodel settings animation </summary>
		public Settings settings;

		/// <summary> Spring for moving position</summary>
		/// See "Assets/Packages/3DUtils/Spring.cs" for details
		public SpringV3 positionSpring;
		/// <summary> Spring for shaking </summary>
		public SpringV3 shakeSpring;
		/// <summary> Spring for recoil</summary>
		public Spring recoilSpring;

		/// <summary> Physical model </summary>
		Transform model;
		/// <summary> Owner of the viewmodel </summary>
		public Transform owner;

		/// <summary> Current bob animation speed scale </summary>
		public float bobSpeed = 1f;
		/// <summary> current recoil position value </summary>
		public float recoil = 0;
		/// <summary> current bob position value </summary>
		public float bob = 0;
		/// <summary> current air position value </summary>
		public float air = 0;
		/// <summary> current reload position value </summary>
		public float reload = 0;
		/// <summary> current sprint position value </summary>
		public float sprint = 0;
		/// <summary> current crouch position value </summary>
		public float crouch = 0;
		/// <summary> current disabled position value </summary>
		public float disabled = 0;

		/// <summary> Current bob animation time </summary>
		float bobTime;
		/// <summary> Time of the last trigger pull </summary>
		float lastFireTime;
	
		/// <summary> Current offset </summary>
		Vector3 offset;
		/// <summary> Current offset from shaking </summary>
		Vector3 shakeOffset;
		/// <summary> Current "offset" rotation </summary>
		Quaternion rotation;
		/// <summary> Offset from owner </summary>
		Vector3 offsetPosition;
	
		// Vector3 initialOffset;
		// Quaternion initialRotation;
	
		void Awake() {
			// Find the attached model
			model = transform.Find("Model");
			// Apply viewmodel layers (typically viewmodels are rendered in a separate pass)
			SetViewmodelLayers();
			// Start with identity offset rotation
			rotation = Quaternion.identity;
			
			//ChangeModelLayer(LAYER_VIEWMODELS);
			//DontDestroyOnLoad(gameObject);
		}
	

		void LateUpdate() {
			First();
			Second();

			if (owner == null) {
				Destroy(gameObject);
			}
			transform.position = owner.position + offsetPosition;
		}

		void First() {
			// Scale bob speed based off of settings and current state
			float bobSpeedScale = settings.normalBobSpeed;
			bobSpeedScale = Mathf.Lerp(bobSpeedScale, settings.sprintBobSpeed, sprint);
			bobSpeedScale = Mathf.Lerp(bobSpeedScale, settings.crouchBobSpeed, crouch);
			bobSpeedScale = Mathf.Lerp(bobSpeedScale, settings.airBobSpeed, air);

			// Update bob/fire timers
			bobTime += deltaTime * bobSpeedScale * bobSpeed;
			lastFireTime += deltaTime;
		}

		void Second() {
			// Must have an owner to do animations
			if (owner == null) { return; }
			Quaternion lastRotation = transform.rotation;
			// Clamp bob value 
			bob = Mathf.Clamp01(bob);

			// Update position spring 
			positionSpring.target = info.offset;
			offset = positionSpring.Animate(offset);
			// Update shake spring 
			shakeSpring.target = Vector3.zero;
			shakeOffset = shakeSpring.Animate(shakeOffset);
			// Update recoil spring
			recoil = recoilSpring.Animate(recoil);
			// Update offset rotation towards identity
			rotation = Quaternion.Lerp(rotation, Quaternion.identity, deltaTime * settings.rotationDampening);

			// Take owners rotation as basis
			Quaternion targetRotation = owner.rotation;
			// Apply all animation rotations based on current states.
			targetRotation *= rotation;
			targetRotation *= Quaternion.Euler(settings.airRotation * air);
			targetRotation *= Quaternion.Euler(settings.reloadRotation * reload);
			targetRotation *= Quaternion.Euler(settings.sprintRotation * sprint);
			targetRotation *= Quaternion.Euler(settings.disabledRotation * disabled);
			transform.rotation = Quaternion.Lerp(transform.rotation, targetRotation, deltaTime * settings.lookDampening);

			// Move to owner position + offsets
			transform.position = owner.position + transform.rotation * offset;
			transform.position += owner.rotation * (settings.sprintOffset * sprint);
			transform.position += owner.rotation * (settings.disabledOffset * disabled);

			// Evaluate weapon bobbing curves
			Vector3 bobOffset = Vector3.Scale(info.bobPositionOffset, curve.Eval(info.bobPositionSpeed * bobTime));
			Vector3 bobRotation = Vector3.Scale(info.bobRotationOffset, curve.Eval(info.bobRotationSpeed * bobTime));

			// Apply weapon bob on top of previous animations
			transform.Translate(bobOffset * info.bobScale * bob);
			transform.Rotate(bobRotation * info.bobScale * bob);
			// Apply recoil on top of previous animations
			transform.Rotate(info.recoilRotation * info.recoilScale * recoil);
			transform.Translate(info.recoilOffset * info.recoilScale * recoil);
			// Apply shake on top of previous animations
			transform.Translate(shakeOffset);

			// Record our current actual offset
			offsetPosition = transform.position - owner.position;
		}

		/// <summary> Change the layer that all attached renderers are on  </summary>
		/// <remarks> If you were wondering, this was used when viewmodels were turned into worldmodels and vice-versa. 
		/// Weapons could receive attachements from pickups, so we had to always get all renderers, 
		/// to avoid writing a complex caching system, plus this only happened the frame the player picked up or dropped a weapon.</remarks>
		public void ChangeModelLayer(int layer) {
			foreach (Renderer rend in model.GetComponentsInChildren<Renderer>()) {
				rend.gameObject.layer = layer;
			}
		}

		/// <summary> Change the layer that all attached renderers are on  </summary>
		/// <remarks> If you were wondering, this was used when viewmodels were turned into worldmodels and vice-versa. 
		/// Weapons could receive attachements from pickups, so we had to always get all renderers, 
		/// to avoid writing a complex caching system, plus this only happened the frame the player picked up or dropped a weapon.</remarks>
		public void SetViewmodelLayers() {
			foreach (Renderer rend in model.GetComponentsInChildren<Renderer>()) {
				if (rend.gameObject.layer < LAYER_VIEWMODELS) {
					rend.gameObject.layer = LAYER_VIEWMODELS;
				}
			}
		}

		/// <summary> Fire the viewmodel, scaled by value. Trigger by sending 'OnFire' to all attached scripts </summary>
		public void OnFire(float val = 1f) {
			Recoil(val);
			lastFireTime = 0;
		}

		/// <summary> Recoil the viewmodel by an amount </summary>
		public void Recoil(float val) {
			recoil += val;
			shakeOffset += Vector3.Scale(Random.insideUnitSphere, info.shakeOffset) * info.shakeScale;
		}
	}

	/// <summary> Helpers for working with <see cref="AnimationCurve"/>s. </summary>
	public static class Curves {
		
		/// <summary> Evaluate a curve at 3 given positions. </summary>
		/// <param name="a"> Curve to evaluate </param>
		/// <param name="f"> Positions to evaluate at </param>
		/// <returns> Value of curve at positions </returns>
		public static Vector3 Eval(this AnimationCurve a, Vector3 p) {
			Vector3 v = Vector3.zero;
			v.x = a.Evaluate(p.x);
			v.y = a.Evaluate(p.y);
			v.z = a.Evaluate(p.z);
			return v;
		}

		/// <summary> Create a unit sin curve (Domain (0,1) Range (-1, 1)) </summary>
		/// <returns> Unit sin curve (Domain (0,1) Range (-1, 1)). </returns>
		public static AnimationCurve SineCurve() { return SineCurve(1, 1); }
		/// <summary> Create a sin curve </summary>
		/// <param name="length"> Length of curve </param>
		/// <param name="magnitude"> Maximum distance from zero the curve reaches. </param>
		/// <returns> Sin curve with the given parameters. </returns>
		public static AnimationCurve SineCurve(float length, float magnitude) {
			AnimationCurve a = new AnimationCurve();
			a.preWrapMode = WrapMode.Loop;
			a.postWrapMode = WrapMode.Loop;
			a.AddKey(0, 0);
			a.AddKey(.25f * length, -magnitude);
			a.AddKey(.5f * length, 0);
			a.AddKey(.75f * length, magnitude);
			a.AddKey(length, 0);
			return a;
		}

	}
}

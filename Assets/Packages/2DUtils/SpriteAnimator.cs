#if UNITY_EDITOR
using UnityEditor;
#endif
using UnityEngine;
using System;
using System.Collections;
using System.Collections.Generic;

/// <summary> Helper methods </summary>
public static class AnimHelpers {
	/// <summary> Extension method, does proper modulo on negative numbers. % is 'remainder' operator, not modulo </summary>
	/// <param name="x"> int to mod </param>
	/// <param name="m"> space to mod inside </param>
	/// <returns> number in range [0, m) </returns>
	public static int mod(this int x, int m) {
		int r = x % m;
		return r < 0 ? r + m : r;
	}
}

/// <summary> Storage object for sprite animation data </summary>
[Serializable]
public class SpriteAnim {
	/// <summary> Poses to flip through </summary>
	public Sprite[] poses;
	/// <summary> Cues to fire </summary>
	public SpriteAnimCue[] cues;
	/// <summary> Speed to flip through poses at (frames per second) </summary>
	public float animSpeed = 15.0f;
	/// <summary> Should the sprites be mirrored over X? </summary>
	public bool mirrorX = false;
	/// <summary> Should the sprites be mirrored over Y? </summary>
	public bool mirrorY = false;
	/// <summary> Should every other animation be played flipped over X? </summary>
	public bool animateMirrorX = false;
	/// <summary> Should every other animation be played flipped over Y? </summary>
	public bool animateMirrorY = false;
}
/// <summary> Storage object to hold sprite animation cues. </summary>
[Serializable]
public class SpriteAnimCue {
	/// <summary> Frame that the message should fire on </summary>
	public float frame;
	/// <summary> Message that should be fired </summary>
	public string message;
	/// <summary> Optional, additional parameter to fire. </summary>
	public string arg;
}
#if UNITY_EDITOR
/// <summary> Custom drawer class to make it easier to edit cues </summary>
[CustomPropertyDrawer(typeof(SpriteAnimCue))]
public class SpriteAnimCueDrawer : PropertyDrawer {
	private static readonly GUIContent FRAME_PREFIX = new GUIContent("F#");
	private static readonly GUIContent MESSAGE_PREFIX = new GUIContent("MSG");
	private static readonly GUIContent ARG_PREFIX = new GUIContent("ARG");
	// For those of you students poking around-
	// this draws the GUI in the Inspector window-
	// when it wants to draw/allow you to edit a `SpriteAnimCue`, this runs:
	public override void OnGUI(Rect position, SerializedProperty property, GUIContent label) {
		EditorGUI.BeginProperty(position, label, property);
		position = EditorGUI.PrefixLabel(position, GUIUtility.GetControlID(FocusType.Passive), label);
		var indent = EditorGUI.indentLevel;
		EditorGUI.indentLevel = 0;

		float x = position.x;
		float y = position.y;
		float h = position.height;
		float w1 = 45;
		float w2 = 100;
		float w3 = 100;
		float spacing = 2;
		Rect frameRect = new Rect(x, y, w1, h);
		x += w1 + spacing;
		Rect messageRect = new Rect(x, y, w2, h);
		x += w2 + spacing;
		Rect paramRect = new Rect(x, y, w3, h);
		x += w3 + spacing;

		EditorGUIUtility.labelWidth = 14f;
		EditorGUI.PropertyField(frameRect, property.FindPropertyRelative("frame"), FRAME_PREFIX);
		EditorGUIUtility.labelWidth = 28f;
		EditorGUI.PropertyField(messageRect, property.FindPropertyRelative("message"), MESSAGE_PREFIX);
		EditorGUIUtility.labelWidth = 28f;
		EditorGUI.PropertyField(paramRect, property.FindPropertyRelative("arg"), ARG_PREFIX);

		EditorGUI.indentLevel = indent;
		EditorGUI.EndProperty();
		// It is possible to extend the Unity engine quite a lot-
		// being able to write custom tools is a good skill to have!
	}
}
#endif


/// <summary> Behavior that handles sprite animation </summary>
public class SpriteAnimator : MonoBehaviour {
	/// <summary> Empty sprite array. </summary>
	private static readonly Sprite[] EMPTY = new Sprite[0];

	/// <summary> Optional asset to use to specify animation </summary>
	public SpriteAnimAsset animAsset;

	/// <summary> Target spriteRenderer </summary>
	public SpriteRenderer spriteRenderer;
	/// <summary> Animation to play </summary>
	public SpriteAnim anim;
	/// <summary> Rate to scale animation at (1.0 means normal speed, pause using 0.0, double speed with 2.0, reverse with -1.0, etc) </summary>
	public float animRate = 1.0f;
	/// <summary> Time of animation </summary>
	public float animTimeout = 0.0f;
	/// <summary> Should any x-animation to be applied be flipped? </summary>
	public bool flipX = false;
	/// <summary> Should any y-animation to be applied be flipped? </summary>
	public bool flipY = false;

	/// <summary> Poses to flip through </summary>
	public Sprite[] poses { get { return anim != null ? anim.poses : EMPTY; } }
	/// <summary> Speed to flip through poses at (frames per second) </summary>
	public float animSpeed { get { return anim != null ? anim.animSpeed : 1.0f; } }
	/// <summary> Does the current animation mirror over X? </summary>
	public bool mirrorX { get { return anim != null ? anim.mirrorX : false; } }
	/// <summary> Does the current animation mirror over Y? </summary>
	public bool mirrorY { get { return anim != null ? anim.mirrorY : false; } }
	/// <summary> Should every other animation cycle be played flipped over X? </summary>
	public bool animateMirrorX { get { return anim != null ? anim.animateMirrorX : false; } }
	/// <summary> Should every other animation cycle be played flipped over Y? </summary>
	public bool animateMirrorY { get { return anim != null ? anim.animateMirrorY : false; } }
	/// <summary> Get the loop that the animation would be on. </summary>
	public int loop { get { return anim != null ? ((int)(animTimeout * animSpeed)) / (poses.Length) : 0; } }
	/// <summary> Get the percentage of how played the current animation is (may be over 1.0). </summary>
	public float percent { get { return anim != null ? animTimeout * animSpeed / poses.Length : 0; } }

	void Awake() {
		if (animAsset != null) { anim = animAsset.data; }
		if (spriteRenderer == null) { spriteRenderer = GetComponent<SpriteRenderer>(); }
	}
	
	void Update() {
		// Find missing link, and exit if not present.
		if (spriteRenderer == null) { spriteRenderer = GetComponent<SpriteRenderer>(); }
		if (spriteRenderer == null) { return; }

		// Fire any cues that will be passed
		CheckCues(animTimeout, Time.deltaTime * animRate);
		// Elapse time
		animTimeout += Time.deltaTime * animRate;
		
		// Update sprite and flip states
		spriteRenderer.sprite = GetPose(animTimeout);
		spriteRenderer.flipX = GetMirrorX(animTimeout) ^ flipX;
		spriteRenderer.flipY = GetMirrorY(animTimeout) ^ flipY;
	}
	
	/// <summary> Checks for and fires any cues. </summary>
	/// <param name="last"> Last animation time </param>
	/// <param name="time"> Current animation time </param>
	public void CheckCues(float last, float time) {
		if (poses.Length == 0 || anim.cues == null || anim.cues.Length == 0) { return; }
		float a = (last*animSpeed) % poses.Length;
		float b = ((last+time)*animSpeed) % poses.Length;
		for (int i = 0; i < anim.cues.Length; i++) {
			var cue = anim.cues[i];
			if (a <= cue.frame && b >= cue.frame) {
				//Debug.Log($"Firing cue {cue.message}({cue.arg})");
				gameObject.SendMessage(cue.message, cue.arg, SendMessageOptions.DontRequireReceiver);
				gameObject.SendMessageUpwards(cue.message, cue.arg, SendMessageOptions.DontRequireReceiver);
			}
		}
	}


	/// <summary> Plays the given animation, with an option to set the rate and time. </summary>
	/// <param name="anim"> Animation data to play </param>
	/// <param name="rate"> Nullable float option for overriding rate </param>
	/// <param name="timeout"> Nullable float option for overriding timeout </param>
	public void Play(SpriteAnim anim, float? rate = null, float? timeout = null) {
		this.anim = anim;
		if (rate.HasValue) { animRate = rate.Value; }
		if (timeout.HasValue) { animTimeout = timeout.Value; }
	}
	

	/// <summary> From a time in seconds, what pose should be displayed </summary>
	/// <param name="time"> Time, in seconds, to sample a pose </param>
	/// <returns> Pose for the given time </returns>
	public Sprite GetPose(float time) {
		if (poses.Length == 0) { return null; }
		// Scale based on animation speed
		time *= animSpeed;
		// Convert to valid index in poses array
		int cel = ((int)(time)).mod(poses.Length);
		// Return that pose
		return poses[cel];
	}

	/// <summary> For a time in seconds, should the sprite be mirrored or not? </summary>
	/// <param name="time"> Time in seconds, to sample FlipX state </param>
	/// <returns> FlipX state </returns>
	public bool GetMirrorX(float time) {
		if (animateMirrorX) {
			// Scale based on animation speed
			time *= animSpeed;
			// If we animate mirroring, figure out how many times the animation has played 
			time /= poses.Length;
			// And every odd time the animation plays, flip it.
			return ((int)time).mod(2) == 1;
		} else {
			// If not animated, we just return the mirror state
			return mirrorX;
		}
	}

	/// <summary> For a time in seconds, should the sprite be mirrored or not? </summary>
	/// <param name="time"> Time in seconds, to sample FlipX state </param>
	/// <returns> FlipX state </returns>
	public bool GetMirrorY(float time) {
		if (animateMirrorY) {
			// Scale based on animation speed
			time *= animSpeed;
			// If we animate mirroring, figure out how many times the animation has played 
			time /= poses.Length;
			// And every odd time the animation plays, flip it.
			return ((int)time).mod(2) == 1;
		} else {
			// If not animated, we just return the mirror state
			return mirrorY;
		}
	}
}

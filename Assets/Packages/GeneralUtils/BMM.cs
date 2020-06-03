#if UNITY_EDITOR
using UnityEditor;
#endif
using UnityEngine;

/// <summary> Data class to store a <see cref="bool"/> flag and two <see cref="float"/>s for a randomizable range.</summary>
/// <remarks> From the same game as the <see cref="ProjectileWeapon"/>, was used in many places in our codebase. </remarks>
[System.Serializable] public class BMM {
	///<summary> Randomize this field? </summary>
	public bool randomize { get { return flag; } }
	/// <summary> More generic name, since this isn't always for randomization </summary>
	public bool flag = false;
	///<summary> Lowest possible value. </summary>
	public float min = 0.9f;
	///<summary> Largest possible value. </summary>
	public float max = 1.1f;

	///<summary> Get the (next) value, evenly distributed </summary>
	public float value {
		get {
			if (!flag) { return 1.0f; }
			return Random.Range(min, max);
		}
	}

	///<summary> Get the (next) value, normally distributed (approx) </summary>
	public float normal {
		get {
			if (!flag) { return 1.0f; }
			// Averages tend towards normal distribution.
			float roll = (Random.value + Random.value + Random.value) / 3f;
			return min + roll * (max-min);
		}
	}

	///<summary> Default constructor </summary>
	public BMM() {
		flag = false;
		min = 0.9f;
		max = 1.1f;
	}

	/// <summary> Two parameter constructor</summary>
	public BMM(float mmin, float mmax) {
		flag = true;
		min = mmin;
		max = mmax;
	}

	///<summary> Three parametered constructor </summary>
	public BMM(bool bb, float mmin, float mmax) {
		flag = bb;
		min = mmin;
		max = mmax;
	}

}
#if UNITY_EDITOR
// Note: this is some editor code that extends the Unity Editor.
// you can write code like this for your own custom types to make them easier to modify in the inspector.
// however, most of the time, the default foldable UI is desirable.
/// <summary> Draws the <see cref="BMM"/> class in the inspector compactly. </summary>
[CustomPropertyDrawer(typeof(BMM))] public class BMMDrawer : PropertyDrawer {

	public override void OnGUI(Rect position, SerializedProperty property, GUIContent label) {
		label = EditorGUI.BeginProperty(position, label, property);
		Rect contentPosition = EditorGUI.PrefixLabel(position, label);

		contentPosition.width *= 0.1f;
		EditorGUI.indentLevel = 0;
		EditorGUIUtility.labelWidth = 14f;
		EditorGUI.PropertyField(contentPosition, property.FindPropertyRelative("flag"), new GUIContent(""));

		contentPosition.x += contentPosition.width;
		contentPosition.width *= 4.5f;
		EditorGUIUtility.labelWidth = 14f;
		EditorGUI.PropertyField(contentPosition, property.FindPropertyRelative("min"), new GUIContent("R"));

		contentPosition.x += contentPosition.width;
		EditorGUIUtility.labelWidth = 14f;
		EditorGUI.PropertyField(contentPosition, property.FindPropertyRelative("max"), new GUIContent("..."));

		EditorGUI.EndProperty();
	}

}
#endif

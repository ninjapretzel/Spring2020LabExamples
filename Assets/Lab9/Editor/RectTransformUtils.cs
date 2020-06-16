// This is intended to be an extension to the Unity editor, and thus, we only want to compile it in editor mode
// If we didn't wrap the code in this conditional compile, it would make building the final game break.
#if UNITY_EDITOR
using UnityEngine;
using UnityEditor;

/// <summary> Class for a custom editor window. </summary>
/// <remarks> Makes it much, much easier to modify UI objects for precise placement by their anchors. </remarks>
public class RectTransformUtils : EditorWindow {

	public static RectTransformUtils window;

	/// <summary> Shows the <see cref="RectTransformUtils"/> window. </summary>
	/// <remarks> This adds a new menu and option to Unity's top menu. </remarks> 
	[MenuItem("Custom/RectTransform Utils")]
	public static void ShowWindow() {
		window = (RectTransformUtils)EditorWindow.GetWindow(typeof(RectTransformUtils));
	}

	/// <summary> Toggles the current anchor mode. </summary> 
	/// <remarks> This adds a new menu and option to Unity's top menu. 
	/// This is also hooked up to "ALT + A" as its shortcut. </remarks>
	[MenuItem("Custom/Toggle Force Percentage Anchors &a")]
	public static void ToggleAnchorMode() {
		anchorMode = !anchorMode;
	}
	/// <summary> Toggles the active state of all selected GameObjects. </summary>
	/// <remarks> This adds a new menu and option to Unity's top menu.
	/// This is also hooked up to "ALT + Q" as its shortcut. </remarks>
	[MenuItem("Custom/Toggle Active State &q")]
	public static void ToggleSelectionActiveState() {
		var selected = Selection.gameObjects;
		if (selected.Length == 0) { return; }
		bool active = selected[0].activeSelf;
		
		// Record change in the undo stack
		Undo.RecordObjects(selected, "Toggle Active State");
		for (int i = 0; i < selected.Length; i++) {
			selected[i].SetActive(!active);
		}
		// Undo.IncrementCurrentGroup();
	}

	/// <summary> True if anchor adjustment mode is engaged </summary>
	private static bool anchorMode = false;

	/// <summary> Current attached <see cref="RectTransform"/>. </summary>
	RectTransform rt;
	/// <summary> Anchored position of attached <see cref="RectTransform"/>. </summary>
	Vector2 anchoredPos { get { return (rt == null) ? Vector2.zero : rt.anchoredPosition; } }
	/// <summary> Maximum anchor position of attached <see cref="RectTransform"/>. </summary>
	Vector2 anchorMax { get { return (rt == null) ? Vector2.zero : rt.anchorMax; } }
	/// <summary> Minimum anchor position of attached <see cref="RectTransform"/>. </summary>
	Vector2 anchorMin { get { return (rt == null) ? Vector2.zero : rt.anchorMin; } }
	/// <summary> Maximum offset position of attached <see cref="RectTransform"/>. </summary>
	Vector2 offsetMax { get { return (rt == null) ? Vector2.zero : rt.offsetMax; } }
	/// <summary> Minimum offset position of attached <see cref="RectTransform"/>. </summary>
	Vector2 offsetMin { get { return (rt == null) ? Vector2.zero : rt.offsetMin; } }
	/// <summary> Pivot position of attached <see cref="RectTransform"/>. </summary>
	Vector2 pivot { get { return (rt == null) ? Vector2.zero : rt.pivot; } }
	/// <summary> Screenspace position of attached <see cref="RectTransform"/>. </summary>
	Rect rect { get { return (rt == null) ? new Rect(0, 0, 0, 0) : rt.rect; } }
	/// <summary> Size deltas of attached <see cref="RectTransform"/>. </summary>
	Vector2 sizeDelta { get { return (rt == null) ? Vector2.zero : rt.sizeDelta; } }

	/// <summary> Constructor, called when Unity creates the window for the first time, 
	/// and every time code is recompiled. </summary>
	public RectTransformUtils() { }

	/// <summary> Called during the UNITY EDITOR UPDATE CYCLE, 
	/// - only when things are modified in the scene during edit mode
	/// - approx 100 times a second during play mode </summary>
	void Update() {
		GameObject selected = Selection.activeGameObject;
		if (selected != null) {
			rt = selected.GetComponent<RectTransform>();
		} else {
			rt = null;
		}

		if (anchorMode) {

			if (rt != null && selected.GetComponent<Canvas>() == null && (rt.offsetMax != Vector2.zero || rt.offsetMin != Vector2.zero)) {
				Undo.RecordObject(rt, "Force rect offsets to zero");
				rt.rotation = Quaternion.identity;
				// TODO: figure out how to adjust anchors when these values are nonzero
				//Vector2 omax = rt.offsetMax;
				//Vector2 omin = rt.offsetMin;
				rt.offsetMax = Vector2.zero;
				rt.offsetMin = Vector2.zero;
				rt.pivot = new Vector2(0.5f, 0.5f);

			}
			Undo.IncrementCurrentGroup();
		}

		Repaint();
	}

	/// <summary> Helper method to draw a label in the next position </summary>
	/// <param name="msg"> Content of label </param>
	void Label(string msg) { GUILayout.Label(msg); }
	/// <summary> Called by Unity to repaint the custom window. </summary>
	void OnGUI() {
		anchorMode = GUILayout.Toggle(anchorMode, "Anchor Mode");

		Label("apos " + anchoredPos);
		Label("amax " + anchorMax);
		Label("amin " + anchorMin);
		Label("omax " + offsetMax);
		Label("omin " + offsetMin);
		Label("pivot " + pivot);
		Label("rect " + rect);
		Label("sizeDelta " + sizeDelta);
	}

}

#endif

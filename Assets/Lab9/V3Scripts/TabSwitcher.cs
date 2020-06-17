using UnityEngine;
using UnityEngine.UI;

namespace Lab9 {
	/// <summary> Small utility class to use to switch between various tabbed displays.</summary>
	public class TabSwitcher : MonoBehaviour {

		/// <summary> Children that can be swapped between </summary>
		Transform[] children;
		/// <summary> Object with the attached buttons </summary>
		public Transform tabBar;
		/// <summary> BG Color for enabled tab buttons </summary>
		public Color enabledColor = Color.white;
		/// <summary> BG Color for disabled tab buttons </summary>
		public Color disabledColor = new Color(.75f, .75f, .75f, 1.0f);
		/// <summary> Screen to show upon initialization </summary>
		public string initialScreen = "Inventory";

		void Start() {
			// Populate children array with attached children 
			int cnt = transform.childCount;
			children = new Transform[cnt];
			for (int i = 0; i < cnt; i++) {
				children[i] = transform.GetChild(i);
			}
			Show(initialScreen);
		}

		/// <summary> Intended to be called from UI events, changes the active child. </summary>
		/// <param name="name"> Name of child to show. </param>
		public void Show(string name) {
			// Loop over all children, only activating the ones with a matching name 
			foreach (var child in children) {
				child.gameObject.SetActive(name == child.name);
			}
			// Update the background colors for the tab bar buttons...
			if (tabBar != null) {
				// Go through tab bar's children
				for (int i = 0; i < tabBar.childCount; i++) {
					// See if there's an Image component attached...
					Image img = tabBar.GetChild(i).GetComponent<Image>();
					if (img != null) {
						// If there is, update its color if it's the button for the active child (WhateverButton).
						img.color = (img.gameObject.name == name+"Button") ? enabledColor : disabledColor;
					}
				}
			}
		}

	}
}

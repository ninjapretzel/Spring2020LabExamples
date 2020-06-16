using UnityEngine;
using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine.UI;
namespace Lab9 {
	/// <summary> One achievable version of the lab. </summary>
	public class Lab9V2 : MonoBehaviour {

		/// <summary> Object to toss labels under, for organizational purposes. </summary>
		public Transform labelParent;
		/// <summary> Camera to use for positioning labels. </summary>
		public Camera cam;
		/// <summary> Input for entering names </summary>
		public InputField nameEntry;
	
		/// <summary> Font to use for labels. </summary>
		public Font font;
		/// <summary> Size for labels </summary>
		public int fontSize = 24;
		/// <summary> Color for labels </summary>
		public Color labelColor = Color.red;
		/// <summary> 3d Offset for labels </summary>
		public Vector3 labelOffset = Vector3.up * 1.5f;
		
		/// <summary> Currently selected object </summary>
		public Transform currentSelection;
		/// <summary> Object to use to mark current selection </summary>
		public Image selectionMarker;
		/// <summary> Text to use to display object name of current selection </summary>
		public Text selectionDisplay;

		/// <summary> List of labels, parallel to <see cref="labeled"/></summary>
		public List<Text> labels;
		/// <summary> List of labels, parallel to <see cref="labels"/></summary>
		public List<Transform> labeled;

		
		
		/// <summary> Called by Unity every frame. </summary>
		void Update() {

			// Check for click...
			if (Input.GetMouseButtonDown(0)) {
				// Raycast to see if mouse is over something
				Ray ray = cam.ScreenPointToRay(Input.mousePosition);
				RaycastHit rayhit;
				if (Physics.Raycast(ray, out rayhit)) {
					// If it is, update selection
					currentSelection = rayhit.transform;
				}
			}
			
			// Update all labels...
			for (int i = labels.Count-1; i >= 0; i--) {
				Text label = labels[i];
				Transform target = labeled[i];
				// Remove any labels for deleted objects
				if (target == null) {
					Destroy(label);
					labels.RemoveAt(i);
					labeled.RemoveAt(i);
					continue;
				}

				// Otherwise, update the position.
				label.transform.position = cam.WorldToScreenPoint(labelOffset + target.position);

			}

			// Update selection marker
			if (selectionMarker != null) {
				selectionMarker.transform.position = currentSelection != null ? cam.WorldToScreenPoint(currentSelection.position) : new Vector3(0,-99999, 0);
			}

			// And update display 
			selectionDisplay.text = (currentSelection == null) ? "(None)" : currentSelection.gameObject.name;
		
		}

		/// <summary> Function called by toggle to set <see cref="labelColor"/> to red when checked </summary>
		/// <param name="isChecked"> True if the toggle was checked, false otherwise </param>
		public void SetColorRed(bool isChecked) { if (isChecked) { labelColor = Color.red; } }

		/// <summary> Function called by toggle to set <see cref="labelColor"/> to green when checked </summary>
		/// <param name="isChecked"> True if the toggle was checked, false otherwise </param>
		public void SetColorGreen(bool isChecked) { if (isChecked) { labelColor = Color.green; } }

		/// <summary> Function called by toggle to set <see cref="labelColor"/> to blue when checked </summary>
		/// <param name="isChecked"> True if the toggle was checked, false otherwise </param>
		public void SetColorBlue(bool isChecked) { if (isChecked) { labelColor = Color.blue; } }

		/// <summary> Function called when the user clicks the button. </summary>
		public void GiveItAName() {
			// Don't try to give null selection a label...
			if (currentSelection == null) { return; }

			// Otherwise loop over our current labels, and update the label if we already have labeled that object
			for (int i = labels.Count-1; i >= 0; i--) {
				Text label = labels[i];
				Transform target = labeled[i];
				if (target == currentSelection) {
					UpdateLabel(label);
					return;
				} 
			}
		
			// If we reach here, we don't already have a label, so we'll create one.
			GameObject textObj = new GameObject("Label");
			textObj.transform.SetParent(labelParent);
			Text display = textObj.AddComponent<Text>();
			// And add it into our lists...
			labels.Add(display);
			labeled.Add(currentSelection);
		
			// And then make it display the name, updating any other settings.
			UpdateLabel(display);

		}

		/// <summary> Common logic to apply changes (name, color, size, etc) to a label </summary>
		/// <param name="label"> Label object to update </param>
		public void UpdateLabel(Text label) {
			string name = nameEntry.text;

			label.transform.position = cam.WorldToScreenPoint(labelOffset + currentSelection.position);
			label.text = name;
			label.color = labelColor;
			label.font = font;
			label.fontSize = fontSize;
			label.horizontalOverflow = HorizontalWrapMode.Overflow;
			label.verticalOverflow = VerticalWrapMode.Overflow;
			label.alignment = TextAnchor.MiddleCenter;

		}

	}
}

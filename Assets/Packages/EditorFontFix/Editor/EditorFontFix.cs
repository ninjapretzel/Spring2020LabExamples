using UnityEngine;
using System;
using System.Collections;
using System.Collections.Generic;
using UnityEditor;
using System.Text;
using System.IO;

public class EditorFontFix  : EditorWindow {
	[MenuItem("Misc/Fix Ugly Font Tool")]
	public static void Fix() {
		EditorWindow.GetWindow<EditorFontFix>().Show();
	}

	public Font selected = null;
	public bool go = false;
	
	public void OnGUI() {

		StringBuilder str = new StringBuilder();
		void Fix(GUISkin skin) {
			Font f = selected;
			//Font bold = Resources.Load<Font>(f.name + "b");
			//Font italic = Resources.Load<Font>(f.name + "i");
			//Font boldItalic = Resources.Load<Font>(f.name + "z");
			//if (bold == null) { bold = f; }
			//if (italic == null) { italic = f; }
			//if (boldItalic == null) { boldItalic = f; }

			skin.font = f;
			skin.box.font = f;
			skin.label.font = f;
			skin.button.font = f;
			skin.horizontalSlider.font = f;
			skin.horizontalSliderThumb.font = f;
			skin.horizontalScrollbar.font = f;
			skin.horizontalScrollbarLeftButton.font = f;
			skin.horizontalScrollbarRightButton.font = f;
			skin.horizontalScrollbarThumb.font = f;
			skin.verticalSlider.font = f;
			skin.verticalSliderThumb.font = f;
			skin.verticalScrollbar.font = f;
			skin.verticalScrollbarUpButton.font = f;
			skin.verticalScrollbarDownButton.font = f;
			skin.verticalScrollbarThumb.font = f;
			skin.toggle.font = f;
			skin.scrollView.font = f;
			skin.textArea.font = f;
			skin.textField.font = f;
			skin.window.font = f;
			
			foreach (var s in skin.customStyles) {
				if (go) {
					str.Append($"Style {s} in {skin}\n");
				}

				if (s.name.ToLower().Contains("bold")) {
					s.fontStyle = FontStyle.Bold;
				}
				s.font = f;
			}
		}

		Vector2 size = minSize;
		size.y = 24;
		minSize = size;

		selected = (Font) EditorGUILayout.ObjectField("Font: ", selected, typeof(Font), false);
		go = GUILayout.Toggle(go,"Dump font styles to './dump.txt' ");

		if (selected != null) {

			GUISkin skin;
			// this method may only be called during OnGUI()...
			skin = EditorGUIUtility.GetBuiltinSkin(EditorSkin.Scene);
			Fix(skin);
			skin = EditorGUIUtility.GetBuiltinSkin(EditorSkin.Game);
			Fix(skin);
			skin = EditorGUIUtility.GetBuiltinSkin(EditorSkin.Inspector);
			Fix(skin);
		
		}

		if (go) {
			go = false;
			File.WriteAllText("Dump.txt", str.ToString());
			Debug.Log(str);
		}

	}



}

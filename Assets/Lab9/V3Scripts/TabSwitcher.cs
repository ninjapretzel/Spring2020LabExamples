using UnityEngine;
using UnityEngine.UI;

namespace Lab9 {
	public class TabSwitcher : MonoBehaviour {

		Transform[] children;
		public Transform tabBar;
		public Color enabledColor = Color.white;
		public Color disabledColor = new Color(.75f, .75f, .75f, 1.0f);
		public string initialScreen = "Inventory";

		void Start() {
			int cnt = transform.childCount;
			children = new Transform[cnt];
			for (int i = 0; i < cnt; i++) {
				children[i] = transform.GetChild(i);
			}
			Show(initialScreen);
		}

		public void Show(string name) {
			foreach (var child in children) {
				child.gameObject.SetActive(name == child.name);
			}
			if (tabBar != null) {
				for (int i = 0; i < tabBar.childCount; i++) {
					Image img = tabBar.GetChild(i).GetComponent<Image>();
					if (img != null) {
						img.color = (img.gameObject.name == name+"Button") ? enabledColor : disabledColor;
					}
				}
			}
		}

	}
}

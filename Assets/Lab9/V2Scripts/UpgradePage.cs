using UnityEngine;
using TMPro;
using UnityEngine.UI;

namespace Lab9 {
	public class UpgradePage : MonoBehaviour {

		public RectTransform slotPrefab;
		public Color enabledColor = Color.white;
		public Color disabledColor = Color.gray;

		public DerivativeClickerGame game;
		public TextMeshProUGUI[] nameTexts;
		public TextMeshProUGUI[] descTexts;
		public TextMeshProUGUI[] costTexts;
		public TextMeshProUGUI[] ownedTexts;
		public Image[] buttonImages;


		bool bought;

		void Start() {
			Rect pos = new Rect(0.01f, 0, .99f, 1f);
			// pos.width /= game.u.Length;
			pos.height /= game.upgrades.Length;

			/// Initialize arrays...
			int numUpgrades = game.upgrades.Length;
			nameTexts = new TextMeshProUGUI[numUpgrades];
			descTexts = new TextMeshProUGUI[numUpgrades];
			costTexts = new TextMeshProUGUI[numUpgrades];
			ownedTexts = new TextMeshProUGUI[numUpgrades];
			buttonImages = new Image[numUpgrades];



			for (int i = 0; i < game.upgrades.Length; i++) {
				RectTransform slot = Instantiate(slotPrefab);
				slot.SetParent(transform);
				Vector2 min = new Vector2(pos.x, 1.0f - pos.y - pos.height);
				Vector2 max = new Vector2(pos.x + pos.width, 1.0f - pos.y);
				slot.anchorMin = min;
				slot.anchorMax = max;
				slot.offsetMin = Vector2.zero;
				slot.offsetMax = Vector2.zero;
				pos.y += pos.height;

				Button button = slot.transform.Find("UpgradeButton").GetComponent<Button>();
				if (button != null) {
					int type = i;
					button.onClick.AddListener(() => {
						int times = 1;
						if (Input.GetKey(KeyCode.LeftShift)) { times = 10; }
						if (Input.GetKey(KeyCode.LeftControl)) { times = 100; }

						for (int j = 0; j < times; j++) {
							if (!game.TryBuyUpgrade(type)) { break; }
						}
						game.ticked = true;
						bought = true;
					});

				}
				descTexts[i] = slot.Find("Desc").GetComponent<TextMeshProUGUI>();
				nameTexts[i] = button.transform.Find("Name").GetComponent<TextMeshProUGUI>();
				costTexts[i] = button.transform.Find("Cost").GetComponent<TextMeshProUGUI>();
				ownedTexts[i] = button.transform.Find("Owned").GetComponent<TextMeshProUGUI>();
				buttonImages[i] = button.GetComponent<Image>();
			}
			UpdateLabels();
		}
		void OnEnable() {
			if (game.ready && nameTexts.Length > 0) {
				UpdateLabels();
			}
		}

		void LateUpdate() {
			if (game.ticked || bought) {
				UpdateLabels();
				bought = false;
			}
		}

		string Fmt(double value) { return game.Fmt(value); }
		string FmtInt(double value) { return game.FmtInt(value); }

		void UpdateLabels() {

			for (int i = 0; i < game.upgrades.Length; i++) {
				nameTexts[i].text = game.upgrades[i].name;
				double cost = game.NextUpgradeCost(i);
				costTexts[i].text = $"Costs: ${Fmt(cost)}";
				ownedTexts[i].text = $"Owned: ${FmtInt(game.state.upgradesPurchased[i])}";
				double power = game.UpgradePower(i);
				descTexts[i].text = string.Format(game.upgrades[i].desc, game.upgrades[i].power, power);

				buttonImages[i].color = game.state.cash >= cost ? enabledColor : disabledColor;
			}

		}

	}
}

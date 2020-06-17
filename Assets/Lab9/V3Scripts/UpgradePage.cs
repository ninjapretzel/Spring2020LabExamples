using UnityEngine;
using TMPro;
using UnityEngine.UI;

namespace Lab9 {

	/// <summary> Script controlling the Upgrade UI page </summary>
	public class UpgradePage : MonoBehaviour {
		/// <summary> Prefab to use when setting up UI </summary>
		public RectTransform slotPrefab;
		/// <summary> Enabled BG color for button images </summary>
		public Color enabledColor = Color.white;
		/// <summary> Disabled BG color for button images </summary>
		public Color disabledColor = Color.gray;
		
		/// <summary> Attached game data </summary>
		public DerivativeClickerGame game;
		/// <summary> display texts for names </summary>
		public TextMeshProUGUI[] nameTexts;
		/// <summary> display texts for descriptions </summary>
		public TextMeshProUGUI[] descTexts;
		/// <summary> display texts for costs </summary>
		public TextMeshProUGUI[] costTexts;
		/// <summary> display texts for owned counts </summary>
		public TextMeshProUGUI[] ownedTexts;
		/// <summary> display background images </summary>
		public Image[] buttonImages;

		/// <summary> Was an item bought this frame? </summary>
		bool bought;

		void Start() {
			// Position of next button (we move it around), in percentages.
			Rect brush = new Rect(0.01f, 0, .99f, 1f);
			// Each gets an equal space of the screen, based on how many upgrades are in-game.
			brush.height /= game.upgrades.Length;

			/// Initialize arrays...
			int numUpgrades = game.upgrades.Length;
			nameTexts = new TextMeshProUGUI[numUpgrades];
			descTexts = new TextMeshProUGUI[numUpgrades];
			costTexts = new TextMeshProUGUI[numUpgrades];
			ownedTexts = new TextMeshProUGUI[numUpgrades];
			buttonImages = new Image[numUpgrades];
			
			// Dynamically create all buttons...
			for (int i = 0; i < game.upgrades.Length; i++) {
				// Instantiate the prefab 
				RectTransform slot = Instantiate(slotPrefab);
				// Attach it to this object 
				slot.SetParent(transform);
				// Set the anchors
				Vector2 min = new Vector2(brush.x, 1.0f - brush.y - brush.height);
				Vector2 max = new Vector2(brush.x + brush.width, 1.0f - brush.y);
				slot.anchorMin = min;
				slot.anchorMax = max;
				// Reset the offsets... (thanks UGUI)
				slot.offsetMin = Vector2.zero;
				slot.offsetMax = Vector2.zero;
				// Move brush to next position
				brush.y += brush.height;

				// Grab UI components and store them in arrays
				Button button = slot.transform.Find("UpgradeButton").GetComponent<Button>();
				descTexts[i] = slot.Find("Desc").GetComponent<TextMeshProUGUI>();
				nameTexts[i] = button.transform.Find("Name").GetComponent<TextMeshProUGUI>();
				costTexts[i] = button.transform.Find("Cost").GetComponent<TextMeshProUGUI>();
				ownedTexts[i] = button.transform.Find("Owned").GetComponent<TextMeshProUGUI>();
				buttonImages[i] = button.GetComponent<Image>();

				// Prepare button
				if (button != null) {
					// Capture this into a new local variable for the callback to use,
					// otherwise the loop counter (i) will be outside the array...
					int type = i;
					// Add a click listener via code!
					// This is called an anonymous function (no name)
					// This syntax `=>` is often called a "lambda rocket"
					button.onClick.AddListener(() => {
						// Figure out how many to buy
						int times = 1;
						if (Input.GetKey(KeyCode.LeftShift)) { times = 10; }
						if (Input.GetKey(KeyCode.LeftControl)) { times = 100; }

						// Try to buy that many
						for (int j = 0; j < times; j++) {
							if (!game.TryBuyUpgrade(type)) { break; } // exit asap if we can't 
						}
						// Force us and anything else to update UI
						game.ticked = true;
						bought = true;
					});
				}
			}

			// When UI is all set up, initialize the labels
			UpdateLabels();
		}

		/// <summary> Called by Unity whenever this object is activated. </summary>
		void OnEnable() {
			// This can happen before the game is awake and before we have started.
			// Hence the game.ready field and the check for our own array's length.

			// this method is used to make it so that our player doesn't see
			// outdated values when they switch the active tab.
			if (game.ready && nameTexts.Length > 0) {
				UpdateLabels();
			}
		}

		/// <summary> Called by Unity every frame, after every object has had it's `Update` called. </summary>
		void LateUpdate() {
			// If we've bought something (or the game has otherwise ticked)
			if (game.ticked || bought) {
				// Update all labels and reset bought flag.
				UpdateLabels();
				bought = false;
			}
		}

		/// <summary> borrowed from <see cref="DerivativeClickerGame.Fmt(double)"/></summary>
		string Fmt(double value) { return game.Fmt(value); }
		/// <summary> borrowed from <see cref="DerivativeClickerGame.FmtInt(double)"/></summary>
		string FmtInt(double value) { return game.FmtInt(value); }

		/// <summary> Update all display labels on all buttons</summary>
		void UpdateLabels() {

			for (int i = 0; i < game.upgrades.Length; i++) {
				// Set name 
				nameTexts[i].text = game.upgrades[i].name;
				// Calculate and set cost 
				double cost = game.NextUpgradeCost(i);
				costTexts[i].text = $"Costs: ${Fmt(cost)}";
				// Set owned text 
				ownedTexts[i].text = $"Owned: ${FmtInt(game.state.upgradesPurchased[i])}";
				// Calculate current power and update description
				double power = game.UpgradePower(i);
				descTexts[i].text = string.Format(game.upgrades[i].desc, game.upgrades[i].power, power);

				// Update background color based on if the upgrade can be bought.
				buttonImages[i].color = (game.state.cash >= cost) ? enabledColor : disabledColor;
			}

		}

	}
}

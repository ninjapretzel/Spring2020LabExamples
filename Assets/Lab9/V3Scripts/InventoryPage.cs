using UnityEngine;
using TMPro;
using UnityEngine.UI;

namespace Lab9 {

	/// <summary> Script controlling the Inventory UI page </summary>
	public class InventoryPage : MonoBehaviour {
		/// <summary> Prefab to use when setting up UI </summary>
		public RectTransform buttonPrefab;
		/// <summary> Enabled BG color for button images </summary>
		public Color enabledColor = Color.white;
		/// <summary> Disabled BG color for button images </summary>
		public Color disabledColor = Color.gray;

		/// <summary> Attached game data </summary>
		public DerivativeClickerGame game;
		/// <summary> display texts for names </summary>
		public TextMeshProUGUI[][] nameTexts;
		/// <summary> display texts for costs </summary>
		public TextMeshProUGUI[][] costTexts;
		/// <summary> display texts for owned counts</summary>
		public TextMeshProUGUI[][] ownedTexts;
		/// <summary> display background images </summary>
		public Image[][] images;
		
		/// <summary> Was an item bought this frame?</summary>
		bool bought = false;
		
		void Start() {
			// Position of next button (we move it around), in percentages.
			Rect brush = new Rect(0, 0, 1f, 1f);
			// Each gets an equal space of the screen, based on how many buildings are in-game
			brush.width /= game.buildings.Length;
			brush.height /= game.buildings[0].Length;

			/// Initialize 2d arrays...
			int numBuildings = game.buildings.Length;
			int numTiers = game.buildings[0].Length;
			nameTexts = new TextMeshProUGUI[numBuildings][];
			costTexts = new TextMeshProUGUI[numBuildings][];
			ownedTexts = new TextMeshProUGUI[numBuildings][];
			images = new Image[numBuildings][];
			for (int i = 0; i < game.buildings.Length; i++) {
				nameTexts[i] = new TextMeshProUGUI[numTiers];
				costTexts[i] = new TextMeshProUGUI[numTiers];
				ownedTexts[i] = new TextMeshProUGUI[numTiers];
				images[i] = new Image[numTiers];
			}
			
			// Dynamically create all buttons...
			for (int i = 0; i < game.buildings.Length; i++) {
				for (int k = 0; k < game.buildings[i].Length; k++) {
					// Instantiate the prefab
					RectTransform button = Instantiate(buttonPrefab);
					// Attach it to this object
					button.SetParent(transform);
					// set the anchors
					Vector2 min = new Vector2(brush.x, 1.0f-brush.y - brush.height);
					Vector2 max = new Vector2(brush.x+brush.width, 1.0f-brush.y );
					button.anchorMin = min; 
					button.anchorMax = max; 
					// Reset the offsets... (thanks UGUI)
					button.offsetMin = Vector2.zero;
					button.offsetMax = Vector2.zero;
					// Move brush to next position.
					brush.y += brush.height;

					// Grab UI components and store them in arrays
					nameTexts[i][k] = button.Find("Name").GetComponent<TextMeshProUGUI>();
					costTexts[i][k] = button.Find("Cost").GetComponent<TextMeshProUGUI>();
					ownedTexts[i][k] = button.Find("Owned").GetComponent<TextMeshProUGUI>();
					images[i][k] = button.GetComponent<Image>();

					// Get and prepare button
					Button b = button.GetComponent<Button>();
					if (b != null) {
						// have to capture these into new local variables for the below callback to use
						// otherwise i/k will point outside of the arrays...
						int type = i;
						int tier = k;
						// Add a click listener via code!
						// This is called an anonymous function (no name)
						// This syntax `=>` is often called a "lambda rocket"
						b.onClick.AddListener( () => { 
							// Figure out how many to buy
							int times = 1;
							if (Input.GetKey(KeyCode.LeftShift)) { times = 10; }
							if (Input.GetKey(KeyCode.LeftControl)) { times = 100; }

							// Try to buy that many buildings 
							for (int j = 0; j < times; j++) {
								if (!game.TryBuyBuilding(type, tier)) { break; } // exit asap if we can't 
							}
							// Force us and anything else to update UI
							game.ticked = true;
							bought = true;
						} );
					}
					
				}
				// Reset brush position to top
				brush.y = 0;
				// and move to next column
				brush.x += brush.width;
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
			// Current cash/proof values
			double cash = game.state.cash;
			double proofs = game.state.proofs;

			// Loop over all buildings
			for (int i = 0; i < game.buildings.Length; i++) {
				for (int k = 0; k < game.buildings[i].Length; k++) {
					// Set name
					nameTexts[i][k].text = game.buildings[i][k].name;

					// Get all sorts of info to populate the cost/owned labels
					string power = (k > 0) ? FmtInt(game.BasePower(i, k)) : Fmt(game.BasePower(i, k));
					string currency = (i == DerivativeClickerGame.MATHS) ? "proofs" : "$$$";
					string rate = (i == DerivativeClickerGame.GRUNTS) ? "/ click" : "/ tick";
					double costAmt = game.NextBuildingCost(i, k);
					string costCurrency = (i == DerivativeClickerGame.COMPUTERS) ? "proofs" : "$$$";
					string cost = (costCurrency == "proofs") ? FmtInt(costAmt) : Fmt(costAmt);
					string total = game.FmtInt(game.TotalBuildings(i,k));
					string purchased = game.FmtInt(game.PurchasedBuildings(i,k));

					// Process info a little
					if (i == DerivativeClickerGame.WORKERS && k == 0) { currency = "faster ticks"; power = ""; rate = ""; }
					if (k > 0) { currency = game.buildings[i][k - 1].name; }
					if (k > 0) { rate = (i == DerivativeClickerGame.GRUNTS) ? $"/ {game.state.clicksPerBuildings} clicks" : $"/ {game.state.ticksPerBuildings} ticks"; }

					// Format cost/owned labels
					costTexts[i][k].text = string.Format("{0} {1} {2}\ncosts {3} {4}.", power, currency, rate, cost, costCurrency);
					ownedTexts[i][k].text = $"Owned: {total} ({purchased})";

					// Update background color based on if the building can be bought.
					if (costCurrency == "$$$") {
						images[i][k].color = (cash >= costAmt) ? enabledColor : disabledColor;
					} else {
						images[i][k].color = (proofs >= costAmt) ? enabledColor : disabledColor;
					}

				}
			}

		}

	}
}

using UnityEngine;
using TMPro;
using UnityEngine.UI;

namespace Lab9 {

	public class InventoryPage : MonoBehaviour {
		public RectTransform buttonPrefab;
		public Color enabledColor = Color.white;
		public Color disabledColor = Color.gray;


		public DerivativeClickerGame game;
		public TextMeshProUGUI[][] nameTexts;
		public TextMeshProUGUI[][] costTexts;
		public TextMeshProUGUI[][] ownedTexts;
		public Image[][] images;



		bool bought = false;


		void Start() {
			Rect pos = new Rect(0, 0, 1f, 1f);
			pos.width /= game.buildings.Length;
			pos.height /= game.buildings[0].Length;

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


			for (int i = 0; i < game.buildings.Length; i++) {
				for (int k = 0; k < game.buildings[i].Length; k++) {
					RectTransform button = Instantiate(buttonPrefab);
					button.SetParent(transform);
					Vector2 min = new Vector2(pos.x, 1.0f-pos.y - pos.height);
					Vector2 max = new Vector2(pos.x+pos.width, 1.0f-pos.y );
					button.anchorMin = min; 
					button.anchorMax = max; 
					button.offsetMin = Vector2.zero;
					button.offsetMax = Vector2.zero;
					pos.y += pos.height;

					Button b = button.GetComponent<Button>();
					if (b != null) {
						int type = i;
						int tier = k;
						b.onClick.AddListener( ()=> { 
							int times = 1;
							if (Input.GetKey(KeyCode.LeftShift)) { times = 10; }
							if (Input.GetKey(KeyCode.LeftControl)) { times = 100; }

							for (int j = 0; j < times; j++) {
								if (!game.TryBuyBuilding(type, tier)) { break; }
							}
							game.ticked = true;
							bought = true;
						} );
					}
				
					nameTexts[i][k] = button.Find("Name").GetComponent<TextMeshProUGUI>();
					costTexts[i][k] = button.Find("Cost").GetComponent<TextMeshProUGUI>();
					ownedTexts[i][k] = button.Find("Owned").GetComponent<TextMeshProUGUI>();
					images[i][k] = button.GetComponent<Image>();
				}
				pos.y = 0;
				pos.x += pos.width;
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
			double cash = game.state.cash;
			double proofs = game.state.proofs;

			for (int i = 0; i < game.buildings.Length; i++) {
				for (int k = 0; k < game.buildings[i].Length; k++) {
					nameTexts[i][k].text = game.buildings[i][k].name;

					string power = (k > 0) ? FmtInt(game.BasePower(i, k)) : Fmt(game.BasePower(i, k));
					string currency = (i == DerivativeClickerGame.MATHS) ? "proofs" : "$$$";
					string rate = (i == DerivativeClickerGame.GRUNTS) ? "/ click" : "/ tick";
					double costAmt = game.NextBuildingCost(i, k);
					string costCurrency = (i == DerivativeClickerGame.COMPUTERS) ? "proofs" : "$$$";
					string cost = (costCurrency == "proofs") ? FmtInt(costAmt) : Fmt(costAmt);

					if (i == DerivativeClickerGame.WORKERS && k == 0) { currency = "faster ticks"; power = ""; rate = ""; }
					if (k > 0) { currency = game.buildings[i][k - 1].name; }
					if (k > 0) { rate = (i == DerivativeClickerGame.GRUNTS) ? $"/ {game.state.clicksPerBuildings} clicks" : $"/ {game.state.ticksPerBuildings} ticks"; }

					string total = game.FmtInt(game.TotalBuildings(i,k));
					string purchased = game.FmtInt(game.PurchasedBuildings(i,k));
					costTexts[i][k].text = string.Format("{0} {1} {2}\ncosts {3} {4}.", power, currency, rate, cost, costCurrency);
					ownedTexts[i][k].text = $"Owned: {total} ({purchased})";

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

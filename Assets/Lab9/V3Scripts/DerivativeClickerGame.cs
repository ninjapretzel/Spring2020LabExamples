using UnityEngine;
using System;
using TMPro;
using System.Runtime.Serialization.Formatters.Binary;
using System.IO;

namespace Lab9 {

	/// <summary> Class holding primary logic for a fairly simple clicker game </summary>
	public class DerivativeClickerGame : MonoBehaviour {

		#region Save/Load helpers
		/// <summary> Performs a deep copy on a given object. </summary>
		/// <typeparam name="T">Generic type</typeparam>
		/// <param name="obj">Object to deep-copy</param>
		/// <returns>A deep copy of obj</returns>
		public static T DeepCopy<T>(T obj) {
			MemoryStream ms = new MemoryStream();
			BinaryFormatter bf = new BinaryFormatter();

			bf.Serialize(ms, obj);
			ms.Seek(0, SeekOrigin.Begin);
			T retval = (T)bf.Deserialize(ms);

			ms.Close();
			return retval;
		}

		/// <summary> Serializes data to base64 format string </summary>
		/// <typeparam name="T"> DataType to serialize </typeparam>
		/// <param name="obj"> Object to serialize </param>
		/// <returns> Serialized data, base64 encoded </returns>
		public static string SerializeToBase64<T>(T obj) {
			MemoryStream ms = new MemoryStream();
			BinaryFormatter bf = new BinaryFormatter();

			bf.Serialize(ms, obj);
			long length = ms.Position;
			if (length > int.MaxValue) { throw new Exception("Data too big to serialize!"); }

			ms.Seek(0, SeekOrigin.Begin);
			byte[] data = new byte[(int)length];
			ms.Read(data, 0, (int)length);
			ms.Close();
			return Convert.ToBase64String(data);
		}

		/// <summary> Deserializes data in base64 format </summary>
		/// <typeparam name="T"> DataType to deserialize </typeparam>
		/// <param name="base64"> string to deserialize </param>
		/// <returns> Deserialized data object </returns>
		public static T DeserializeFromBase64<T>(string base64) {
			byte[] data = Convert.FromBase64String(base64);
			MemoryStream ms = new MemoryStream();
			BinaryFormatter bf = new BinaryFormatter();

			ms.Write(data, 0, data.Length);
			ms.Seek(0, SeekOrigin.Begin);
			T retval = (T)bf.Deserialize(ms);
			ms.Close();
			return retval;
		}
		#endregion

		#region Formatting helpers
		/// <summary> Short names for various large numbers. </summary>
		static readonly string[] SHORT_NAMES = { "", "K", "M", "B", "T", "Qa", "Qt", "Sx", "Sp", "Oc", "No",
			"Dc", "UDc", "DDc", "TDc", "QaDc", "QtDc", "SxDc", "SpDc", "OcDc", "NoDc",
			"Vg", "UVg", "DVg", "TVg", "QaVg", "QtVg", "SxVg", "SpVg", "OcVg", "NoVg",
			"Qq", "UQq", "DQq", "TQq", "QaQq", "QtQq", "SxQq", "SpQq", "OcQq", "NoQq",
			"Sg", "USg", "DSg", "TSg", "QaSg", "QtSg", "SxSg", "SpSg", "OcSg", "NoSg",
			"St", "USt", "DSt", "TSt", "QaSt", "QtSt", "SxSt", "SpSt", "OcSt", "NoSt",
			"Ot", "UOt", "DOt", "TOt", "QaOt", "QtOt", "SxOt", "SpOt", "OcOt", "NoOt",
			"Nt", "UNt", "DNt", "TNt", "QaNt", "QtNt", "SxNt", "SpNt", "OcNt", "NoNt",
			"Ct", "UCt", "DCt", "TCt", "QaCt", "QtCt", "SxCt", "SpCt", "OcCt", "NoCt",
		};

		/// <summary> Format a large number, allowing floating precision at low values </summary>
		/// <param name="value"> Value to format </param>
		/// <returns> Value formatted with a short name and 2 decimal places. </returns>
		public string Fmt(double value) { return Format(value); }
		/// <summary> Format a large number, using integer precision at low values. </summary>
		/// <param name="value"> Value to format </param>
		/// <returns> Value formatted with a short name and up to 2 decimal places at larger values. </returns>
		public string FmtInt(double value) { return FormatInt(value); }
		/// <summary> Format a large number, allowing floating precision at low values </summary>
		/// <param name="value"> Value to format </param>
		/// <returns> Value formatted with a short name and 2 decimal places. </returns>
		public static string Format(double value) {
			if (value == 0) { return "0.00"; }
			if (value < 1.0) { return $"{value:F2}"; }

			double pow10 = (int)Math.Log10(value);
			int category = (int)(pow10 / 3);
			if (category >= 0 && category < SHORT_NAMES.Length) {
				double normalized = value / Math.Pow(10, category * 3);
				bool negative = category < 0;
				return $"{normalized:F2}{SHORT_NAMES[category]}{(negative ? "^-1" : "")}";
			}
			return $"{value:0.#####E+00}";
		}
		/// <summary> Format a large number, using integer precision at low values. </summary>
		/// <param name="value"> Value to format </param>
		/// <returns> Value formatted with a short name and up to 2 decimal places at larger values. </returns>
		public static string FormatInt(double value) {
			if (value == 0) { return "0"; }
			if (value < 1000) { return $"{(int)Math.Floor(value)}"; }

			double pow10 = (int)Math.Log10(value);
			int category = (int)(pow10 / 3);
			if (category >= 0 && category < SHORT_NAMES.Length) {
				double normalized = value / Math.Pow(10, category * 3);
				bool negative = category < 0;
				return $"{normalized:F2}{SHORT_NAMES[category]}{(negative ? "^-1" : "")}";
			}
			return $"{value:0.#####E+00}";
		}
		#endregion
		
		/// <summary> Class to store information about a building in the clicker game </summary>
		[Serializable] public class BuildingInfo {
			/// <summary> Display name </summary>
			public string name = "Default Building";
			/// <summary> Building power </summary>
			public double power = 1.00;
			/// <summary> Base cost for the first purchase </summary>
			public double baseCost = 1.00;
			/// <summary> Multiplier for cost past the first purchase </summary>
			public double costRate = 1.15;
		}
		/// <summary> Class to store information about an upgrade in the clicker game </summary>
		[Serializable] public class UpgradeInfo {
			/// <summary> Display name </summary>
			public string name = "Default Upgrade";
			/// <summary> Display description </summary>
			public string desc = "Boosts all whatever by {0} per whatever, each. Current multiplier: {1}";
			/// <summary> Base cost for the first purchase </summary>
			public double baseCost = 1.00;
			/// <summary> Multiplier for cost past the first purchase </summary>
			public double costRate = 10.00;
			/// <summary> Displayed power </summary>
			public double power = 1.0f;
			/// <summary> Conversion rate for power </summary>
			public double powerRate = 1.0f;
		}

		/// <summary> Class to store the current GameState information. </summary>
		[Serializable] public class GameState {
			/// <summary> Cash on hand </summary>
			public double cash = .1;
			/// <summary> Proofs on hand (separate currency) </summary>
			public double proofs = 0;
			/// <summary> Multiplier per manual click </summary>
			public int clickPower = 1;
			/// <summary> Multiplier per tick </summary>
			public int tickPower = 1;
			/// <summary> Speed of ticks </summary>
			public double tickRate = 1.00;
			/// <summary> milliseconds per tick </summary>
			public double msPerTick = 1000;

			/// <summary> Cash earned per autoclick </summary>
			public double cashPerAuto = 0;
			/// <summary> Cash earned per tick </summary>
			public double cashPerTick = 0;
			/// <summary> Net cash earned per tick (minus cost of proofs) </summary>
			public double netCashPerTick = 0;
			/// <summary> Cash earned per manual click</summary>
			public double cashPerClick = 1;
			/// <summary> Proofs earned per tick </summary>
			public double proofsPerTick = 0;
			/// <summary> Money spent per proof </summary>
			public double costPerProof = 5f;
			/// <summary> Current sub-click for gaining buildings </summary>
			public int midClick = 0;
			/// <summary> Current sub-tick for gaining buildings </summary>
			public int midTick = 0;
			/// <summary> Current sub-tick for autoclicks </summary>
			public int midAuto = 0;

			/// <summary> Clicks between getting more clicker buildings </summary>
			public int clicksPerBuildings = 25;
			/// <summary> Ticks between getting more tick buildings </summary>
			public int ticksPerBuildings = 10;

			/// <summary> Current upgrades purchased </summary>
			public int[] upgradesPurchased;
			/// <summary> Current buildings purchased </summary>
			public int[][] buildingsPurchased;
			/// <summary> Buildings earned from </summary>
			public double[][] buildingsEarned;
			/// <summary> Current multipliers </summary>
			public double[] multipliers;
		}

		/// <summary> Class to store UI object links</summary>
		[Serializable] public class UILinks {
			public TextMeshProUGUI moneyDisplay;
			public TextMeshProUGUI proofDisplay;
			public TextMeshProUGUI miscDisplay;
		}

		/// <summary> Is the game currently loaded/ready for other things to use? </summary>
		public bool ready = false;
		/// <summary> Current game state </summary>
		public GameState state;
		/// <summary> Initial game state. used to reset cleanly when wiping save. </summary>
		private GameState initialState;
		/// <summary> Hooked up UI </summary>
		public UILinks ui;
		/// <summary> Building category, index <see cref="DERIVATIVES"/> in <see cref="buildings"/>. </summary>
		public BuildingInfo[] derivatives;
		/// <summary> Building category, index <see cref="MATHS"/> in <see cref="buildings"/>. </summary>
		public BuildingInfo[] maths;
		/// <summary> Building category, index <see cref="COMPUTERS"/> in <see cref="buildings"/>. </summary>
		public BuildingInfo[] computers;
		/// <summary> Building category, index <see cref="GRUNTS"/> in <see cref="buildings"/>. </summary>
		public BuildingInfo[] grunts;
		/// <summary> Building category, index <see cref="WORKERS"/> in <see cref="buildings"/>. </summary>
		public BuildingInfo[] workers;
		/// <summary> All buildings by category. Unfortunately unity does not serialize arrays over dimension 1. </summary>
		public BuildingInfo[][] buildings;
		/// <summary> All upgrades. </summary>
		public UpgradeInfo[] upgrades;

		/// <summary> Constant for position of <see cref="derivatives"/> in <see cref="buildings"/>. </summary>
		public const int DERIVATIVES = 0;
		/// <summary> Constant for position of <see cref="maths"/> in <see cref="buildings"/>. </summary>
		public const int MATHS = 1;
		/// <summary> Constant for position of <see cref="computers"/> in <see cref="buildings"/>. </summary>
		public const int COMPUTERS = 2;
		/// <summary> Constant for position of <see cref="grunts"/> in <see cref="buildings"/>. </summary>
		public const int GRUNTS = 3;
		/// <summary> Constant for position of <see cref="workers"/> in <see cref="buildings"/>. </summary>
		public const int WORKERS = 4;

		/// <summary> Constant for position fixed position autoclickers upgrade in <see cref="upgrades"/>. </summary>
		public const int AUTOS = 0;
		/// <summary> Constant for position fixed position clickboost upgrade in <see cref="upgrades"/>. </summary>
		public const int CLICKBOOST = 1;
		/// <summary> Constant for number of fixed upgrades in <see cref="upgrades"/>. </summary>
		public const int FIXED_UPGRADES = 2;

		/// <summary> Current length of ticks </summary>
		public float tickLength = 1.00f;
		/// <summary> Current timeout for ticking </summary>
		public float tickTimeout = 0f;
		/// <summary> Current length for autosaving </summary>
		public float autosaveTime = 10f;
		/// <summary> Current timeout for autosaving </summary>
		public float autosaveTimeout = 0;
		/// <summary> Did the game tick this frame? </summary>
		public bool ticked = false;

		/// <summary> Save the game </summary>
		public void Save() {
			// PlayerPrefs is good for saving, but can only store ints float and string.
			// If we want to save more complex types, the best way is to turn them into
			// a string, and write that string to PlayerPrefs (or a file, if you want).
			string save = SerializeToBase64(state);
			PlayerPrefs.SetString("save", save);
		}

		/// <summary> Load the game from a save </summary>
		public void Load() {
			// Check if we have a savegame 
			if (PlayerPrefs.HasKey("save")) {
				try {
					// If we do, attempt to load it 
					string save = PlayerPrefs.GetString("save");
					state = DeserializeFromBase64<GameState>(save);

				} catch (Exception e) {
					// If loading fails for any reason, wipe the save.
					Debug.LogWarning("Error loading save: ");
					Debug.LogWarning(e);
					WipeSave();
				}
			}
		}

		/// <summary> Reset save data and start fresh </summary>
		public void WipeSave() {
			// If we have a save, remove it 
			if (PlayerPrefs.HasKey("save")) {
				PlayerPrefs.DeleteKey("save");
			}
			// And reset the state via copy.
			state = DeepCopy(initialState);
		}
		
		void Awake() {
			// Initialize 2d arrays and stuff, since unity won't serialize them ;.;
			// (or they are derived from other data)
			buildings = new BuildingInfo[][] { derivatives, maths, computers, grunts, workers };
			state.upgradesPurchased = new int[upgrades.Length];
			state.buildingsPurchased = new int[buildings.Length][];
			state.buildingsEarned = new double[buildings.Length][];
			for (int i = 0; i < buildings.Length; i++) {
				state.buildingsPurchased[i] = new int[buildings[i].Length];
				state.buildingsEarned[i] = new double[buildings[i].Length];
			}
			state.multipliers = new double[4];

			// Recalculate all stuffs
			Recalc();
			// Make a copy of the current state, as the state is now initialized completely.
			initialState = DeepCopy(state);

			// Then load any saved game
			Load();

			// Once all that is complete, game is ready to play. 
			ready = true;
		}

		void Start() {
			// Update labels once everything else has awoken
			// so they display correctly once the user sees the game.
			UpdateLabels();
		}

		void Update() {
			// Reset tick, and move timer forward.
			ticked = false;
			tickTimeout += Time.deltaTime;
			// If we tick...
			if (tickTimeout > tickLength) {
				tickTimeout -= tickLength;
				// Calculate up-to-date info
				Recalc();
				// Tick as many times as needed via multiplier 
				for (int i = 0; i < state.tickPower; i++) {
					Tick();
				}

				// Update display labels
				UpdateLabels();
				// Communicate tick to other scripts
				ticked = true;
			}

			// Update and check for autosave as well
			autosaveTimeout += Time.deltaTime;
			if (autosaveTimeout >= autosaveTime) {
				autosaveTimeout -= autosaveTime;
				Save();
			}

		}

		void Tick() {

			// Give cash
			state.cash += state.cashPerTick;
			// Check for enough cash for proofs 
			if (state.cash > state.proofsPerTick * state.costPerProof) {
				state.proofs += state.proofsPerTick;
				state.cash -= state.proofsPerTick * state.costPerProof;
			} else {
				// Otherwise make them buy as many as they possibly can
				double maxProofs = Math.Floor(state.cash / state.costPerProof);
				state.proofs += maxProofs;
				state.cash -= maxProofs * state.costPerProof;
			}

			// Update mid counters
			state.midTick++;
			state.midAuto++;

			// Give buildings if they have ticked enough 
			if (state.midTick >= state.ticksPerBuildings) {
				state.midTick = 0;
				YieldBuildings(DERIVATIVES);
				YieldBuildings(MATHS);
				YieldBuildings(COMPUTERS);
				YieldBuildings(WORKERS);
			}

			// Give autoclicks if they have ticked enough
			if (state.midAuto >= upgrades[AUTOS].power) {
				state.midAuto = 0;
				for (int i = 0; i < state.upgradesPurchased[AUTOS]; i++) {
					Clicked();
				}
			}

		}

		/// <summary> Give the player buildings of the given type, starting from lowest to highest tier. </summary>
		/// <param name="type"> Category of building to give </param>
		void YieldBuildings(int type) {
			var builds = buildings[type];
			for (int i = 1; i < builds.Length; i++) {
				state.buildingsEarned[type][i - 1] += Math.Floor(TotalPower(type, i));
			}
		}

		/// <summary> Callback for when the "Click to earn money" button is clicked. </summary>
		public void OnClick() {
			// Calculate up-to-date values.
			Recalc();

			// Click as many times as needed via multiplier 
			for (int i = 0; i < state.clickPower; i++) {
				// Call click logic
				Clicked();
			}

			// Update display labels
			UpdateLabels();
		}
		/// <summary> Logic to run for a single click </summary>
		void Clicked() {
			// Give cash 
			state.cash += state.cashPerClick;
			// Add mid counter
			state.midClick++;
			// Give buildings if they have clicked enough
			if (state.midClick >= state.clicksPerBuildings) {
				state.midClick = 0;
				YieldBuildings(GRUNTS);
			}
		}

		/// <summary> Attempt to purchase the specific building <paramref name="type"/> and <paramref name="tier"/>.</summary>
		/// <param name="type"> Type/Category of building to purchase </param>
		/// <param name="tier"> Tier power of building to purchase </param>
		/// <returns> True if purchase was successful, false otherwise. </returns>
		public bool TryBuyBuilding(int type, int tier) {
			// Get cost of the next building
			double cost = NextBuildingCost(type, tier);

			// Get on-hand currency used to purchase that kind of building
			double currency = (type == COMPUTERS) ? state.proofs : state.cash;
			if (type == COMPUTERS) { cost = Math.Floor(cost); }

			// If they have enough
			if (currency >= cost) {
				// update currency
				currency -= cost;
				// give them the building
				state.buildingsPurchased[type][tier] += 1;
				// put currency back correctly
				if (type == COMPUTERS) {
					state.proofs = currency;
				} else {
					state.cash = currency;
				}
				// Recalculate derived values, and update all labels. 
				Recalc();
				UpdateLabels();
				// Report success
				return true;
			}
			// Report failure
			return false;
		}

		/// <summary> Attempt to purchase the specific upgrade <paramref name="type"/>. </summary>
		/// <param name="type"> Type/index of upgrade to buy </param>
		/// <returns> True if purchase was successful, false otherwise. </returns>
		public bool TryBuyUpgrade(int type) {
			// Get cost of next upgrade
			double cost = NextUpgradeCost(type);

			// If they have enough cash
			if (state.cash >= cost) {
				// Take the cash
				state.cash -= cost;
				// give the upgrade
				state.upgradesPurchased[type] += 1;
				// Recalculate derived values, and update all labels. 
				Recalc();
				UpdateLabels();
				// Report success
				return true;
			}
			// Report failure
			return false;
		}
		
		/// <summary> Update UI labels with current information </summary>
		void UpdateLabels() {
			ui.moneyDisplay.text = $"${Format(state.cash)}\n${Format(state.cashPerTick)}\n${Format(state.netCashPerTick)}";

			ui.proofDisplay.text = $"{FormatInt(state.proofs)}\n{FormatInt(state.proofsPerTick)}\n${Format(state.costPerProof)}";

			ui.miscDisplay.text = $"Money/Click: ${Format(state.cashPerClick * UpgradePower(CLICKBOOST))}\nMoney/Auto: ${Format(state.cashPerAuto)}\nTick Length: {(int)state.msPerTick}ms @{FormatInt(state.tickPower)}x";
		}

		/// <summary> Gets the power of only purchased buildings. </summary>
		/// <param name="type"> Type of building </param>
		/// <param name="tier"> Tier of building </param>
		/// <returns> Power of only purchased buildings </returns>
		public double PurchasedPower(int type, int tier) {
			return state.buildingsPurchased[type][tier] * buildings[type][tier].power;
		}
		/// <summary> Gets the total raw power of a building. </summary>
		/// <param name="type"> Type of building </param>
		/// <param name="tier"> Tier of building </param>
		/// <returns> Total raw power of a building </returns>
		public double TotalPower(int type, int tier) {
			return (state.buildingsEarned[type][tier] + state.buildingsPurchased[type][tier]) * buildings[type][tier].power;
		}
		/// <summary> Gets the base power of a building </summary>
		/// <param name="type"> Type of building </param>
		/// <param name="tier"> Tier of building </param>
		/// <returns> Base power of a building </returns>
		public double BasePower(int type, int tier) {
			return buildings[type][tier].power * state.multipliers[tier];
		}
		/// <summary> Gets the power of an upgrade </summary>
		/// <param name="type"> Type/index of upgrade </param>
		/// <returns> Total power of upgrade. </returns>
		public double UpgradePower(int type) {
			// If this is a tiered upgrade
			if (type >= FIXED_UPGRADES) {
				// Tier boost, use buildings manually purchased in given tier
				int tier = type - FIXED_UPGRADES;
				// Sum purchased buildings
				int purchased = 0;
				for (int i = 0; i < buildings.Length; i++) {
					purchased += state.buildingsPurchased[i][tier];
				}
				// And apply purchased buildings to return value
				return 1.0 + purchased * state.upgradesPurchased[type] * upgrades[type].power * upgrades[type].powerRate;
			}
			// Otherwise, only purchased upgrades count
			return 1.0 + state.upgradesPurchased[type] * upgrades[type].power * upgrades[type].powerRate;
		}
		/// <summary> Gets the cost of the next building </summary>
		/// <param name="type"> Type of building </param>
		/// <param name="tier"> Tier of building </param>
		/// <returns> Cost of next building </returns>
		public double NextBuildingCost(int type, int tier) {
			return buildings[type][tier].baseCost * Math.Pow(buildings[type][tier].costRate, state.buildingsPurchased[type][tier]);
		}
		/// <summary> Gets the cost of the next upgrade </summary>
		/// <param name="type"> Type/index of upgrade </param>
		/// <returns> Cost of next upgrade. </returns>
		public double NextUpgradeCost(int type) {
			return upgrades[type].baseCost * Math.Pow(upgrades[type].costRate, state.upgradesPurchased[type]);
		}
		/// <summary> Gets the total count of a building. </summary>
		/// <param name="type"> Type of building </param>
		/// <param name="tier"> Tier of building </param>
		/// <returns> Total count of a building </returns>
		public double TotalBuildings(int type, int tier) {
			return state.buildingsPurchased[type][tier] + state.buildingsEarned[type][tier];
		}
		/// <summary> Gets the purchase count of a building. </summary>
		/// <param name="type"> Type of building </param>
		/// <param name="tier"> Tier of building </param>
		/// <returns> Purchase count of a building </returns>
		public double PurchasedBuildings(int type, int tier) {
			return state.buildingsPurchased[type][tier];
		}
		/// <summary> Gets the earned count of a building. </summary>
		/// <param name="type"> Type of building </param>
		/// <param name="tier"> Tier of building </param>
		/// <returns> Earned count of a building </returns>
		public double EarnedBuildings(int type, int tier) {
			return state.buildingsEarned[type][tier];
		}

		/// <summary> Recalculates all derived values for the game state </summary>
		void Recalc() {
			// Multipliers are calculated from upgrades
			for (int i = 0; i < state.multipliers.Length; i++) {
				state.multipliers[i] = UpgradePower(i + 2);
			}

			// Earnings
			state.proofsPerTick = Math.Floor((TotalPower(MATHS, 0)) * state.multipliers[0]);
			state.cashPerTick = (TotalPower(DERIVATIVES, 0) + TotalPower(COMPUTERS, 0)) * state.multipliers[0];
			state.cashPerClick = 1.0 + (TotalPower(GRUNTS, 0)) * state.multipliers[0];
			state.netCashPerTick = state.cashPerTick - state.proofsPerTick * state.costPerProof;
			
			// Tick speed and power 
			state.tickRate = 1.0 + Math.Log10(1 + .001 * state.multipliers[0] * TotalPower(WORKERS, 0));
			state.tickPower = 1;
			while (state.tickRate > 10) {
				state.tickRate /= 10;
				state.tickPower *= 10;
			}
			tickLength = 1.0f / (float)state.tickRate;
			state.msPerTick = (int)(1000f * tickLength);

			// Click/autoclick gains
			state.clickPower = (int)(UpgradePower(CLICKBOOST));
			state.cashPerAuto = state.upgradesPurchased[AUTOS] * state.cashPerClick;
		}

	}
}

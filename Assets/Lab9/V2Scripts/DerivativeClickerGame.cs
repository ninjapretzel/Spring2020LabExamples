using UnityEngine;
using System;
using TMPro;
using System.Runtime.Serialization.Formatters.Binary;
using System.IO;

namespace Lab9 {

	public class DerivativeClickerGame : MonoBehaviour {

		/// <summary> Performs a deep copy on a given object.</summary>
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

		public string Fmt(double value) { return Format(value); }
		public string FmtInt(double value) { return FormatInt(value); }
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
			return $"{value:0.###E+00}";
		}
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
			return $"{value:0.###E+00}";
		}

		[Serializable]
		public class BuildingInfo {
			public string name = "Default Building";
			public double power = 1.00;
			public double baseCost = 1.00;
			public double costRate = 1.15;
		}
		[Serializable]
		public class UpgradeInfo {
			public string name = "Default Upgrade";
			public string desc = "Boosts all whatever by {0} per whatever, each. Current multiplier: {1}";
			public double baseCost = 1.00;
			public double costRate = 10.00;
			public double power = 1.0f;
			public double powerRate = 1.0f;
		}

		[Serializable]
		public class GameState {
			public double cash = .1;
			public double proofs = 0;
			public double tickers = 0;
			public int clickPower = 1;
			public int tickPower = 1;
			public double tickRate = 1.00;

			public double cashPerAuto = 0;
			public double msPerTick = 1000;
			public double cashPerTick = 0;
			public double netCashPerTick = 0;
			public double cashPerClick = 1;
			public double proofsPerTick = 0;
			public double costPerProof = 5f;
			public int midClick = 0;
			public int midTick = 0;
			public int midAuto = 0;

			public int clicksPerBuildings = 25;
			public int ticksPerBuildings = 10;

			public int[] upgradesPurchased;
			public int[][] buildingsPurchased;
			public double[][] buildingsTotal;
			public double[] multipliers;
		}

		[Serializable]
		public class UILinks {
			public TextMeshProUGUI moneyDisplay;
			public TextMeshProUGUI proofDisplay;
			public TextMeshProUGUI miscDisplay;
		}

		public bool ready = false;
		public GameState state;
		private GameState initialState;
		public UILinks ui;
		public BuildingInfo[] derivatives;
		public BuildingInfo[] maths;
		public BuildingInfo[] computers;
		public BuildingInfo[] grunts;
		public BuildingInfo[] workers;
		public BuildingInfo[][] buildings;

		public UpgradeInfo[] upgrades;

		public const int DERIVATIVES = 0;
		public const int MATHS = 1;
		public const int COMPUTERS = 2;
		public const int GRUNTS = 3;
		public const int WORKERS = 4;

		public const int AUTOS = 0;
		public const int CLICKBOOST = 1;
		public const int FIXED_UPGRADES = 2;


		public float tickLength = 1.00f;
		public float timeout = 0f;
		public float autosaveTime = 10f;
		public float autosaveTimeout = 0;
		public bool ticked = false;

		public void Save() {
			string save = SerializeToBase64(state);
			PlayerPrefs.SetString("save", save);
		}

		public void Load() {
			if (PlayerPrefs.HasKey("save")) {
				string save = PlayerPrefs.GetString("save");
				state = DeserializeFromBase64<GameState>(save);
			}
		}

		public void WipeSave() {
			if (PlayerPrefs.HasKey("save")) {
				PlayerPrefs.DeleteKey("save");
			}
			state = DeepCopy(initialState);
		}
		
		void Awake() {
			// Initialize 2d arrays and stuff, since unity won't serialize them ;.;
			// (or they are derived from other data)
			buildings = new BuildingInfo[][] { derivatives, maths, computers, grunts, workers };
			state.upgradesPurchased = new int[upgrades.Length];
			state.buildingsPurchased = new int[buildings.Length][];
			state.buildingsTotal = new double[buildings.Length][];
			for (int i = 0; i < buildings.Length; i++) {
				state.buildingsPurchased[i] = new int[buildings[i].Length];
				state.buildingsTotal[i] = new double[buildings[i].Length];
			}
			state.multipliers = new double[4];
			Recalc();
			initialState = DeepCopy(state);

			Load();
			ready = true;
		}

		void Start() {
			UpdateLabels();
		}

		void Update() {
			timeout += Time.deltaTime;
			ticked = false;
			if (timeout > tickLength) {
				timeout -= tickLength;
				Recalc();

				for (int i = 0; i < state.tickPower; i++) {
					Tick();
				}

				UpdateLabels();
				ticked = true;
			}

			autosaveTimeout += Time.deltaTime;
			if (autosaveTimeout >= autosaveTime) {
				autosaveTimeout -= autosaveTime;
				Save();
			}

		}

		void Tick() {

			state.cash += state.cashPerTick;
			if (state.cash > state.proofsPerTick * state.costPerProof) {
				state.proofs += state.proofsPerTick;
				state.cash -= state.proofsPerTick * state.costPerProof;
			} else {
				double maxProofs = Math.Floor(state.cash / state.costPerProof);
				state.proofs += maxProofs;
				state.cash -= maxProofs * state.costPerProof;
			}

			state.midTick++;
			state.midAuto++;

			if (state.midTick >= state.ticksPerBuildings) {
				state.midTick = 0;
				YieldBuildings(DERIVATIVES);
				YieldBuildings(MATHS);
				YieldBuildings(COMPUTERS);
				YieldBuildings(WORKERS);
			}

			if (state.midAuto >= upgrades[AUTOS].power) {
				state.midAuto = 0;
				for (int i = 0; i < state.upgradesPurchased[AUTOS]; i++) {
					Clicked();
				}
			}

		}

		void YieldBuildings(int type) {
			var builds = buildings[type];
			for (int i = 1; i < builds.Length; i++) {
				state.buildingsTotal[type][i - 1] += Math.Floor(RawPower(type, i));
			}
		}

		public void OnClick() {
			Recalc();

			for (int i = 0; i < state.clickPower; i++) {
				Clicked();
			}

			UpdateLabels();
		}
		void Clicked() {
			state.cash += state.cashPerClick;
			state.midClick++;
			if (state.midClick >= state.clicksPerBuildings) {
				state.midClick = 0;
				YieldBuildings(GRUNTS);
			}
		}

		public bool TryBuyBuilding(int type, int tier) {
			double cost = NextBuildingCost(type, tier);

			double currency = (type == COMPUTERS) ? state.proofs : state.cash;
			if (type == COMPUTERS) { cost = Math.Floor(cost); }


			if (currency >= cost) {
				currency -= cost;
				state.buildingsPurchased[type][tier] += 1;
				if (type == COMPUTERS) {
					state.proofs = currency;
				} else {
					state.cash = currency;
				}
				Recalc();
				UpdateLabels();
				return true;
			}
			return false;
		}

		public bool TryBuyUpgrade(int type) {
			double cost = NextUpgradeCost(type);

			if (state.cash >= cost) {
				state.cash -= cost;
				state.upgradesPurchased[type] += 1;
				Recalc();
				UpdateLabels();
				return true;
			}

			return false;
		}




		void UpdateLabels() {

			ui.moneyDisplay.text = $"${Format(state.cash)}\n${Format(state.cashPerTick)}\n${Format(state.netCashPerTick)}";

			ui.proofDisplay.text = $"{FormatInt(state.proofs)}\n{FormatInt(state.proofsPerTick)}\n${Format(state.costPerProof)}";

			ui.miscDisplay.text = $"Money/Click: ${Format(state.cashPerClick * UpgradePower(CLICKBOOST))}\nMoney/Auto: ${Format(state.cashPerAuto)}\nTick Length: {(int)state.msPerTick}ms @{FormatInt(state.tickPower)}x";
		}


		public double PurchasedPower(int type, int tier) {
			return state.buildingsPurchased[type][tier] * buildings[type][tier].power;
		}
		public double RawPower(int type, int tier) {
			return (state.buildingsTotal[type][tier] + state.buildingsPurchased[type][tier]) * buildings[type][tier].power;
		}
		public double BasePower(int type, int tier) {
			return buildings[type][tier].power * state.multipliers[tier];
		}
		public double UpgradePower(int type) {
			if (type >= FIXED_UPGRADES) {
				// Tier boost, use buildings manually purchased in given tier
				int tier = type - FIXED_UPGRADES;
				int purchased = 0;
				for (int i = 0; i < buildings.Length; i++) {
					purchased += state.buildingsPurchased[i][tier];
				}

				return 1.0 + purchased * state.upgradesPurchased[type] * upgrades[type].power * upgrades[type].powerRate;
			}
			return 1.0 + state.upgradesPurchased[type] * upgrades[type].power * upgrades[type].powerRate;
		}
		public double NextBuildingCost(int type, int tier) {
			return buildings[type][tier].baseCost * Math.Pow(buildings[type][tier].costRate, state.buildingsPurchased[type][tier]);
		}
		public double NextUpgradeCost(int type) {
			return upgrades[type].baseCost * Math.Pow(upgrades[type].costRate, state.upgradesPurchased[type]);
		}

		public double TotalBuildings(int type, int tier) {
			return state.buildingsPurchased[type][tier] + state.buildingsTotal[type][tier];
		}
		public double PurchasedBuildings(int type, int tier) {
			return state.buildingsPurchased[type][tier];
		}
		public double EarnedBuildings(int type, int tier) {
			return state.buildingsTotal[type][tier];
		}

		void Recalc() {
			for (int i = 0; i < state.multipliers.Length; i++) {
				state.multipliers[i] = UpgradePower(i + 2);
			}

			state.proofsPerTick = Math.Floor((RawPower(MATHS, 0)) * state.multipliers[0]);
			state.cashPerTick = (RawPower(DERIVATIVES, 0) + RawPower(COMPUTERS, 0)) * state.multipliers[0];
			state.cashPerClick = 1.0 + (RawPower(GRUNTS, 0)) * state.multipliers[0];

			state.netCashPerTick = state.cashPerTick - state.proofsPerTick * state.costPerProof;

			state.tickRate = 1.0 + Math.Log10(1 + .001 * state.multipliers[0] * RawPower(WORKERS, 0));
			state.tickPower = 1;
			while (state.tickRate > 10) {
				state.tickRate /= 10;
				state.tickPower *= 10;
			}

			tickLength = 1.0f / (float)state.tickRate;
			state.msPerTick = (int)(1000f * tickLength);

			state.clickPower = (int)(UpgradePower(CLICKBOOST));
			state.cashPerAuto = state.upgradesPurchased[AUTOS] * state.cashPerClick;
			// state.msPerTick =
		}

	}
}

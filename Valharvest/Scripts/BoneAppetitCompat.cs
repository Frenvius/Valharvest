using BepInEx.Bootstrap;
using Jotunn;
using Jotunn.Configs;

namespace Valharvest;

public static class BoneAppetitCompat {
	private static bool? _isInstalled;

	public static bool IsInstalled {
		get {
			if (!_isInstalled.HasValue) {
				_isInstalled = Chainloader.PluginInfos.ContainsKey("com.rockerkitten.boneappetit");
				Logger.LogInfo($"BoneAppetit detection: {(_isInstalled.Value ? "INSTALLED" : "NOT FOUND")}");
			}

			return _isInstalled.Value;
		}
	}

	public static string GetEggItem() {
		return IsInstalled ? "rk_egg" : "vh_egg";
	}

	public static string GetPorkItem() {
		return IsInstalled ? "rk_pork" : "RawMeat";
	}

	public static string GetCookingStation(string stationName, bool useBAStations = true) {
		if (IsInstalled && useBAStations) return stationName;

		switch (stationName) {
			case "rk_prep":
				if (Configurations.Valharvest.UseVanillaPrepTable.Value)
					return "piece_preptable";
				return "piece_prep_table";
			case "rk_griddle":
			case "rk_grill":
				return "piece_cooking_pot";
			default:
				return stationName;
		}
	}

	public static void RemapIngredients(RequirementConfig[] requirements) {
		if (IsInstalled || requirements == null) return;

		for (int i = 0; i < requirements.Length; i++)
			if (requirements[i].Item == "rk_egg")
				requirements[i].Item = "vh_egg";
			else if (requirements[i].Item == "rk_pork") requirements[i].Item = "RawMeat";
	}
}

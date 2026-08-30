using Jotunn.Managers;
using UnityEngine;
using Logger = Jotunn.Logger;

namespace Valharvest.Scripts;

public static class PrepTableRecipeCopier {
	private static bool _redirected;


	public static void CopyVanillaPrepTableRecipes() {
		if (_redirected) return;

		if (BoneAppetitCompat.IsInstalled && Configurations.Valharvest.UseBoneAppetitCookingStations.Value) {
			Logger.LogInfo("PrepTableRecipeCopier: Skipped (using BoneAppetit stations)");
			PrefabManager.OnPrefabsRegistered -= CopyVanillaPrepTableRecipes;
			return;
		}

		if (Configurations.Valharvest.UseVanillaPrepTable.Value) {
			Logger.LogInfo("PrepTableRecipeCopier: Skipped (using vanilla prep table)");
			PrefabManager.OnPrefabsRegistered -= CopyVanillaPrepTableRecipes;
			return;
		}

		GameObject vanillaPrepPrefab = PrefabManager.Instance.GetPrefab("piece_preptable");
		GameObject customPrepPrefab = PrefabManager.Instance.GetPrefab("piece_prep_table");

		if (vanillaPrepPrefab == null || customPrepPrefab == null) {
			Logger.LogWarning("PrepTableRecipeCopier: Could not find prep table prefabs");
			PrefabManager.OnPrefabsRegistered -= CopyVanillaPrepTableRecipes;
			return;
		}

		var vanillaStation = vanillaPrepPrefab.GetComponent<CraftingStation>();
		var customStation = customPrepPrefab.GetComponent<CraftingStation>();

		if (vanillaStation == null || customStation == null) {
			Logger.LogWarning("PrepTableRecipeCopier: Missing CraftingStation component");
			PrefabManager.OnPrefabsRegistered -= CopyVanillaPrepTableRecipes;
			return;
		}

		int modifiedCount = 0;

		foreach (Recipe recipe in ObjectDB.instance.m_recipes) {
			if (recipe == null || recipe.m_craftingStation != vanillaStation)
				continue;
			recipe.m_craftingStation = customStation;
			recipe.m_minStationLevel = 2;
			modifiedCount++;
		}

		_redirected = true;
		Logger.LogInfo($"PrepTableRecipeCopier: Redirected {modifiedCount} recipes to custom prep table");

		PrefabManager.OnPrefabsRegistered -= CopyVanillaPrepTableRecipes;
	}
}

using System;
using System.Collections.Generic;
using System.IO;
using SimpleJson;
using UnityEngine;
using static Valharvest.Utils;
using Logger = Jotunn.Logger;
using Object = UnityEngine.Object;

namespace Valharvest.Scripts;

public static class ConsumableItemExtractor {
	private static HashSet<string> GetValharvestPrefabNames() {
		var names = new HashSet<string>();

		try {
			string itemsJson = ReadEmbeddedFile("items.resources");
			var items = (JsonObject)SimpleJson.SimpleJson.DeserializeObject(itemsJson);
			foreach (string key in items.Keys)
				names.Add(key);
		} catch {
		}

		try {
			string foodsJson = ReadEmbeddedFile("newFoodsConfig.resources");
			var foods = (JsonObject)SimpleJson.SimpleJson.DeserializeObject(foodsJson);
			foreach (string key in foods.Keys)
				names.Add(key);
		} catch {
		}

		try {
			string plantsJson = ReadEmbeddedFile("plants.resources");
			var plants = (JsonObject)SimpleJson.SimpleJson.DeserializeObject(plantsJson);
			foreach (string key in plants.Keys)
				names.Add(key);
		} catch {
		}

		return names;
	}

	public static void GenerateConsumableItemList() {
		Logger.LogInfo("Starting item extraction...");

		if (!ObjectDB.instance) {
			Logger.LogError("ObjectDB not available. Make sure you're in-game.");
			return;
		}

		JsonArray itemData = ExtractAllItems();
		File.WriteAllText("itemdrops.json", itemData.ToString());
		Logger.LogInfo($"Wrote itemdrops.json with {itemData.Count} items");

		JsonArray recipeData = ExtractAllRecipes();
		File.WriteAllText("recipes.json", recipeData.ToString());
		Logger.LogInfo($"Wrote recipes.json with {recipeData.Count} recipes");

		Logger.LogInfo("Item extraction complete!");
	}

	private static JsonArray ExtractAllItems() {
		var itemData = new JsonArray();
		var processedNames = new HashSet<string>();
		HashSet<string> valharvestItems = GetValharvestPrefabNames();

		string iconDir = "icons";
		if (!Directory.Exists(iconDir))
			Directory.CreateDirectory(iconDir);

		int iconCount = 0;

		foreach (ItemDrop.ItemData.ItemType itemType in Enum.GetValues(typeof(ItemDrop.ItemData.ItemType))) {
			List<ItemDrop> items = ObjectDB.instance.GetAllItems(itemType, "");

			foreach (ItemDrop item in items) {
				if (item == null || item.m_itemData == null || item.m_itemData.m_shared == null)
					continue;

				string prefabName = item.gameObject.name;

				if (processedNames.Contains(prefabName))
					continue;

				if (valharvestItems.Contains(prefabName))
					continue;

				processedNames.Add(prefabName);

				var itemJson = new JsonObject();
				ItemDrop.ItemData.SharedData shared = item.m_itemData.m_shared;

				itemJson.Add("var_name", shared.m_name ?? "");
				itemJson.Add("raw_name", Localization.instance.Localize(shared.m_name ?? ""));
				itemJson.Add("true_name", prefabName);

				var sharedJson = new JsonObject();

				sharedJson.Add("raw_name", Localization.instance.Localize(shared.m_name ?? ""));
				sharedJson.Add("var_name", shared.m_name ?? "");
				sharedJson.Add("item_type_name", shared.m_itemType.ToString());
				sharedJson.Add("item_type", (int)shared.m_itemType);
				sharedJson.Add("description", Localization.instance.Localize(shared.m_description ?? ""));
				sharedJson.Add("prefab_name", prefabName);

				sharedJson.Add("food", shared.m_food);
				sharedJson.Add("food_burn_time", shared.m_foodBurnTime);
				sharedJson.Add("food_regen", shared.m_foodRegen);
				sharedJson.Add("food_stamina", shared.m_foodStamina);

				sharedJson.Add("armor", shared.m_armor);
				sharedJson.Add("armor_per_level", shared.m_armorPerLevel);
				sharedJson.Add("attack_force", shared.m_attackForce);
				sharedJson.Add("backstab_bonus", shared.m_backstabBonus);
				sharedJson.Add("block_power", shared.m_blockPower);
				sharedJson.Add("block_power_per_level", shared.m_blockPowerPerLevel);
				sharedJson.Add("deflection_force", shared.m_deflectionForce);
				sharedJson.Add("deflection_force_per_level", shared.m_deflectionForcePerLevel);

				sharedJson.Add("ai_attack_interval", shared.m_aiAttackInterval);
				sharedJson.Add("ai_attack_max_angle", shared.m_aiAttackMaxAngle);
				sharedJson.Add("ai_attack_range", shared.m_aiAttackRange);
				sharedJson.Add("ai_attack_range_min", shared.m_aiAttackRangeMin);
				sharedJson.Add("ai_prioritized", shared.m_aiPrioritized);
				sharedJson.Add("ai_target_type", shared.m_aiTargetType.ToString());
				sharedJson.Add("ai_when_flying", shared.m_aiWhenFlying);
				sharedJson.Add("ai_when_swiming", shared.m_aiWhenSwiming);
				sharedJson.Add("ai_when_walking", shared.m_aiWhenWalking);

				sharedJson.Add("animation_state", shared.m_animationState.ToString());
				sharedJson.Add("blockable", shared.m_blockable);
				sharedJson.Add("can_be_reparied", shared.m_canBeReparied);
				sharedJson.Add("destroy_broken", shared.m_destroyBroken);
				sharedJson.Add("dodgeable", shared.m_dodgeable);
				sharedJson.Add("durability_drain", shared.m_durabilityDrain);
				sharedJson.Add("durability_per_level", shared.m_durabilityPerLevel);
				sharedJson.Add("equip_duration", shared.m_equipDuration);
				sharedJson.Add("helmet_hide_hair", shared.m_helmetHideHair.ToString());
				sharedJson.Add("max_durability", shared.m_maxDurability);
				sharedJson.Add("max_quality", shared.m_maxQuality);
				sharedJson.Add("max_stack_size", shared.m_maxStackSize);
				sharedJson.Add("movement_modifier", shared.m_movementModifier);
				sharedJson.Add("quest_item", shared.m_questItem);
				sharedJson.Add("set_size", shared.m_setSize);
				sharedJson.Add("teleportable", shared.m_teleportable);
				sharedJson.Add("timed_block_bonus", shared.m_timedBlockBonus);
				sharedJson.Add("tool_tier", shared.m_toolTier);
				sharedJson.Add("use_durability", shared.m_useDurability);
				sharedJson.Add("use_durability_drain", shared.m_useDurabilityDrain);
				sharedJson.Add("value", shared.m_value);
				sharedJson.Add("variants", shared.m_variants);
				sharedJson.Add("weight", shared.m_weight);
				sharedJson.Add("ammo_type", shared.m_ammoType ?? "");

				sharedJson.Add("skill_type", shared.m_skillType.ToString());

				sharedJson.Add("damages", ExtractDamageData(shared.m_damages));
				sharedJson.Add("damages_per_level", ExtractDamageDataWithPrefix(shared.m_damagesPerLevel));

				sharedJson.Add("status_effects", ExtractStatusEffects(shared));

				sharedJson.Add("set_name", shared.m_setName ?? "");
				sharedJson.Add("set_status_effect", shared.m_setStatusEffect != null ? shared.m_setStatusEffect.name : "");

				sharedJson.Add("food_eitr", shared.m_foodEitr);

				if (shared.m_icons != null && shared.m_icons.Length > 0 && shared.m_icons[0] != null) {
					sharedJson.Add("icon", $"{prefabName}.png");
					if (ExportItemIcon(shared, prefabName, iconDir))
						iconCount++;
				}

				itemJson.Add("shared_data", sharedJson);
				itemData.Add(itemJson);
			}
		}

		Logger.LogInfo($"Exported {iconCount} icons to {iconDir}/");
		return itemData;
	}

	private static JsonArray ExtractAllRecipes() {
		var recipeData = new JsonArray();
		HashSet<string> valharvestItems = GetValharvestPrefabNames();

		if (ObjectDB.instance.m_recipes == null)
			return recipeData;

		foreach (Recipe recipe in ObjectDB.instance.m_recipes) {
			if (recipe == null || recipe.m_item == null)
				continue;

			string itemPrefabName = recipe.m_item.gameObject.name;
			if (valharvestItems.Contains(itemPrefabName))
				continue;

			var recipeJson = new JsonObject();
			ItemDrop.ItemData.SharedData itemShared = recipe.m_item.m_itemData.m_shared;

			recipeJson.Add("raw_name", Localization.instance.Localize(itemShared.m_name ?? ""));
			recipeJson.Add("var_name", itemShared.m_name ?? "");
			recipeJson.Add("true_name", recipe.name ?? "");
			recipeJson.Add("enabled", recipe.m_enabled);
			recipeJson.Add("min_station_level", recipe.m_minStationLevel);
			recipeJson.Add("amount", recipe.m_amount);

			if (recipe.m_craftingStation != null) {
				recipeJson.Add("raw_crafting_station_name",
					Localization.instance.Localize(recipe.m_craftingStation.m_name ?? ""));
				recipeJson.Add("true_crafting_station_name", recipe.m_craftingStation.name ?? "");
			} else {
				recipeJson.Add("raw_crafting_station_name", "");
				recipeJson.Add("true_crafting_station_name", "");
			}

			var requirements = new JsonArray();
			if (recipe.m_resources != null)
				foreach (Piece.Requirement req in recipe.m_resources) {
					if (req == null || req.m_resItem == null)
						continue;

					var reqJson = new JsonObject();
					ItemDrop.ItemData.SharedData reqShared = req.m_resItem.m_itemData.m_shared;

					reqJson.Add("amount", req.m_amount);
					reqJson.Add("amount_per_level", req.m_amountPerLevel);
					reqJson.Add("raw_name", Localization.instance.Localize(reqShared.m_name ?? ""));
					reqJson.Add("var_name", reqShared.m_name ?? "");
					reqJson.Add("true_name", req.m_resItem.gameObject.name ?? "");
					reqJson.Add("recover", req.m_recover);

					requirements.Add(reqJson);
				}

			recipeJson.Add("requirements", requirements);

			recipeData.Add(recipeJson);
		}

		return recipeData;
	}

	private static JsonObject ExtractDamageData(HitData.DamageTypes damages) {
		var damageJson = new JsonObject();
		damageJson.Add("damage", damages.m_damage);
		damageJson.Add("blunt", damages.m_blunt);
		damageJson.Add("slash", damages.m_slash);
		damageJson.Add("pierce", damages.m_pierce);
		damageJson.Add("chop", damages.m_chop);
		damageJson.Add("pickaxe", damages.m_pickaxe);
		damageJson.Add("fire", damages.m_fire);
		damageJson.Add("frost", damages.m_frost);
		damageJson.Add("lightning", damages.m_lightning);
		damageJson.Add("poison", damages.m_poison);
		damageJson.Add("spirit", damages.m_spirit);
		return damageJson;
	}

	private static JsonObject ExtractDamageDataWithPrefix(HitData.DamageTypes damages) {
		var damageJson = new JsonObject();
		damageJson.Add("m_damage", damages.m_damage);
		damageJson.Add("m_blunt", damages.m_blunt);
		damageJson.Add("m_slash", damages.m_slash);
		damageJson.Add("m_pierce", damages.m_pierce);
		damageJson.Add("m_chop", damages.m_chop);
		damageJson.Add("m_pickaxe", damages.m_pickaxe);
		damageJson.Add("m_fire", damages.m_fire);
		damageJson.Add("m_frost", damages.m_frost);
		damageJson.Add("m_lightning", damages.m_lightning);
		damageJson.Add("m_poison", damages.m_poison);
		damageJson.Add("m_spirit", damages.m_spirit);
		return damageJson;
	}

	private static JsonArray ExtractStatusEffects(ItemDrop.ItemData.SharedData shared) {
		var effects = new JsonArray();

		if (shared.m_attackStatusEffect != null)
			effects.Add(shared.m_attackStatusEffect.name);

		if (shared.m_consumeStatusEffect != null)
			effects.Add(shared.m_consumeStatusEffect.name);

		if (shared.m_equipStatusEffect != null)
			effects.Add(shared.m_equipStatusEffect.name);

		if (shared.m_setStatusEffect != null)
			effects.Add(shared.m_setStatusEffect.name);

		return effects;
	}

	private static bool ExportItemIcon(ItemDrop.ItemData.SharedData shared, string prefabName, string outputDir) {
		if (shared.m_icons == null || shared.m_icons.Length == 0)
			return false;

		Sprite sprite = shared.m_icons[0];
		if (sprite == null || sprite.texture == null)
			return false;

		try {
			Texture2D readableTexture = MakeTextureReadable(sprite);
			if (readableTexture == null)
				return false;

			byte[] pngData = readableTexture.EncodeToPNG();
			string filePath = Path.Combine(outputDir, $"{prefabName}.png");
			File.WriteAllBytes(filePath, pngData);

			Object.Destroy(readableTexture);
			return true;
		} catch (Exception ex) {
			Logger.LogWarning($"Failed to export icon for {prefabName}: {ex.Message}");
			return false;
		}
	}

	private static Texture2D MakeTextureReadable(Sprite sprite) {
		Texture2D sourceTexture = sprite.texture;
		Rect spriteRect = sprite.textureRect;

		RenderTexture rt = RenderTexture.GetTemporary(
			sourceTexture.width, sourceTexture.height, 0, RenderTextureFormat.ARGB32);

		Graphics.Blit(sourceTexture, rt);

		RenderTexture previous = RenderTexture.active;
		RenderTexture.active = rt;

		int x = Mathf.FloorToInt(spriteRect.x);
		int y = Mathf.FloorToInt(spriteRect.y);
		int width = Mathf.FloorToInt(spriteRect.width);
		int height = Mathf.FloorToInt(spriteRect.height);

		var readableTexture = new Texture2D(width, height, TextureFormat.ARGB32, false);
		readableTexture.ReadPixels(new Rect(x, y, width, height), 0, 0);
		readableTexture.Apply();

		RenderTexture.active = previous;
		RenderTexture.ReleaseTemporary(rt);

		return readableTexture;
	}
}

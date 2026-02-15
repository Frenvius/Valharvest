using System;
using System.Collections.Generic;
using Jotunn.Configs;
using Jotunn.Entities;
using Jotunn.Managers;
using SimpleJson;
using UnityEngine;
using static Valharvest.Main;
using static Valharvest.Utils;
using static SimpleJson.SimpleJson;
using Logger = Jotunn.Logger;

namespace Valharvest.Scripts;

public static class Loaders {
	public static void LoadItems() {
		string jsonContent = ReadEmbeddedFile("items.resources");
		if (jsonContent != null) {
			var itemJson = DeserializeObject<JsonObject>(jsonContent);
			foreach (KeyValuePair<string, object> item in itemJson) {
				Dictionary<string, AssetBundle> getAssetBundle = GetAssetBundle();
				var itemObject = DeserializeObject<JsonObject>(item.Value.ToString());
				var itemPrefab = getAssetBundle[itemObject["assetBundle"].ToString()].LoadAsset<GameObject>(item.Key);

				try {
					CustomItem customItem = itemObject["crafting"] != null
						? CreateItemRecipe(itemPrefab, itemObject)
						: new CustomItem(itemPrefab, true);

					ItemDrop.ItemData.SharedData itemDrop = customItem.ItemDrop.m_itemData.m_shared;
					SetItemDrop(itemObject, itemDrop);
					ItemManager.Instance.AddItem(customItem);
				}
				catch (Exception ex) {
					Logger.LogError($"Error while loading {item.Key}: {ex.Message}");
				}
			}
		}
	}

	// Todo: refactor this method to be more reusable
	public static void LoadPieces() {
		string jsonContent = ReadEmbeddedFile("pieces.resources");
		if (jsonContent == null) return;
		var pieceJson = DeserializeObject<JsonObject>(jsonContent);
		foreach (KeyValuePair<string, object> piece in pieceJson) {
			if (Configurations.Valharvest.UseBoneAppetitCookingStations.Value)
				if (piece.Key is "piece_cooking_pot" or "piece_prep_table")
					continue;

			Dictionary<string, AssetBundle> getAssetBundle = GetAssetBundle();
			var pieceObject = DeserializeObject<JsonObject>(piece.Value.ToString());
			var piecePrefab = getAssetBundle[pieceObject["assetBundle"].ToString()].LoadAsset<GameObject>(piece.Key);

			try {
				CustomPiece customPiece = CreatePieceRecipe(piecePrefab, pieceObject);
				Piece pieceInfo = customPiece.Piece;
				SetPieceInfo(pieceObject, pieceInfo);
				PieceManager.Instance.AddPiece(customPiece);
			}
			catch (Exception ex) {
				Logger.LogError($"Error while loading {piece.Key}: {ex.Message}");
			}
		}

		Logger.LogInfo("Loaded pieces");
	}

	public static void LoadCookingStations() {
		if (!Configurations.Valharvest.UseBoneAppetitCookingStations.Value) {
			PieceManager.Instance.RemovePiece("rk_prep");
			PieceManager.Instance.RemovePiece("rk_griddle");
		}
	}

	public static Dictionary<string, AssetBundle> GetAssetBundle() {
		return new Dictionary<string, AssetBundle> {
			{ "valharvest", modAssets }
		};
	}

	public static void SetItemDrop(JsonObject content, ItemDrop.ItemData.SharedData itemDrop) {
		if (content["name"] != null) itemDrop.m_name = content["name"].ToString();
		if (content["description"] != null) itemDrop.m_description = content["description"].ToString();
	}

	public static void SetPieceInfo(JsonObject content, Piece piece) {
		if (content["name"] != null) piece.m_name = content["name"].ToString();
		if (content["description"] != null) piece.m_description = content["description"].ToString();
	}

	public static CustomItem CreateItemRecipe(GameObject itemPrefab, JsonObject itemObject) {
		var itemCraftObject = DeserializeObject<JsonObject>(itemObject["crafting"].ToString());
		RequirementConfig[] requirementsArr = DeserializeObject<RequirementConfig[]>(itemCraftObject["requirements"].ToString());
		var itemRecipe = new CustomItem(itemPrefab, true,
			new ItemConfig {
				Name = itemObject["name"].ToString(),
				Enabled = true,
				Amount = Convert.ToInt32(itemCraftObject["amount"].ToString()),
				CraftingStation = itemCraftObject["craftingStation"].ToString(),
				Requirements = requirementsArr
			});

		return itemRecipe;
	}

	public static CustomPiece CreatePieceRecipe(GameObject piecePrefab, JsonObject pieceObject) {
		var itemCraftObject = DeserializeObject<JsonObject>(pieceObject["crafting"].ToString());
		RequirementConfig[] requirementsArr = DeserializeObject<RequirementConfig[]>(itemCraftObject["requirements"].ToString());
		var pieceRecipe = new CustomPiece(piecePrefab, true,
			new PieceConfig {
				Name = pieceObject["name"].ToString(),
				Enabled = true,
				AllowedInDungeons = (bool)itemCraftObject["allowedInDungeons"],
				PieceTable = itemCraftObject["pieceTable"].ToString(),
				CraftingStation = itemCraftObject["craftingStation"]?.ToString(),
				Requirements = requirementsArr
			});

		return pieceRecipe;
	}
}

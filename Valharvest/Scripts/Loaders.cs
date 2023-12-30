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

namespace Valharvest.Scripts {
    public static class Loaders {
        public static void LoadItems() {
            var jsonContent = ReadEmbeddedFile("items.resources");
            if (jsonContent != null) {
                var itemJson = DeserializeObject<JsonObject>(jsonContent);
                foreach (var item in itemJson) {
                    var getAssetBundle = GetAssetBundle();
                    var itemObject = DeserializeObject<JsonObject>(item.Value.ToString());
                    var itemPrefab = getAssetBundle[itemObject["assetBundle"].ToString()].LoadAsset<GameObject>(item.Key);
                    
                    try {
                        var customItem = itemObject["crafting"] != null ? 
                            CreateItemRecipe(itemPrefab, itemObject) : 
                            new CustomItem(itemPrefab, true);
                        
                        var itemDrop = customItem.ItemDrop.m_itemData.m_shared;
                        SetItemDrop(itemObject, itemDrop);
                        ItemManager.Instance.AddItem(customItem);
                    } catch (Exception ex) {
                        Jotunn.Logger.LogError($"Error while loading {item.Key}: {ex.Message}");
                    }
                }
            }
        }
        
        // Todo: refactor this method to be more reusable
        public static void LoadPieces() {
            var jsonContent = ReadEmbeddedFile("pieces.resources");
            if (jsonContent == null) return;
            var pieceJson = DeserializeObject<JsonObject>(jsonContent);
            foreach (var piece in pieceJson) {
                var getAssetBundle = GetAssetBundle();
                var pieceObject = DeserializeObject<JsonObject>(piece.Value.ToString());
                var piecePrefab = getAssetBundle[pieceObject["assetBundle"].ToString()].LoadAsset<GameObject>(piece.Key);
                    
                try {
                    var customPiece = CreatePieceRecipe(piecePrefab, pieceObject);
                    var pieceInfo = customPiece.Piece;
                    SetPieceInfo(pieceObject, pieceInfo);
                    PieceManager.Instance.AddPiece(customPiece);
                } catch (Exception ex) {
                    Jotunn.Logger.LogError($"Error while loading {piece.Key}: {ex.Message}");
                }
            }
            Jotunn.Logger.LogInfo("Loaded pieces");
        }

        public static Dictionary<string, AssetBundle> GetAssetBundle() {
            return new Dictionary<string, AssetBundle> {
                {"valharvest", modAssets}
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
            var requirementsArr = DeserializeObject<RequirementConfig[]>(itemCraftObject["requirements"].ToString());
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
            var requirementsArr = DeserializeObject<RequirementConfig[]>(itemCraftObject["requirements"].ToString());
            var pieceRecipe = new CustomPiece(piecePrefab, true,
                new PieceConfig {
                    Name = pieceObject["name"].ToString(),
                    Enabled = true,
                    AllowedInDungeons = (bool) itemCraftObject["allowedInDungeons"],
                    PieceTable = itemCraftObject["pieceTable"].ToString(),
                    CraftingStation = itemCraftObject["craftingStation"]?.ToString(),
                    Requirements = requirementsArr
                });
            
            return pieceRecipe;
        }
    }
}
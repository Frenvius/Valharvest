using System.IO;
using UnityEngine;
using SimpleJson;
using System.Collections.Generic;
using Jotunn.Managers;

namespace Valharvest.Scripts; 

public static class ConsumableItemExtractor {
    public static void GenerateConsumableItemList() {
        Jotunn.Logger.LogInfo("Extracting consumable items...");
        // Access Valheim item database
        if (ObjectDB.instance) {
            List<ItemDrop> allItems = ObjectDB.instance.GetAllItems(ItemDrop.ItemData.ItemType.Material, "");
            List<ItemDrop> consumableItems = ObjectDB.instance.GetAllItems(ItemDrop.ItemData.ItemType.Consumable, "");

            // Create a Array to put all itens JSON object inside
            JsonArray itemData = new JsonArray();
            
            // merge allItems and consumableItems
            allItems.AddRange(consumableItems);
            
            Jotunn.Logger.LogInfo("Found " + allItems.Count + " items.");
            
            // Loop through all registered items
            foreach (ItemDrop item in allItems) {
                // Check if the item is consumable
                // Create a JSON object to store the item's data
                JsonObject itemJson = new JsonObject();
                itemJson.Add("var_name", item.m_itemData.m_shared.m_name);
                // get english localization
                itemJson.Add("raw_name", Localization.instance.Localize(item.m_itemData.m_shared.m_name));
                itemJson.Add("true_name", item.gameObject.name);
                
                JsonObject sharedJson = new JsonObject();

                foreach (var sharedString in item.m_itemData.m_shared.m_icons) {
                    sharedJson.Add("raw_name", sharedString.name);
                    sharedJson.Add("var_name", item.m_itemData.m_shared.m_name);
                    sharedJson.Add("item_type_name", item.m_itemData.m_shared.m_itemType.ToString());
                    sharedJson.Add("item_type", (int)item.m_itemData.m_shared.m_itemType);
                    sharedJson.Add("description", Localization.instance.Localize(item.m_itemData.m_shared.m_description));
                    sharedJson.Add("prefab_name", item.gameObject.name);
                    sharedJson.Add("food", item.m_itemData.m_shared.m_food.ToString());
                    sharedJson.Add("food_burn_time", item.m_itemData.m_shared.m_foodBurnTime.ToString());
                    sharedJson.Add("food_regen", item.m_itemData.m_shared.m_foodRegen.ToString());
                    sharedJson.Add("food_stamina", item.m_itemData.m_shared.m_foodStamina.ToString());
                    sharedJson.Add("weight", item.m_itemData.m_shared.m_weight.ToString());
                }
                
                itemJson.Add("shared_data", sharedJson);
                
                itemData.Add(itemJson);
            }
            
            // Write the item data JSON object to a file
            File.WriteAllText("itemdrops.json", itemData.ToString());
            
            Jotunn.Logger.LogInfo("Finished extracting consumable items.");
        }
    }

    private static bool IsConsumable(ItemDrop item) {
        // Add logic to determine if an item is consumable (e.g., check tags or components)
        // For demonstration purposes, assume items with the "Consumable" tag are consumable.
        return item.m_itemData.m_shared.m_itemType == ItemDrop.ItemData.ItemType.Consumable;
    }

    [System.Serializable]
    private class ItemData {
        public string Name;
        public string Description;
        // Add other item properties you need
    }
}
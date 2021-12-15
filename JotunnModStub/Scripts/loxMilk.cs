using HarmonyLib;
using UnityEngine;

namespace Valharvest.Scripts {
    [HarmonyPatch(typeof(Tameable), nameof(Tameable.Interact))]
    public static class InteractPatch {
        public static bool Prefix(Tameable __instance, ref bool __runOriginal) {
            if ((__instance.gameObject.name == "Lox(Clone)") | __instance.gameObject.name.Contains("Lox")) {
                if (Input.GetKey(KeyCode.LeftAlt)) {
                    __runOriginal = false;
                    if (!__instance.m_nview.IsValid()) 
                        return false;
                    var loxComponent = __instance.gameObject.GetComponent<MilkLox>();
                    var milkLevel = loxComponent.GetMilkLevel();
                    if (milkLevel > 0) {
                        CheckAbleToAddItem(loxComponent, loxComponent.loxPoint);
                        return true;
                    }
                }
            }
            return __runOriginal = true;
        }
        
        public static void CheckAbleToAddItem(MilkLox loxComponent, Transform loxPoint) {
            var inventory = Player.m_localPlayer.GetInventory();
            var milkLevel = loxComponent.GetMilkLevel();
            if (milkLevel > 0) {
                if (inventory.HaveEmptySlot()) {
                    if (ItemExistOnInventory()) {
                        loxComponent.spawnEffect.Create(loxPoint.position, Quaternion.identity);
                        AddItemToPlayer(loxComponent, milkLevel);
                    } else {
                        Player.m_localPlayer.Message(MessageHud.MessageType.Center, "$piece_itemstand_missingitem");
                    }   
                } else {
                    Player.m_localPlayer.Message(MessageHud.MessageType.Center, "$msg_noroom");
                }
            }
        }
        
        private static bool ItemExistOnInventory() {
            var inventory = Player.m_localPlayer.GetInventory();
            return inventory.HaveItem("$empty_bottle_name");
        }

        private static void AddItemToPlayer(MilkLox loxComponent, int milkLevel) {
            var inventory = Player.m_localPlayer.GetInventory();
            var bottle = inventory.GetItem("$empty_bottle_name");
            var count = 0;
            for (var i = 0; i < milkLevel; i++) {
                if (inventory.HaveEmptySlot()) {
                    if (ItemExistOnInventory()) {
                        if (inventory.RemoveOneItem(bottle)) {
                            loxComponent.nview.InvokeRPC("ExtractMilk");
                            count++;
                        }
                    } else {
                        Player.m_localPlayer.Message(MessageHud.MessageType.Center, "$piece_itemstand_missingitem");
                        break;
                    }   
                } else {
                    Player.m_localPlayer.Message(MessageHud.MessageType.Center, "$msg_noroom");
                }
            }
            loxComponent.nview.InvokeRPC("ResetLevel", count);
        }
    }

    [HarmonyPatch(typeof(Tameable), nameof(Tameable.GetHoverText))]
    public static class HoverTextPatch {
        public static void Postfix(ref string __result, Tameable __instance) {
            if (!__instance.gameObject.GetComponent<Humanoid>().IsTamed()) return;
            if ((__instance.gameObject.name == "Lox(Clone)") | __instance.gameObject.name.Contains("Lox")) {
                var loxComponent = __instance.gameObject.GetComponent<MilkLox>();
                var milkLevel = loxComponent.GetMilkLevel();

                if (milkLevel > 0) {
                    __result += Localization.instance.Localize("\n\nLox Milk" + " ( $milk_bottle_name x " +
                                                               milkLevel +
                                                               " )\n[<b><color=yellow>L-Alt + $KEY_Use</color></b>] $water_well_extract");                    
                } else {
                    __result += Localization.instance.Localize("\n\nLox Milk ( $piece_container_empty )");
                }
            }
        }
    }
}
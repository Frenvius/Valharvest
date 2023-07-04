using System.Linq;
using HarmonyLib;
using UnityEngine;

namespace Valharvest.Scripts; 

[HarmonyPatch(typeof(Tameable), nameof(Tameable.Interact))]
public static class InteractPatch {
    public static bool Prefix(Tameable __instance, ref bool __runOriginal) {
        if (!((__instance.gameObject.name == "Lox(Clone)") | __instance.gameObject.name.Contains("Lox")))
            return __runOriginal = true;
        if (!Input.GetKey(KeyCode.LeftAlt)) return __runOriginal = true;
        __runOriginal = false;
        if (!__instance.m_nview.IsValid())
            return false;
        var loxComponent = __instance.gameObject.GetComponent<MilkLox>();
        var milkLevel = loxComponent.GetMilkLevel();
        if (milkLevel <= 0) return __runOriginal = true;
        CheckAbleToAddItem(loxComponent, loxComponent.loxPoint);
        return true;
    }

    private static void CheckAbleToAddItem(MilkLox loxComponent, Transform loxPoint) {
        var inventory = Player.m_localPlayer.m_inventory;
        var milkLevel = loxComponent.GetMilkLevel();
        if (milkLevel <= 0) return;
        if (inventory.HaveEmptySlot()) {
            if (GetItemOnInventory("$empty_bottle_name") != null) {
                loxComponent.SpawnEffect.Create(loxPoint.position, Quaternion.identity);
                AddItemToPlayer(loxComponent, milkLevel);
            }
            else {
                Player.m_localPlayer.Message(MessageHud.MessageType.Center, "$piece_itemstand_missingitem");
            }
        }
        else {
            Player.m_localPlayer.Message(MessageHud.MessageType.Center, "$msg_noroom");
        }
    }

    private static ItemDrop.ItemData GetItemOnInventory(string name) {
        var inventory = Player.m_localPlayer.m_inventory;
        return inventory.m_inventory.FirstOrDefault(item => item.m_shared.m_name == name);
    }

    private static void AddItemToPlayer(MilkLox loxComponent, int milkLevel) {
        var inventory = Player.m_localPlayer.m_inventory;
        var bottle = GetItemOnInventory("$empty_bottle_name");
        var count = 0;
        for (var i = 0; i < milkLevel; i++)
            if (inventory.HaveEmptySlot()) {
                if (bottle != null) {
                    if (!inventory.RemoveOneItem(bottle)) continue;
                    loxComponent.nview.InvokeRPC("ExtractMilk");
                    count++;
                }
                else {
                    Player.m_localPlayer.Message(MessageHud.MessageType.Center, "$piece_itemstand_missingitem");
                    break;
                }
            }
            else {
                Player.m_localPlayer.Message(MessageHud.MessageType.Center, "$msg_noroom");
            }

        loxComponent.nview.InvokeRPC("ResetLevel", count);
    }
}

[HarmonyPatch(typeof(Tameable), nameof(Tameable.GetHoverText))]
public static class HoverTextPatch {
    public static void Postfix(ref string __result, Tameable __instance) {
        if (!__instance.gameObject.GetComponent<Humanoid>().IsTamed()) return;
        if (!((__instance.gameObject.name == "Lox(Clone)") | __instance.gameObject.name.Contains("Lox"))) return;
        var loxComponent = __instance.gameObject.GetComponent<MilkLox>();
        var milkLevel = loxComponent.GetMilkLevel();

        if (milkLevel > 0)
            __result += Localization.instance.Localize("\n\nLox Milk" + " ( $milk_bottle_name x " +
                                                       milkLevel +
                                                       " )\n[<b><color=yellow>L-Alt + $KEY_Use</color></b>] $lox_milk_extract");
        else
            __result += Localization.instance.Localize("\n\nLox Milk ( $piece_container_empty )");
    }
}
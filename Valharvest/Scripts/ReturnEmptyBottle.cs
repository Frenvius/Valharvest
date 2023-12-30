using System.Collections.Generic;
using HarmonyLib;
using UnityEngine;

namespace Valharvest.Scripts;

[HarmonyPatch]
public static class ReturnEmptyBottle {
    [HarmonyPatch(typeof(Player), nameof(Player.ConsumeResources))]
    private static class ConsumeResourcesPatch {
        private static bool Prefix(Player __instance, IEnumerable<Piece.Requirement> requirements, int qualityLevel) {
            foreach (var requirement in requirements) {
                if (!requirement.m_resItem) continue;
                var totalRequirement = requirement.GetAmount(qualityLevel);
                if (totalRequirement <= 0)
                    continue;

                var reqName = requirement.m_resItem.m_itemData.m_shared.m_name;

                if (reqName != "$milk_bottle_name") continue;
                var inventory = __instance.GetInventory();
                var emptyBottle = InteractPatch.GetItemOnInventory("$empty_bottle_name");
                if (inventory.HaveEmptySlot()) {
                    AddEmptyMilkBottle(__instance, qualityLevel, inventory, totalRequirement);
                    return true;
                }
                if (emptyBottle != null) {
                    if (inventory.CanAddItem(emptyBottle, totalRequirement)) {
                        AddEmptyMilkBottle(__instance, qualityLevel, inventory, totalRequirement);
                        return true;
                    }
                }
                __instance.Message(MessageHud.MessageType.Center, "$msg_noroom");
                return true;
            }
            return true;
        }

        private static void AddEmptyMilkBottle(Player instance, int qualityLevel, Inventory inventory, int totalRequirement) {
            inventory.AddItem("empty_bottle", totalRequirement, qualityLevel, 0, 0, instance.GetPlayerName());
        }
    }

    [HarmonyPatch(typeof(Player), nameof(Player.RemovePiece))]
    private static class RemovePiecePatch {
        public static bool Prefix(Player __instance, ref bool __runOriginal) {
            if (!Physics.Raycast(GameCamera.instance.transform.position, GameCamera.instance.transform.forward, out var hitInfo, 50f, __instance.m_removeRayMask) ||
                !(Vector3.Distance(hitInfo.point, __instance.m_eye.position) < __instance.m_maxPlaceDistance)) return __runOriginal;
            var piece = hitInfo.collider.GetComponentInParent<Piece>();
            if (piece == null || piece.m_name != "$piece_milk_can_name") return __runOriginal;
            var resources = piece.m_resources;
            foreach (var resource in resources) {
                if (!resource.m_resItem) continue;
                string reqName = resource.m_resItem.m_itemData.m_shared.m_name;
                if (reqName != "$milk_bottle_name") continue;
                var inventory = __instance.GetInventory();
                var emptyBottle = InteractPatch.GetItemOnInventory("$empty_bottle_name");
                if (emptyBottle == null) {
                    __instance.Message(MessageHud.MessageType.Center, "$msg_donthaveany");
                    __runOriginal = false;
                    return false;
                }
                int totalRequirement = resource.GetAmount(0);
                if (inventory.CountItems(emptyBottle.m_shared.m_name) < totalRequirement) {
                    __instance.Message(MessageHud.MessageType.Center, "$enough_empty_bottle");
                    __runOriginal = false;
                    return false;
                }
                inventory.RemoveItem(emptyBottle.m_shared.m_name, totalRequirement);
            }
            return __runOriginal = true;
        }
    }
}
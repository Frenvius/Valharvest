using System;
using HarmonyLib;
using Jotunn.Managers;
using UnityEngine;
using static Valharvest.Main;
using Logger = Jotunn.Logger;

namespace Valharvest.Scripts {
    [HarmonyPatch(typeof(Player), "Update")]
    public static class UpdateLoxSfx {
        private static void Postfix(ref Player __instance) {
            if (PlayerOnSpawn.isOnline)
                if (HoverTextPatch.loxInstance) {
                    var inventory = Player.m_localPlayer.GetInventory();
                    if (Input.GetKeyDown(KeyCode.LeftAlt)) {
                        if (!inventory.HaveEmptySlot() | !inventory.HaveItem("Empty Bottle")) {
                            HoverTextPatch.loxInstance.m_petEffect = new EffectList();
                        } else {
                            if (InteractPatch.IsTimeToMilk(HoverTextPatch.loxInstance))
                                HoverTextPatch.loxInstance.m_petEffect = loxMilkSfx;
                            else
                                HoverTextPatch.loxInstance.m_petEffect = new EffectList();
                        }
                    }

                    if (Input.GetKeyUp(KeyCode.LeftAlt)) HoverTextPatch.loxInstance.m_petEffect = loxPetSfx;
                }
        }
    }

    [HarmonyPatch(typeof(Player), "OnSpawned")]
    public static class PlayerOnSpawn {
        public static bool isOnline;

        private static void Prefix(ref Player __instance) {
            isOnline = true;
        }
    }

    [HarmonyPatch(typeof(Player), "OnDestroy")]
    public static class PlayerDestroy {
        private static void Prefix(ref Player __instance) {
            PlayerOnSpawn.isOnline = false;
        }
    }

    [HarmonyPatch(typeof(Tameable), nameof(Tameable.Interact))]
    public static class InteractPatch {
        public const double TimeToMilk = 2400;
        private static GameObject _milkBottle;

        public static void Postfix(Tameable __instance) {
            if ((__instance.gameObject.name == "Lox(Clone)") | __instance.gameObject.name.Contains("Lox")) {
                if (!IsTimeToMilk(__instance)) {
                    Player.m_localPlayer.Message(MessageHud.MessageType.Center, "");
                    return;
                }

                if (Input.GetKey(KeyCode.LeftAlt))
                    try {
                        _milkBottle = PrefabManager.Instance.GetPrefab("milk_bottle");
                        var inventory = Player.m_localPlayer.GetInventory();
                        var userItems = inventory.GetAllItems();
                        if (inventory.HaveEmptySlot())
                            foreach (var item in userItems) {
                                if (item.m_shared.m_name == "Empty Bottle") {
                                    var count = 0;
                                    var bottle = inventory.GetItem(item.m_shared.m_name);
                                    if (inventory.RemoveOneItem(bottle)) {
                                        inventory.AddItem(_milkBottle, 1);
                                        count += 1;
                                    }

                                    if (inventory.RemoveOneItem(bottle)) {
                                        inventory.AddItem(_milkBottle, 1);
                                        count += 1;
                                    }

                                    Player.m_localPlayer.Message(MessageHud.MessageType.Center,
                                        count + " Milk Bottle Added");
                                    if (__instance.m_nview.IsValid())
                                        __instance.m_nview.GetZDO()
                                            .Set("TameLastMilking", ZNet.instance.GetTime().Ticks);
                                    break;
                                }

                                Player.m_localPlayer.Message(MessageHud.MessageType.Center,
                                    "$piece_itemstand_missingitem");
                            }
                        else
                            Player.m_localPlayer.Message(MessageHud.MessageType.Center, "$msg_noroom");
                    } catch (Exception ex) {
                        Logger.LogError($"Error while milking Lox: {ex.Message}");
                    }
            }
        }

        public static bool IsTimeToMilk(Tameable __instance) {
            var dateTime = new DateTime(__instance.m_nview.GetZDO().GetLong("TameLastMilking"));
            return (ZNet.instance.GetTime() - dateTime).TotalSeconds > TimeToMilk;
        }

        public static float GetRemainingTimeToMilk(Tameable __instance) {
            var dateTime = new DateTime(__instance.m_nview.GetZDO().GetLong("TameLastMilking"));
            return (float) (ZNet.instance.GetTime() - dateTime).TotalSeconds;
        }
    }

    [HarmonyPatch(typeof(Tameable), nameof(Tameable.GetHoverText))]
    public static class HoverTextPatch {
        public static Tameable loxInstance;

        public static void Postfix(ref string __result, Tameable __instance) {
            if (!__instance.gameObject.GetComponent<Humanoid>().IsTamed()) return;
            if ((__instance.gameObject.name == "Lox(Clone)") | __instance.gameObject.name.Contains("Lox")) {
                loxInstance = __instance;
                var timeToMilk =
                    TimeSpan.FromSeconds(InteractPatch.GetRemainingTimeToMilk(__instance) - InteractPatch.TimeToMilk);
                if (InteractPatch.IsTimeToMilk(__instance))
                    __result += Localization.instance.Localize(
                        "\n[<b><color=yellow>L-Alt + $KEY_Use</color></b>] To Milk Lox");
                else
                    __result += Localization.instance.Localize(
                        "\n<b><color=red>[" + timeToMilk.ToString(@"mm\:ss") + " to milk Lox]</color></b>");
            }
        }
    }
}

//postfix awake
// if( __instance.m_consumeItems != null )
// {
//     foreach ( var item in instance.m_consumeItems )
//     {
//         if(item.m_itemData.m_shared.m_name == "whatever the name is for raw meat")
//         {
//             //it can consume meat so it can also consume pork
//             __instance.m_consumeItems.add( 'prefab for pork here' )
//         }        
//     }
// }
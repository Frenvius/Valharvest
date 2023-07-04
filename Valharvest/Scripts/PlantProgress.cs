using System;
using HarmonyLib;
using UnityEngine;
using System.Collections.Generic;
using System.Globalization;

namespace Valharvest.Scripts;

[HarmonyPatch]
public static class PlantProgress {
    private static readonly List<string> PickableList = new()
        { "RaspberryBush(Clone)", "BlueberryBush(Clone)", "CloudberryBush(Clone)", "apple_tree(Clone)" };

    private static string GetColour(double percentage) {
        var colour = percentage switch {
            >= 25 and <= 49 => "orange",
            >= 50 and <= 74 => "yellow",
            >= 75 and <= 100 => "green",
            _ => "red"
        };

        return colour;
    }

    [HarmonyPostfix]
    [HarmonyPatch(typeof(Plant), "GetHoverText")]
    public static string PlantGetHoverText_Patch(string __result, Plant __instance) {
        if (__instance == null) return __result;
        if (!Configurations.Valharvest.PlantProgressEnabled.Value) return __result;

        double percentage = Mathf.Floor((float)__instance.TimeSincePlanted() / __instance.GetGrowTime() * 100);
        var colour = GetColour(percentage);
        var percentageString = decimal.Round((decimal) percentage, 2, MidpointRounding.AwayFromZero).ToString(CultureInfo.InvariantCulture);
        var growPercentage = "<color=" + colour + ">" + percentageString + "%</color>";
        return __result.Replace(" )", $", {growPercentage} )");
    }

    [HarmonyPostfix]
    [HarmonyPatch(typeof(Pickable), "GetHoverText")]
    public static string BerryBushPickable_Patch(string __result, Pickable __instance) {
        if (!PickableList.Contains(__instance.name)) return __result;
        if (!Configurations.Valharvest.PickableProgressEnabled.Value) return __result;

        var startTime = new DateTime(__instance.m_nview.GetZDO().GetLong("picked_time"));
        var percentage = (ZNet.instance.GetTime() - startTime).TotalMinutes / __instance.m_respawnTimeMinutes * 100;
        if (percentage > 99.99f) return __result;

        var colour = GetColour(percentage);
        var percentageString = decimal.Round((decimal) percentage, 2).ToString(CultureInfo.InvariantCulture);
        var growPercentage = "<color=" + colour + ">" + percentageString + "%</color>";
        return __result + " " + $"{Localization.instance.Localize(__instance.GetHoverName())} ( {growPercentage} )";
    }
    
    [HarmonyPatch(typeof(Pickable), "SetPicked")]
    public static class FixPickableTime {
        [HarmonyPrefix]
        public static void Prefix(bool picked, ZNetView ___m_nview, bool ___m_picked, ref PickState __state) {
            __state = new PickState {
                picked_time = ___m_nview.GetZDO().GetLong("picked_time"),
                picked = ___m_picked
            };
        }

        [HarmonyPostfix]
        public static void Postfix(bool picked, ZNetView ___m_nview, bool ___m_picked, ref PickState __state) {
            if (__state.picked == ___m_picked) ___m_nview.GetZDO().Set("picked_time", __state.picked_time);
        }

        public class PickState {
            public bool picked;
            public long picked_time;
        }
    }
}
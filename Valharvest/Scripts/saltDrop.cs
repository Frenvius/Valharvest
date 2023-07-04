using System;
using System.Collections.Generic;
using HarmonyLib;
using UnityEngine;
using Random = UnityEngine.Random;

namespace Valharvest.Scripts; 

[HarmonyPatch]
public static class SaltDrop {
    private static string _dropTableObject = "";

    [HarmonyPostfix]
    [HarmonyPatch(typeof(DropTable), "GetDropList", new Type[] { })]
    public static void DropTableGetDropList_Patch(ref List<GameObject> __result) {
        if (!Environment.StackTrace.Contains("MineRock") &&
            (!Environment.StackTrace.Contains("DropOnDestroyed") ||
             !_dropTableObject.Contains("Rock"))) return;
        if (!(Random.value < 0.40f)) return;
        var go = ZNetScene.instance.GetPrefab("salt");
        __result.Add(go);
    }

    [HarmonyPatch(typeof(DropOnDestroyed), "OnDestroyed")]
    public static class DropOnDestroyedOnDestroyedPatch {
        [HarmonyPrefix]
        private static void Prefix(ref DropOnDestroyed __instance) {
            _dropTableObject = __instance.gameObject.name;
        }

        [HarmonyPostfix]
        private static void Postfix(ref DropOnDestroyed __instance) {
            _dropTableObject = "";
        }
    }
}
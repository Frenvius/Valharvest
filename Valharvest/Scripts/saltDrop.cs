using System;
using System.Collections.Generic;
using HarmonyLib;
using UnityEngine;
using Random = UnityEngine.Random;

namespace Valharvest.Scripts {
    [HarmonyPatch(typeof(DropTable), "GetDropList", typeof(int))]
    public static class SaltDrop {
        private static string _dropTableObject = "";

        [HarmonyPatch(typeof(DropOnDestroyed), "OnDestroyed")]
        private static class DropOnDestroyed_OnDestroyed_Patch {
            private static void Prefix(ref DropOnDestroyed __instance) {
                _dropTableObject = __instance.gameObject.name;
            }

            private static void Postfix(ref DropOnDestroyed __instance) {
                _dropTableObject = "";
            }
        }

        [HarmonyPatch(typeof(DropTable), "GetDropList", new Type[] { })]
        private static class DropTable_GetDropList_Patch {
            private static void Postfix(ref List<GameObject> __result) {
                if (Environment.StackTrace.Contains("MineRock") || Environment.StackTrace.Contains("DropOnDestroyed") &&
                    _dropTableObject.Contains("Rock"))
                    if (Random.value < 0.80f) {
                        var go = ZNetScene.instance.GetPrefab("salt");
                        __result.Add(go);
                    }
            }
        }
    }
}
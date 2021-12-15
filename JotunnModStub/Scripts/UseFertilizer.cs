using HarmonyLib;
using UnityEngine;

namespace Valharvest.Scripts {
    public class UseFertilizer {
        [HarmonyPatch(typeof(Plant), nameof(Plant.Awake))]
        public static class FixPlantHealth {
            public static void Prefix(Plant __instance) {
                __instance.gameObject.AddComponent<InteractFertilizer>();
            }
        }
    }
}
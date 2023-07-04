using HarmonyLib;

namespace Valharvest.Scripts;

[HarmonyPatch]
public class UseFertilizer {
    [HarmonyPrefix]
    [HarmonyPatch(typeof(Plant), nameof(Plant.Awake))]
    public static void FixPlantHealth(Plant __instance) {
        __instance.gameObject.AddComponent<InteractFertilizer>();
    }
}
using HarmonyLib;

namespace Valharvest.Scripts;

[HarmonyPatch]
public static class AddLoxComponent {
    [HarmonyPrefix]
    [HarmonyPatch(typeof(MonsterAI), nameof(MonsterAI.Awake))]
    public static void MonsterAIAwakePatch(MonsterAI __instance) {
        if (__instance.gameObject.name.Contains("Lox")) __instance.gameObject.AddComponent<MilkLox>();
    }
}
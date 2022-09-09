using HarmonyLib;

namespace Valharvest.Scripts {
    public class AddLoxComponent {
        [HarmonyPatch(typeof(MonsterAI), nameof(MonsterAI.Awake))]
        public static class MonsterAIAwakePatch {
            public static void Prefix(MonsterAI __instance) {
                if (__instance.gameObject.name.Contains("Lox")) {
                    __instance.gameObject.AddComponent<MilkLox>();
                }
            }
        }
    }
}
using HarmonyLib;
using Jotunn.Managers;

namespace Valharvest.Scripts;

[HarmonyPatch]
public static class HandlePrefabComponent {
    [HarmonyPrefix]
    [HarmonyPatch(typeof(MonsterAI), nameof(MonsterAI.Awake))]
    public static void MonsterAIAwakePatch(MonsterAI __instance) {
        if (__instance.gameObject.name.Contains("Lox")) __instance.gameObject.AddComponent<MilkLox>();
    }
    
    public static void ZNetViewAwakePatch() {
	    var waterWell = PrefabManager.Instance.GetPrefab("water_well");
	    if (waterWell) {
		    var water = waterWell.gameObject.transform.Find("water");
		    var spawnPoint = waterWell.gameObject.transform.Find("spawnpoint");
		    
		    if (water != null && spawnPoint != null) {
			    var customBeehive = waterWell.gameObject.AddComponent<CustomBeehive>();
			    customBeehive.m_hideWhenPicked = water.gameObject;
			    customBeehive.m_spawnPoint = spawnPoint;
			    customBeehive.m_honeyItem = PrefabManager.Instance.GetPrefab("water_bucket").GetComponent<ItemDrop>();
			    var spawnEffect = new EffectList();
			    if (spawnEffect.m_effectPrefabs.Length == 0) {
				    spawnEffect.m_effectPrefabs = new EffectList.EffectData[1];
			    }
			    spawnEffect.m_effectPrefabs[0] = new EffectList.EffectData {
				    m_prefab = PrefabManager.Instance.GetPrefab("sfx_ship_waterimpact"),
				    m_enabled = true
			    };
			    customBeehive.m_spawnEffect = spawnEffect;
		    }
	    }
    }
}
using HarmonyLib;
using Jotunn.Managers;
using UnityEngine;

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

    public static void PrepTableSmokePatch() {
        if (Configurations.Valharvest.PrepTableSmokeEnabled.Value) return;

        var prepTable = PrefabManager.Instance.GetPrefab("piece_prep_table");
        if (prepTable == null) return;

        var smokeSpawner = prepTable.transform.Find("connectionEffectPoint/prep/SmokeSpawner");
        if (smokeSpawner != null) {
            smokeSpawner.gameObject.SetActive(false);
        }
    }

    public static void FoodCrateEffectsPatch() {
        var crate = PrefabManager.Instance.GetPrefab("vh_piece_food_crate");
        if (crate == null) return;

        var placeFx = PrefabManager.Instance.GetPrefab("vfx_Place_wood_pole");
        var placeSfx = PrefabManager.Instance.GetPrefab("sfx_build_hammer_wood");
        var hitFx    = PrefabManager.Instance.GetPrefab("vfx_SawDust");
        var hitSfx   = PrefabManager.Instance.GetPrefab("sfx_wood_hit");
        var destroyFx = PrefabManager.Instance.GetPrefab("vfx_SawDust");
        var destroySfx = PrefabManager.Instance.GetPrefab("sfx_wood_destroyed");

        var piece = crate.GetComponent<Piece>();
        if (piece != null) {
            piece.m_placeEffect = BuildEffectList(placeFx, placeSfx);
        }

        var wnt = crate.GetComponent<WearNTear>();
        if (wnt != null) {
            wnt.m_hitEffect       = BuildEffectList(hitFx, hitSfx);
            wnt.m_destroyedEffect = BuildEffectList(destroyFx, destroySfx);
        }
    }

    private static EffectList BuildEffectList(params GameObject[] effects) {
        var datas = new System.Collections.Generic.List<EffectList.EffectData>();
        foreach (var fx in effects) {
            if (fx == null) continue;
            datas.Add(new EffectList.EffectData { m_prefab = fx, m_enabled = true });
        }
        return new EffectList { m_effectPrefabs = datas.ToArray() };
    }
}
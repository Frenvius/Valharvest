using System.Collections;
using HarmonyLib;
using UnityEngine;
using Object = UnityEngine.Object;
using Random = UnityEngine.Random;

namespace Valharvest.Scripts;

[HarmonyPatch]
public class EggAndFeatherDrop {
    
    [HarmonyPostfix]
    [HarmonyPatch(typeof(TreeBase), nameof(TreeBase.SpawnLog))]
    public static void TreeBaseSpawnLog_Patch(TreeBase __instance) {
        string prefabName = __instance.m_logPrefab.name;
        string[] logPrefab = { "PineTree_log", "PineTree_logOLD", "FirTree_log", "beech_log", "Birch_log", "Oak_log" };
        
        bool isPrefabInArray = ((IList) logPrefab).Contains(prefabName);

        if (!isPrefabInArray) return;
        if (!(Random.value < 0.15f)) return;
        var feather = ZNetScene.instance.GetPrefab("Feathers");
        var egg = ZNetScene.instance.GetPrefab("rk_egg");
        int numFeathers = Random.Range(1, 8);
        int numEggs = Random.Range(1, 3);
        for (int i = 0; i < numFeathers; i++) {
            Object.Instantiate(feather, __instance.transform.position, Quaternion.identity);
        }
        for (int i = 0; i < numEggs; i++) {
            Object.Instantiate(egg, __instance.transform.position, Quaternion.identity);
        }
    }
}
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

    [HarmonyPatch(typeof (Player), "UpdatePlacementGhost")]
    public static class FixFarmGrid {
        public static void Postfix(Player __instance) {
            GameObject placementGhost = GetPlantObject(__instance);
            if (placementGhost != null) {
                Plant plant = placementGhost.GetComponent<Plant>();
                if (plant != null) {
                    var plantPostion = plant.transform.position;
                    var cultivatedGround = FixPlantHealth.GetCultivatedGround(new Vector3(plantPostion.x, plantPostion.y + 1f, plantPostion.z));
                    if (cultivatedGround != null) {
                        var placementPosition = placementGhost.transform.position;
                        var groundPosition = cultivatedGround.transform.position;
                        var placementNewPosition = new Vector3(placementPosition.x, groundPosition.y + 0.0444f, placementPosition.z);
                        placementGhost.transform.position = placementNewPosition;
                        __instance.m_placementStatus = Player.PlacementStatus.Valid;
                        __instance.m_placementGhost.SetActive(value: true);
                        __instance.SetPlacementGhostValid(true);
                    }
                }
            }
        }

        private static GameObject GetPlantObject(Player __instance) {
            GameObject placementGhost = __instance?.m_placementGhost;
            if (placementGhost == null){
                return null;   
            }
            return placementGhost;
        }
    }

    [HarmonyPatch(typeof(Plant), nameof(Plant.UpdateHealth))]
    public static class FixPlantHealth {
        public static void Prefix(Plant __instance, ref bool __runOriginal) {
            __runOriginal = false;
            var plantPosition = __instance.transform.position;
            if (CheckIfItemBellowIsCultivatedGround(new Vector3(plantPosition.x, plantPosition.y + 1f, plantPosition.z))) {
                __instance.m_status = Plant.Status.Healthy;
                return;
            }
            
            __runOriginal = true;
        }

        public static bool CheckIfItemBellowIsCultivatedGround(Vector3 position) {
            var ray = new Ray(position, Vector3.down);
            var hits = Physics.RaycastAll(ray);
            foreach (var hit in hits) {
                if (hit.collider.gameObject.name == "Cultivated_ground_piece") {
                    return true;
                }
            }
            return false;
        }
        
        public static GameObject GetCultivatedGround(Vector3 position) {
            var ray = new Ray(position, Vector3.down);
            var hits = Physics.RaycastAll(ray);
            foreach (var hit in hits) {
                if (hit.collider.gameObject.name == "Cultivated_ground_piece") {
                    return hit.collider.gameObject;
                }
            }
            return null;
        }
    }
}
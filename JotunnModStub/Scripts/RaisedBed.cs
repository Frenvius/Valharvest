using System.Linq;
using HarmonyLib;
using UnityEngine;

namespace Valharvest.Scripts {
    public class RaisedBed {
        [HarmonyPatch(typeof (Player), "UpdatePlacementGhost")]
    public static class FixFarmGrid {
        public static void Postfix(Player __instance) {
            GameObject placementGhost = GetPlantObject(__instance);
            if (placementGhost != null) {
                Plant plant = placementGhost.GetComponent<Plant>();
                if (plant != null) {
                    var cultivatedHeight = FixPlantHealth.GetCultivatedGroundHeight(plant);
                    var placementPosition = placementGhost.transform.position;
                    var placementNewPosition = new Vector3(placementPosition.x, cultivatedHeight, placementPosition.z);
                    placementGhost.transform.position = placementNewPosition;
                    __instance.m_placementStatus = Player.PlacementStatus.Valid;
                    __instance.m_placementGhost.SetActive(value: true);
                    __instance.SetPlacementGhostValid(true);
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
        
        public static float GetCultivatedGroundHeight(Plant plant) {
            var transform = plant.transform;
            ZoneSystem.instance.GetSolidHeight(transform.position, 0, out var height, transform);
            return height;
        }
    }
    
    [HarmonyPatch(typeof(Pickable), nameof(Pickable.Awake))]
    public static class FixPlantPhysics {
        public static void Prefix(Pickable __instance) {
            string[] plantsArray = {
                "Pickable_Barley",
                "Pickable_Carrot",
                "Pickable_Dandelion",
                "Pickable_Flax",
                "Pickable_Mushroom",
                "Pickable_Mushroom_blue",
                "Pickable_Mushroom_yellow",
                "Pickable_Onion",
                "Pickable_SeedCarrot",
                "Pickable_SeedOnion",
                "Pickable_SeedTurnip",
                "Pickable_Thistle",
                "Pickable_Turnip"
            };
            
            if (plantsArray.Any(__instance.name.Contains)) {
                StaticPhysics staticPhysics = __instance.gameObject.GetComponent<StaticPhysics>();
                staticPhysics.m_checkSolids = true;
                
                // Todo: make this check work with the raised bed
                // var plantPosition = __instance.gameObject.transform.position;
                // if (FixPlantHealth.CheckIfItemBellowIsCultivatedGround(new Vector3(plantPosition.x, plantPosition.y + 3f, plantPosition.z))) {
                //     StaticPhysics staticPhysics = __instance.gameObject.GetComponent<StaticPhysics>();
                //     staticPhysics.m_checkSolids = true;
                // }
            }
        }
    }
    }
}
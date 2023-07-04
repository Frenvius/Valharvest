using System.Collections.Generic;
using System.Linq;
using HarmonyLib;
using UnityEngine;

namespace Valharvest.Scripts;

[HarmonyPatch]
public class RaisedBed {
    private static readonly string[] PlantsArray = {
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

    private static readonly Collider[] pieceColliders = new Collider[2000];
    private static bool massPlanting = true;
    private static readonly List<Piece> m_tempPieces = new();
    private static readonly List<Transform> m_tempSnapPoints1 = new();
    private static readonly List<Transform> m_tempSnapPoints2 = new();

    [HarmonyPostfix]
    [HarmonyPriority(Priority.Last)]
    [HarmonyAfter("mod.valheim_plus", "org.bepinex.plugins.farming", "com.odinplusqol.mod", "BepIn.Sarcen.FarmGrid",
        "Harmony.Sarcen.FarmGrid", "infinity_hammer", "org.bepinex.plugins.conversionsizespeed")]
    [HarmonyPatch(typeof(Player), nameof(Player.UpdatePlacementGhost))]
    private static void UpdatePlacementGrid(Player __instance, bool flashGuardStone) {
        var placementGhost = __instance.m_placementGhost;
        if (placementGhost == null) return;
        var plant = placementGhost.GetComponent<Plant>();
        var piece = placementGhost.GetComponent<Piece>();
        if (plant == null) return;
        if (Main.IsFarmingModInstalled) {
            var massPlantingKey = Main.MassPlantShortcut;
            if (massPlantingKey.IsDown()) {
                massPlanting = !massPlanting;
            }
        }
        
        if (!IsPlantPlantable(plant)) return;
        var cultivatedHeight = GetCultivatedGroundHeight(plant, __instance);
        var placementPosition = placementGhost.transform.position;
        piece.SetInvalidPlacementHeightlight(false);
        __instance.m_placementStatus = Player.PlacementStatus.Valid;

        if (Main.IsFarmingModInstalled) {
            var massPlantingKey = Main.MassPlantShortcut;
            if (massPlanting) {
                piece.SetInvalidPlacementHeightlight(true);
                __instance.m_placementStatus = Player.PlacementStatus.Invalid;
                var message = Localization.instance.Localize("$msg_TurnMassPlantingOff");
                __instance.Message(MessageHud.MessageType.Center, $"{message}{massPlantingKey.ToString()} key", 0, (Sprite) null);
            }
        }

        var placementNewPosition = new Vector3(placementPosition.x, cultivatedHeight, placementPosition.z);
        placementGhost.transform.position = placementNewPosition;
        if (!GetClosestSnapPoints(placementGhost.transform, 0.5f, out var a, out var b, __instance,
                plant.transform)) return;
        var vector4 = b.position - (a.position - placementGhost.transform.position);
        if (!IsPlantOverlapping(plant)) {
            placementGhost.transform.position = vector4;
        } else {
            piece.SetInvalidPlacementHeightlight(true);
            __instance.m_placementStatus = Player.PlacementStatus.Invalid;
        }
    }

    private static bool GetClosestSnapPoints(Component ghost, float maxSnapDistance, out Transform a, out Transform b,
        Player player, Transform plant) {
        m_tempSnapPoints1.Clear();
        m_tempSnapPoints1.Add(plant);
        m_tempSnapPoints2.Clear();
        m_tempPieces.Clear();
        GetSnapPoints(ghost.transform.position, 10f, m_tempSnapPoints2, m_tempPieces);
        var num = 9999999f;
        a = null;
        b = null;
        foreach (var item in m_tempSnapPoints1) {
            if (!player.FindClosestSnappoint(item.position, m_tempSnapPoints2, maxSnapDistance, out var closest,
                    out var distance) || !(distance < num)) continue;
            num = distance;
            a = item;
            b = closest;
        }

        return a != null;
    }

    private static bool IsPlantOverlapping(Component plant) {
        var plantCollider = plant.GetComponent<Collider>();
        var bounds = plantCollider.bounds;
        var num = Physics.OverlapSphereNonAlloc(bounds.center, 0.2f, pieceColliders);
        
        for (var i = 0; i < num; i++) {
            var collider = pieceColliders[i];
            if (collider == null) continue;
            // if (!collider.name.Contains("Clone")) continue;
            if (collider.name == plant.name) continue;
            if (collider.gameObject.GetComponent<Plant>() == null) continue;
            return true;
        }
        
        return false;
    }

    private static void GetSnapPoints(Vector3 point, float radius, ICollection<Transform> points,
        ICollection<Piece> pieces) {
        if (Piece.s_pieceRayMask == 0) Piece.s_pieceRayMask = LayerMask.GetMask("piece", "piece_nonsolid");
        var num = Physics.OverlapSphereNonAlloc(point, radius, pieceColliders, Piece.s_pieceRayMask);
        for (var i = 0; i < num; i++) {
            var componentInParent = pieceColliders[i].GetComponentInParent<Piece>();
            if (componentInParent == null) continue;
            if (!componentInParent.name.Contains("cultivatedGround")) continue;
            GetSnapPoints(points, componentInParent);
            pieces.Add(componentInParent);
        }
    }

    private static void GetSnapPoints(ICollection<Transform> points, Component piece) {
        for (var index = 0; index < piece.transform.childCount; ++index) {
            var child = piece.transform.GetChild(index);
            if (child.name.Contains("_snapplant"))
                points.Add(child);
        }
    }

    private static float GetCultivatedGroundHeight(Component plant, Player player) {
        var position = plant.transform.position;
        var playerHoveringPiece = player.m_hoveringPiece;
        if (playerHoveringPiece == null) return position.y;
        if (playerHoveringPiece.name.Contains("cultivatedGround")) {
            var collider = playerHoveringPiece.GetComponentInChildren<Collider>();
            return collider.transform.position.y;
        }

        var piecePos = playerHoveringPiece.transform.position;
        var positionBellow = new Vector3(piecePos.x, piecePos.y + 3f, piecePos.z);
        var ray = new Ray(positionBellow, Vector3.down);
        var hits = Physics.RaycastAll(ray);
        var hit = hits.OrderBy(x => Vector3.Distance(x.point, position)).FirstOrDefault(x => x.collider.gameObject.name == "Cultivated_ground_piece");
        return hit.collider == null ? position.y : hit.point.y;
    }

    [HarmonyPrefix]
    [HarmonyPatch(typeof(Piece), nameof(Piece.Awake))]
    private static void SetFakeComponent(Piece __instance) {
        var ts = __instance.transform.GetComponentsInChildren<Collider>();
        foreach (var t in ts)
            if (t.gameObject.name == "Cultivated_ground_piece")
                t.gameObject.AddComponent<CultivatedGround>();
    }

    [HarmonyPostfix]
    [HarmonyPriority(Priority.Last)]
    [HarmonyPatch(typeof(Heightmap), nameof(Heightmap.IsCultivated))]
    private static bool PatchIsCultivated(bool __result, Vector3 worldPos) {
        Plant.m_roofMask = 0;
        Plant.m_spaceMask = 0;
        var isCultivated = CheckIfItemBellowIsCultivatedGround(worldPos);
        if (!isCultivated) return __result;
        Plant.m_roofMask = LayerMask.GetMask("");
        Plant.m_spaceMask = LayerMask.GetMask("Default", "piece_nonsolid");
        return true;
    }

    private static bool IsPlantPlantable(Component plant) {
        var plantPosition = plant.transform.position;
        return CheckIfItemBellowIsCultivatedGround(new Vector3(plantPosition.x, plantPosition.y + 1f,
            plantPosition.z));
    }

    public static bool CheckIfItemBellowIsCultivatedGround(Vector3 position) {
        var layerMask = LayerMask.GetMask("piece");
        var positionAbove = new Vector3(position.x, position.y + 100f, position.z);
        var ray = new Ray(positionAbove, Vector3.down);
        var hits = Physics.RaycastAll(ray, 1000f, layerMask);
        return hits.Any(hit => hit.collider.gameObject.name == "Cultivated_ground_piece");
    }

    [HarmonyPrefix]
    [HarmonyPatch(typeof(Pickable), nameof(Pickable.Awake))]
    public static void FixPlantPhysics(Pickable __instance) {
        if (!PlantsArray.Any(__instance.name.Contains)) return;
        var staticPhysics = __instance.gameObject.GetComponent<StaticPhysics>();
        if (staticPhysics != null) staticPhysics.m_checkSolids = true;

        // Todo: make this check work with the raised bed
        // var plantPosition = __instance.gameObject.transform.position;
        // if (FixPlantHealth.CheckIfItemBellowIsCultivatedGround(new Vector3(plantPosition.x, plantPosition.y + 3f, plantPosition.z))) {
        //     StaticPhysics staticPhysics = __instance.gameObject.GetComponent<StaticPhysics>();
        //     staticPhysics.m_checkSolids = true;
        // }
    }

    [HarmonyPrefix]
    [HarmonyPriority(Priority.Last)]
    [HarmonyAfter("mod.valheim_plus", "org.bepinex.plugins.farming", "com.odinplusqol.mod", "BepIn.Sarcen.FarmGrid",
        "Harmony.Sarcen.FarmGrid")]
    [HarmonyPatch(typeof(Plant), nameof(Plant.Awake))]
    public static void FixPlantHealth(Plant __instance) {
        __instance.gameObject.AddComponent<FixPlantHealth>();
    }
}
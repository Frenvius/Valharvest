using System;
using System.Linq;
using HarmonyLib;
using UnityEngine;
using static Valharvest.Main;
using Random = UnityEngine.Random;

namespace Valharvest.Scripts;

[HarmonyPatch]
public class UseFertilizer {
    private static readonly string[] PickableArray = {"Thistle", "apple"};
    
    [HarmonyPrefix]
    [HarmonyPatch(typeof(Plant), nameof(Plant.Awake))]
    public static void AddPlantFertilizer(Plant __instance) {
        __instance.gameObject.AddComponent<InteractFertilizer>();
    }
    
    [HarmonyPrefix]
    [HarmonyPatch(typeof(Pickable), nameof(Pickable.UseItem))]
    private static bool AddThistleFertilizer(Pickable __instance, ref bool __result, Humanoid user, ItemDrop.ItemData item)
    {
        __result = true;
        if (!PickableArray.Any(__instance.name.Contains)) return true;
        if (!IsPickablePickable(__instance)) return true;
        var inventory = Player.m_localPlayer.GetInventory();
        var isSet = SetPickableTime(__instance, item);
        if (!isSet) return false;
        inventory.RemoveOneItem(item);
        GrowPickable(__instance);
        return false;
    }
    
    private static bool SetPickableTime(Pickable pickable, ItemDrop.ItemData item) {
        var nview = pickable.m_nview;
        var isBoneMeal = item.m_dropPrefab.name == "bonemeal";
        var isBucket = item.m_dropPrefab.name == "water_bucket";
        double growTime = pickable.m_respawnTimeMinutes;
        var divisor = isBoneMeal ? 8 : 4;
        if (!isBoneMeal && !isBucket) return false;
        var currentPickableTicks = nview.GetZDO().GetLong(ZDOVars.s_pickedTime, ZNet.instance.GetTime().Ticks);
        var fertilizerAmountTicks = (long) (growTime * 60 * TimeSpan.TicksPerSecond / divisor);
        var newPickableTicks = currentPickableTicks - fertilizerAmountTicks;
        if (newPickableTicks < 0) newPickableTicks = 0;
        nview.GetZDO().Set("picked_time", newPickableTicks);
        SendPickableEffect(pickable);
        return true;
    }
    
    private static void SendPickableEffect(Pickable pickable) {
        var num = Random.Range((float) (1f * 0.5), (float) (1f * 0.5));
        var baseRot = Quaternion.Euler(0.0f, Random.Range(0.0f, 360f), 0.0f);
        boneMealVfx.Create(pickable.transform.position, baseRot, scale: num);
    }
    
    private static bool IsPickablePickable(Pickable pickable) {
        var nview = pickable.m_nview;
        if (!nview.IsValid()) return false;
        DateTime dateTime = new DateTime(nview.GetZDO().GetLong(ZDOVars.s_pickedTime));
        return (ZNet.instance.GetTime() - dateTime).TotalMinutes <= pickable.m_respawnTimeMinutes;
    }
    
    private static void GrowPickable(Pickable pickable) {
        if (IsPickablePickable(pickable)) return;
        pickable.SetPicked(false);
    }
}
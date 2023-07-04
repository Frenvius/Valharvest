using System;
using UnityEngine;
using static Valharvest.Main;
using Object = UnityEngine.Object;
using Random = UnityEngine.Random;

namespace Valharvest.Scripts; 

public class InteractFertilizer : MonoBehaviour, Interactable {
    private ZNetView _nview;

    private void Awake() {
        _nview = gameObject.GetComponent<ZNetView>();
    }

    public bool Interact(Humanoid user, bool hold, bool alt) {
        return false;
    }

    public bool UseItem(Humanoid user, ItemDrop.ItemData item) {
        var inventory = Player.m_localPlayer.GetInventory();
        var plant = gameObject.GetComponent<Plant>();
        var isPlanted = SetPlantTime(plant, item);
        if (!isPlanted) return false;
        inventory.RemoveOneItem(item);
        GrowPlant(plant);
        return true;
    }
    
    private bool SetPlantTime(Plant plant, ItemDrop.ItemData item) {
        var isBoneMeal = item.m_dropPrefab.name == "bonemeal";
        var isBucket = item.m_dropPrefab.name == "water_bucket";
        double growTime = plant.m_growTime;
        var divisor = isBoneMeal ? 5 : 2;
        if (!isBoneMeal && !isBucket) return false;
        var currentPlantTicks = _nview.GetZDO().GetLong("plantTime", ZNet.instance.GetTime().Ticks);
        var fertilizerAmountTicks = (long) (growTime * TimeSpan.TicksPerSecond / divisor);
        var newPlantTicks = currentPlantTicks - fertilizerAmountTicks;
        if (newPlantTicks < 0) newPlantTicks = 0;
        _nview.GetZDO().Set("plantTime", newPlantTicks);
        SendPlantEffect(plant);
        return true;
    }

    private static void SendPlantEffect(Plant plant) {
        var num = Random.Range((float) (plant.m_minScale * 0.5), (float) (plant.m_maxScale * 0.5));
        var baseRot = Quaternion.Euler(0.0f, Random.Range(0.0f, 360f), 0.0f);
        boneMealVfx.Create(plant.transform.position, baseRot, scale: num);
    }

    public void GrowPlant(Plant plant) {
        if (!plant.m_nview.IsValid()) return;
        plant.m_updateTime = Time.time;
        var timeSincePlanted = plant.TimeSincePlanted();
        var growTime = plant.GetGrowTime();
        if ((bool) (Object) plant.m_healthyGrown) {
            var flag = timeSincePlanted > growTime * 0.5;
            plant.m_healthy.SetActive(!flag && plant.m_status == Plant.Status.Healthy);
            plant.m_unhealthy.SetActive(!flag && (uint) plant.m_status > 0U);
            plant.m_healthyGrown.SetActive(flag && plant.m_status == Plant.Status.Healthy);
            plant.m_unhealthyGrown.SetActive(flag && (uint) plant.m_status > 0U);
        } else {
            plant.m_healthy.SetActive(plant.m_status == Plant.Status.Healthy);
            plant.m_unhealthy.SetActive((uint) plant.m_status > 0U);
        }

        if (!plant.m_nview.IsOwner() || timeSincePlanted <= growTime) return;
        plant.Grow();
    }
}
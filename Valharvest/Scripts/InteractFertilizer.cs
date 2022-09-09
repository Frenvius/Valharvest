using System;
using UnityEngine;
using static Valharvest.Main;
using Object = UnityEngine.Object;
using Random = UnityEngine.Random;

public class InteractFertilizer : MonoBehaviour, Interactable {
    private ZNetView _nview;

    private void Awake() {
        _nview = gameObject.GetComponent<ZNetView>();
    }

    public bool Interact(Humanoid user, bool hold, bool alt) {
        return false;
    }

    public bool UseItem(Humanoid user, ItemDrop.ItemData item) {
        var isBoneMeal = item.m_dropPrefab.name == "bonemeal";
        var isBucket = item.m_dropPrefab.name == "water_bucket";
        var inventory = Player.m_localPlayer.GetInventory();
        if (!isBoneMeal && !isBucket) return false;

        var baseRot = Quaternion.Euler(0.0f, Random.Range(0.0f, 360f), 0.0f);
        var plant = gameObject.GetComponent<Plant>();
        double growTime = plant.m_growTime;
        var num = Random.Range((float) (plant.m_minScale * 0.5), (float) (plant.m_maxScale * 0.5));

        var divisor = isBoneMeal ? 5 : 2;
        var dateTime = new DateTime(_nview.GetZDO().GetLong("plantTime", ZNet.instance.GetTime().Ticks));
        var newTime = dateTime.AddSeconds(growTime / divisor * -1);
        _nview.GetZDO().Set("plantTime", newTime.Ticks);
        inventory.RemoveOneItem(item);

        boneMealVfx.Create(plant.transform.position, baseRot, scale: num);
        GrowPlant(plant);
        return true;
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
using System;
using Jotunn.Entities;
using Jotunn.Managers;
using UnityEngine;
using UnityEngine.Serialization;
using static Valharvest.Main;
using Random = UnityEngine.Random;

namespace Valharvest.Scripts;

public class MilkLox : MonoBehaviour {
    private const int MaxMilk = 3;
    private const float SecPerUnit = 800f;
    public Transform loxPoint;

    [FormerlySerializedAs("_nview")] public ZNetView nview;
    private readonly CustomItem _milkBottleFab = ItemManager.Instance.GetItem("milk_bottle");
    public readonly EffectList SpawnEffect = loxMilkSfx;

    private void Awake() {
        nview = GetComponent<ZNetView>();
        loxPoint = transform;
        if (nview.GetZDO() != null) {
            if (nview.IsOwner() && nview.GetZDO().GetLong("lastMilkTime") == 0L)
                nview.GetZDO().Set("lastMilkTime", ZNet.instance.GetTime().Ticks);
            nview.Register("ExtractMilk", RPC_ExtractMilk);
            nview.Register<int>("ResetLevel", RPC_ResetLevel);
            InvokeRepeating(nameof(UpdateMilk), 0f, 10f);
        }
    }

    private void RPC_ExtractMilk(long caller) {
        if (!nview.IsOwner()) return;
        var vector = Random.insideUnitCircle * 0.5f;
        var position = loxPoint.position + new Vector3(vector.x, 0.25f * 1, vector.y);
        Instantiate(_milkBottleFab.ItemDrop, position, Quaternion.identity);
    }

    private void RPC_ResetLevel(long caller, int level) {
        if (!nview.IsOwner()) return;
        var milkLevel = GetMilkLevel();
        nview.GetZDO().Set("milkLevel", milkLevel - level);
    }

    private float GetTimeSinceLastUpdate() {
        var dateTime = new DateTime(nview.GetZDO().GetLong("lastMilkTime", ZNet.instance.GetTime().Ticks));
        var time = ZNet.instance.GetTime();
        var timeSpan = time - dateTime;
        nview.GetZDO().Set("lastMilkTime", time.Ticks);
        var num = timeSpan.TotalSeconds;
        if (num < 0.0) num = 0.0;
        return (float)num;
    }

    private void IncreaseLevel(int i) {
        var milkLevel = GetMilkLevel();
        milkLevel += i;
        milkLevel = Mathf.Clamp(milkLevel, 0, MaxMilk);
        nview.GetZDO().Set("milkLevel", milkLevel);
    }

    public int GetMilkLevel() {
        return nview.GetZDO().GetInt("milkLevel");
    }

    public void UpdateMilk() {
        if (!nview.IsOwner()) return;
        var timeSinceLastUpdate = GetTimeSinceLastUpdate();
        var @float = nview.GetZDO().GetFloat("product");
        @float += timeSinceLastUpdate;
        if (@float > SecPerUnit) {
            var i = (int)(@float / SecPerUnit);
            IncreaseLevel(i);
            @float = 0f;
        }

        nview.GetZDO().Set("product", @float);
    }
}
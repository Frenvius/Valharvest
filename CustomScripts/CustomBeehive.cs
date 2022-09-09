using System;
using UnityEngine;
using Random = UnityEngine.Random;

public class CustomBeehive : MonoBehaviour, Hoverable, Interactable {
    public string m_name = "";
    
    public GameObject m_hideWhenPicked;

    public Transform m_spawnPoint;

    public float m_secPerUnit = 10f;

    public int m_maxHoney = 3;

    public ItemDrop m_honeyItem;

    public EffectList m_spawnEffect = new EffectList();

    private ZNetView m_nview;

    private void Awake() {
        m_nview = GetComponent<ZNetView>();
        if (m_nview.GetZDO() != null) {
            HidePrefab();
            if (m_nview.IsOwner() && m_nview.GetZDO().GetLong("lastTime", 0L) == 0L)
                m_nview.GetZDO().Set("lastTime", ZNet.instance.GetTime().Ticks);
            m_nview.Register("Extract", RPC_Extract);
            InvokeRepeating("UpdateBees", 0f, 10f);
            InvokeRepeating("HidePrefab", 0f, 5f);
        }
    }

    private void HidePrefab() {
        if ((bool)m_hideWhenPicked) {
            var honeyLevel = GetHoneyLevel();
            m_hideWhenPicked.SetActive(honeyLevel > 0);
        }
    }

    public string GetHoverText() {
        if (!PrivateArea.CheckAccess(base.transform.position, 0f, flash: false))
            return Localization.instance.Localize(m_name + "\n$piece_noaccess");
        var honeyLevel = GetHoneyLevel();
        if (honeyLevel > 0)
            return Localization.instance.Localize(m_name + " ( " + m_honeyItem.m_itemData.m_shared.m_name + " x " +
                                                  honeyLevel +
                                                  " )\n[<color=yellow><b>$KEY_Use</b></color>] $water_well_extract");
        return Localization.instance.Localize(m_name +
                                              " ( $piece_container_empty )\n[<color=yellow><b>$KEY_Use</b></color>] $water_well_check");
    }

    public string GetHoverName() {
        return m_name;
    }

    public bool Interact(Humanoid character, bool repeat, bool alt) {
        if (repeat) return false;
        if (!PrivateArea.CheckAccess(base.transform.position)) return true;
        if (GetHoneyLevel() > 0) {
            Extract();
        } else {
            character.Message(MessageHud.MessageType.Center, "$water_well_use");
        }

        return true;
    }

    public bool UseItem(Humanoid user, ItemDrop.ItemData item) {
        return false;
    }

    private void Extract() {
        m_nview.InvokeRPC("Extract");
    }

    private void RPC_Extract(long caller) {
        var honeyLevel = GetHoneyLevel();
        if (honeyLevel > 0) {
            if ((bool) m_hideWhenPicked) {
                m_hideWhenPicked.SetActive(false);
            }
            m_spawnEffect.Create(m_spawnPoint.position, Quaternion.identity);
            for (var i = 0; i < honeyLevel; i++) {
                var vector = Random.insideUnitCircle * 0.5f;
                var position = m_spawnPoint.position + new Vector3(vector.x, 0.25f * i, vector.y);
                Instantiate(m_honeyItem, position, Quaternion.identity);
            }

            ResetLevel();
        }
    }

    private float GetTimeSinceLastUpdate() {
        var dateTime = new DateTime(m_nview.GetZDO().GetLong("lastTime", ZNet.instance.GetTime().Ticks));
        DateTime time = ZNet.instance.GetTime();
        var timeSpan = time - dateTime;
        m_nview.GetZDO().Set("lastTime", time.Ticks);
        var num = timeSpan.TotalSeconds;
        if (num < 0.0) num = 0.0;
        return (float) num;
    }

    private void ResetLevel() {
        m_nview.GetZDO().Set("level", 0);
    }

    private void IncreseLevel(int i) {
        var honeyLevel = GetHoneyLevel();
        honeyLevel += i;
        honeyLevel = Mathf.Clamp(honeyLevel, 0, m_maxHoney);
        m_nview.GetZDO().Set("level", honeyLevel);
    }

    private int GetHoneyLevel() {
        return m_nview.GetZDO().GetInt("level");
    }

    private void UpdateBees() {
        var honeyLevel = GetHoneyLevel();
        if (honeyLevel > 0) {
            if ((bool) m_hideWhenPicked) {
                m_hideWhenPicked.SetActive(true);
            }
        }
        if (m_nview.IsOwner()) {
            var timeSinceLastUpdate = GetTimeSinceLastUpdate();
            float @float = m_nview.GetZDO().GetFloat("product");
            @float += timeSinceLastUpdate;
            if (@float > m_secPerUnit) {
                var i = (int) (@float / m_secPerUnit);
                IncreseLevel(i);
                @float = 0f;
            }

            m_nview.GetZDO().Set("product", @float);
        }
    }
}
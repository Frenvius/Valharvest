using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Logger = Jotunn.Logger;

namespace Valharvest.Scripts;

public class FoodCrate : MonoBehaviour {
	public const string VH_PILE_ITEM_KEY = "vh_pile_item";
	public const string VH_MIGRATED_KEY = "vh_migrated";
	private const string VH_GROUNDED_KEY = "vh_grounded";
	private const string RPC_REFRESH = "VH_RefreshPile";
	private const string MAT_PREFIX = "Pile";

	private const float GroundSnapEpsilon = 0.05f;
	private const int GroundSettlePasses = 24;

	private static Dictionary<string, Material> _materialCache;

	private Container _container;
	private ZNetView _nview;
	private GameObject[] _piles;

	private void Awake() {
		_container = GetComponent<Container>();
		_nview = GetComponent<ZNetView>();

		Transform model = transform.Find("model");
		if (model != null)
			_piles = new[] {
				model.Find("pile_1_25")?.gameObject,
				model.Find("pile_2_50")?.gameObject,
				model.Find("pile_3_75")?.gameObject,
				model.Find("pile_4_100")?.gameObject
			};

		_nview?.Register(RPC_REFRESH, _ => RefreshVisual());
	}

	private void Start() {
		if (_container?.m_inventory != null)
			_container.m_inventory.m_onChanged += OnInventoryChanged;

		StartCoroutine(GroundOnceDeferred());
		RefreshVisual();
	}

	private void OnDestroy() {
		if (_container?.m_inventory != null)
			_container.m_inventory.m_onChanged -= OnInventoryChanged;
	}

	private IEnumerator GroundOnceDeferred() {
		yield return null;
		yield return null;
		yield return null;

		if (_nview == null || !_nview.IsValid() || !_nview.IsOwner())
			yield break;

		ZDO zdo = _nview.GetZDO();
		if (zdo.GetBool(VH_GROUNDED_KEY))
			yield break;

		int mask = LayerMask.GetMask("Default", "static_solid", "Default_small",
			"piece", "terrain", "vehicle");

		for (int pass = 0; pass < GroundSettlePasses; pass++) {
			Vector3 pos = transform.position;
			RaycastHit[] hits = Physics.RaycastAll(pos + Vector3.up * 0.1f, Vector3.down, 50f, mask);
			Array.Sort(hits, (a, b) => a.distance.CompareTo(b.distance));

			float groundY = float.NaN;
			foreach (RaycastHit h in hits) {
				if (h.collider.transform.root == transform) continue; // skip self
				groundY = h.point.y;
				break;
			}

			if (!float.IsNaN(groundY) && pos.y - groundY > GroundSnapEpsilon) {
				pos.y = groundY;
				transform.position = pos;
				zdo.SetPosition(pos);
			}

			yield return null;
		}

		zdo.Set(VH_GROUNDED_KEY, true);
	}

	private void OnInventoryChanged() {
		if (_nview == null || !_nview.IsValid() || !_nview.IsOwner()) return;

		List<ItemDrop.ItemData> items = _container.m_inventory.GetAllItems();
		ZDO zdo = _nview.GetZDO();
		string current = zdo.GetString(VH_PILE_ITEM_KEY);

		if (items.Count == 0) {
			if (!string.IsNullOrEmpty(current)) zdo.Set(VH_PILE_ITEM_KEY, "");
		} else if (string.IsNullOrEmpty(current)) {
			string prefabName = ResolvePrefabName(items[0]);
			if (!string.IsNullOrEmpty(prefabName)) zdo.Set(VH_PILE_ITEM_KEY, prefabName);
		}

		_nview.InvokeRPC(ZNetView.Everybody, RPC_REFRESH);
	}

	private void RefreshVisual() {
		if (_piles == null || _container?.m_inventory == null) return;

		for (int i = 0; i < _piles.Length; i++)
			if (_piles[i] != null)
				_piles[i].SetActive(false);

		int usedSlots = _container.m_inventory.GetAllItems().Count;
		if (usedSlots == 0) return;

		int totalSlots = _container.m_inventory.GetWidth() * _container.m_inventory.GetHeight();
		float pct = totalSlots > 0 ? (float)usedSlots / totalSlots : 0f;
		int pileIdx = pct <= 0.25f ? 0
			: pct <= 0.50f ? 1
			: pct <= 0.75f ? 2
			: 3;

		GameObject pile = _piles[pileIdx];
		if (pile == null) return;
		pile.SetActive(true);

		string lockedItem = _nview != null && _nview.IsValid()
			? _nview.GetZDO().GetString(VH_PILE_ITEM_KEY)
			: "";

		Material mat = GetPileMaterial(lockedItem);
		if (mat == null) return;

		var renderer = pile.GetComponent<Renderer>();
		if (renderer != null) renderer.sharedMaterial = mat;
	}

	private static string ResolvePrefabName(ItemDrop.ItemData item) {
		if (item?.m_dropPrefab != null) return item.m_dropPrefab.name;

		string shared = item?.m_shared?.m_name;
		if (!string.IsNullOrEmpty(shared) && shared.StartsWith("$") && shared.EndsWith("_name"))
			return shared.Substring(1, shared.Length - "$_name".Length + 1);

		return null;
	}

	private static Material GetPileMaterial(string itemPrefabName) {
		if (string.IsNullOrEmpty(itemPrefabName)) return null;
		if (_materialCache == null) BuildCache();

		string key = MAT_PREFIX + char.ToUpper(itemPrefabName[0]) + itemPrefabName.Substring(1);
		_materialCache.TryGetValue(key, out Material mat);
		return mat;
	}

	private static void BuildCache() {
		_materialCache = new Dictionary<string, Material>(StringComparer.OrdinalIgnoreCase);
		if (Main.modAssets == null) {
			Logger.LogWarning("[FoodCrate] modAssets is null; can't build pile material cache");
			return;
		}

		foreach (Material mat in Main.modAssets.LoadAllAssets<Material>()) {
			if (mat == null || !mat.name.StartsWith(MAT_PREFIX)) continue;
			string key = mat.name.Split('_')[0];
			_materialCache[key] = mat;
		}

		Logger.LogInfo($"[FoodCrate] Cached {_materialCache.Count} pile materials");
	}
}

using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Logger = Jotunn.Logger;

namespace Valharvest.Scripts;

public class LegacyBoxMigrator : MonoBehaviour {
	private const int MigrateAmount = 50;

	private static readonly Dictionary<string, string> BoxToItem = new() {
		{ "piece_appleBox", "apple" },
		{ "piece_pepperBox", "pepper" },
		{ "piece_garlicBox", "garlic" },
		{ "piece_potatoBox", "potato" },
		{ "piece_saltBox", "salt" },
		{ "piece_tomatoBox", "tomato" }
	};

	private ZNetView _nview;

	private void Awake() {
		_nview = GetComponent<ZNetView>();
		if (_nview == null) return;
		StartCoroutine(MigrateDeferred());
	}

	private IEnumerator MigrateDeferred() {
		yield return null;

		if (_nview == null || !_nview.IsValid() || !_nview.IsOwner())
			yield break;

		string boxName = global::Utils.GetPrefabName(gameObject);
		if (!BoxToItem.TryGetValue(boxName, out string itemName))
			yield break;

		if (ZNetScene.instance == null || ObjectDB.instance == null)
			yield break;

		GameObject cratePrefab = ZNetScene.instance.GetPrefab("vh_piece_food_crate");
		if (cratePrefab == null) {
			Logger.LogWarning($"[LegacyBoxMigrator] vh_piece_food_crate not found; skipping {boxName}");
			yield break;
		}

		Quaternion spawnRot = Quaternion.Euler(0f, transform.eulerAngles.y, 0f);
		GameObject crate = Instantiate(cratePrefab, transform.position, spawnRot);

		var container = crate.GetComponent<Container>();
		GameObject itemPrefab = ObjectDB.instance.GetItemPrefab(itemName);
		if (container != null && itemPrefab != null) {
			var itemDrop = itemPrefab.GetComponent<ItemDrop>();
			int quality = itemDrop != null ? itemDrop.m_itemData.m_quality : 1;
			container.GetInventory().AddItem(itemName, MigrateAmount, quality, 0, 0L, "");
			container.Save();

			var crateNview = crate.GetComponent<ZNetView>();
			ZDO crateZdo = crateNview?.GetZDO();
			if (crateZdo != null) {
				crateZdo.Set(FoodCrate.VH_PILE_ITEM_KEY, itemName);
				crateZdo.Set(FoodCrate.VH_MIGRATED_KEY, true);
			}

			Logger.LogInfo($"[LegacyBoxMigrator] {boxName} -> vh_piece_food_crate ({itemName} x{MigrateAmount})");
		} else {
			Logger.LogWarning($"[LegacyBoxMigrator] {boxName}: missing Container or item prefab '{itemName}'");
		}

		var piece = crate.GetComponent<Piece>();
		if (piece != null && Player.m_localPlayer != null)
			piece.SetCreator(Player.m_localPlayer.GetPlayerID());

		ZNetScene.instance.Destroy(gameObject);
	}
}

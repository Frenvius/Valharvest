using System;
using System.Linq;
using Jotunn.Managers;
using Jotunn.Utils;
using UnityEngine;
using static Valharvest.Main;
using static Valharvest.Utils;
using Logger = Jotunn.Logger;

namespace Valharvest.Scripts;

public static class BoneAppetitBalance {
	public static void SeagullEgg() {
		try {
			var seagullEggFab = PrefabManager.Instance.GetPrefab("rk_egg");

			foreach (var mat in from rend in ShaderHelper.GetRenderers(seagullEggFab)
			         from mat in rend.materials
			         where mat.HasProperty("_MainTex")
			         select mat)
				mat.SetTexture(MainTex, newEggTexture);

			var seagullEgg = ItemManager.Instance.GetItem("rk_egg");

			var itemDrop = seagullEgg.ItemDrop;
			itemDrop.m_itemData.m_shared.m_description = "A small egg.";
			itemDrop.m_itemData.m_shared.m_icons[0] = newEggSprite;
		} catch (Exception ex) {
			Logger.LogError($"Error while loading SeagullEgg: {ex.Message}");
		} finally {
			Logger.LogInfo("SeagullEgg Loaded.");
		}
	}

	public static void Kabob() {
		var kabob = ItemManager.Instance.GetItem("rk_kabob");
		if (Configurations.Valharvest.KabobName.Value) kabob.ItemDrop.m_itemData.m_shared.m_name = "Kebab";
	}

	public static void Pizza() {
		var pizza = ItemManager.Instance.GetItem("rk_pizza");
		var pizzaFab = PrefabManager.Instance.GetPrefab("rk_pizza");
		foreach (var rend in ShaderHelper.GetRenderers(pizzaFab))
			switch (rend.name) {
				case "pizza": {
					foreach (var mat in rend.materials) {
						ColorUtility.TryParseHtmlString("#FFFFFF", out var rgbColor);
						mat.SetColor(Utils.Color, rgbColor);
					}

					break;
				}
				case "Tarelka001": {
					foreach (var mat in rend.materials) {
						ColorUtility.TryParseHtmlString("#855B41", out var rgbColor);
						mat.SetTexture(MainTex, pizzaPlateTexture);
						mat.SetColor(Utils.Color, rgbColor);
					}

					break;
				}
			}

		var itemDrop = pizza.ItemDrop;
		itemDrop.m_itemData.m_shared.m_icons[0] = pizzaSprite;
	}
}
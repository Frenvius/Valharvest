using System;
using Jotunn.Managers;
using Jotunn.Utils;
using UnityEngine;
using static Valharvest.Main;
using static Valharvest.Utils;

namespace Valharvest.Scripts {
    public static class BoneAppetitBalance {
        public static void SeagullEgg() {
            try {
                var seagullEggFab = PrefabManager.Instance.GetPrefab("rk_egg");

                foreach (var rend in ShaderHelper.GetRenderers(seagullEggFab))
                foreach (var mat in rend.materials)
                    if (mat.HasProperty("_MainTex"))
                        mat.SetTexture(MainTex, newEggTexture);

                var seagullEgg = ItemManager.Instance.GetItem("rk_egg");

                var itemDrop = seagullEgg.ItemDrop;
                itemDrop.m_itemData.m_shared.m_description = "A small egg.";
                itemDrop.m_itemData.m_shared.m_icons[0] = newEggSprite;
            } catch (Exception ex) {
                Jotunn.Logger.LogError($"Error while loading SeagullEgg: {ex.Message}");
            } finally {
                Jotunn.Logger.LogInfo("SeagullEgg Loaded.");
            }
        }

        public static void Kabob() {
            var kabob = ItemManager.Instance.GetItem("rk_kabob");
            if (kabobName.Value) kabob.ItemDrop.m_itemData.m_shared.m_name = "Kebab";
        }

        public static void Pizza() {
            var pizza = ItemManager.Instance.GetItem("rk_pizza");
            var pizzaFab = PrefabManager.Instance.GetPrefab("rk_pizza");
            foreach (var rend in ShaderHelper.GetRenderers(pizzaFab)) {
                if (rend.name == "pizza") {
                    foreach (var mat in rend.materials) {
                        ColorUtility.TryParseHtmlString("#FFFFFF", out var rgbColor);
                        mat.SetColor(Utils.Color, rgbColor);   
                    }
                } else if (rend.name == "Tarelka001") {
                    foreach (var mat in rend.materials) {
                        ColorUtility.TryParseHtmlString("#855B41", out var rgbColor);
                        mat.SetTexture(MainTex, pizzaPlateTexture);
                        mat.SetColor(Utils.Color, rgbColor);
                    }
                }
            }
            
            var itemDrop = pizza.ItemDrop;
            itemDrop.m_itemData.m_shared.m_icons[0] = pizzaSprite;
        }
    }
}
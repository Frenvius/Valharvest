using System;
using System.Collections.Generic;
using Jotunn.Entities;
using Jotunn.Managers;
using Jotunn.Utils;
using SimpleJson;
using UnityEngine;
using Valharvest.Scripts;
using static Valharvest.Utils;
using static SimpleJson.SimpleJson;

namespace Valharvest.WorldGen {
    public static class PlantUtils {
        public static void AddCustomPlantsPrefab() {
            var jsonContent = ReadEmbeddedFile("plants.resources");
            if (jsonContent != null) {
                var plantJson = DeserializeObject<JsonObject>(jsonContent);
                foreach (var plant in plantJson) {
                    var assetBundle = Loaders.GetAssetBundle();
                    var plantObject = DeserializeObject<JsonObject>(plant.Value.ToString());
                    var bundle = plantObject["assetBundle"].ToString();
                    var dropString = plantObject["dropConfig"];
                    var respawnConfig = plantObject["respawnConfig"];
                    var dropConfigs = Configurations.Valharvest.GetDropConfigs();
                    var respawnConfigs = Configurations.Valharvest.GetRespawnTimeConfigs();
                    var plantPrefab = assetBundle[bundle].LoadAsset<GameObject>(plant.Key);
                    
                    LoadPlantMaterials(plantPrefab, plantObject);

                    try {
                        if (dropString != null) {
                            var config = dropConfigs[dropString.ToString()];
                            var pickable = plantPrefab.GetComponent<Pickable>();
                            pickable.m_amount = config.Value;
                        }
                        
                        if (respawnConfig != null) {
							var config = respawnConfigs[respawnConfig.ToString()];
							var pickable = plantPrefab.GetComponent<Pickable>();
							pickable.m_respawnTimeMinutes = config.Value * 60;
                        }

                        if (plantObject["crafting"] != null) {
                            var plantItem = Loaders.CreatePieceRecipe(plantPrefab, plantObject);
                            PieceManager.Instance.AddPiece(plantItem);
                        } else {
                            PrefabManager.Instance.AddPrefab(new CustomPrefab(plantPrefab, true));   
                        }
                    } catch (Exception ex) {
                        Jotunn.Logger.LogError($"Error while loading {plant.Key}: {ex.Message}");
                    } finally {
                        PrefabManager.OnVanillaPrefabsAvailable -= AddCustomPlantsPrefab;
                    }
                }
            }
        }

        private static void LoadPlantMaterials(GameObject plantPrefab, JsonObject plantObject) {
            var plantMaterials = DeserializeObject<JsonObject>(plantObject["materials"].ToString());
            foreach (var renderer in ShaderHelper.GetRenderers(plantPrefab)) {
                foreach (var material in renderer.sharedMaterials) {
                    if (material == null) continue;
                    var matName = material.name;
                    if (plantMaterials.ContainsKey(matName)) {
                        if (!string.IsNullOrEmpty(plantMaterials[matName].ToString())) {
                            Dictionary<Type, int> typeDict = GetTypeDict();
                            Dictionary<string, int> getMatItem = GetMatItem();
                            var matObject = DeserializeObject<JsonObject>(plantMaterials[matName].ToString());
                            var shader = PrefabManager.Cache.GetPrefab<Shader>(matObject["shader"].ToString());
                            var texture = material.mainTexture;

                            ConfigureMaterial(material, shader, matObject, typeDict, getMatItem);
                            material.SetTexture(MainTex, texture);
                        }
                    }
                }
            }
        }
    }
}
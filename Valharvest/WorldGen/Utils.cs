using System;
using System.Collections.Generic;
using Jotunn.Entities;
using Jotunn.Managers;
using Jotunn.Utils;
using SimpleJson;
using UnityEngine;
using static Valharvest.Utils;
using static SimpleJson.SimpleJson;
using static Valharvest.Main;
using static Valharvest.Scripts.Loaders;

namespace Valharvest.WorldGen {
    public static class PlantUtils {
        public static void AddCustomPlantsPrefab() {
            var jsonContent = ReadEmbeddedFile("plants.resources");
            if (jsonContent != null) {
                var plantJson = DeserializeObject<JsonObject>(jsonContent);
                foreach (var plant in plantJson) {
                    var plantPrefab = plantAssets.LoadAsset<GameObject>(plant.Key);
                    var plantObject = DeserializeObject<JsonObject>(plant.Value.ToString());
                    var plantMaterials = DeserializeObject<JsonObject>(plantObject["materials"].ToString());
                    foreach (var renderer in ShaderHelper.GetRenderers(plantPrefab)) {
                        foreach (var material in renderer.materials) {
                            const string materialInstance = " (Instance)";
                            var matName = material.name;
                            var matRealName = matName.Substring(0, matName.Length - materialInstance.Length);
                            if (plantMaterials.ContainsKey(matRealName)) {
                                if (!string.IsNullOrEmpty(plantMaterials[matRealName].ToString())) {
                                    Dictionary<Type, int> typeDict = GetTypeDict();
                                    Dictionary<string, int> getMatItem = GetMatItem();
                                    var matObject = DeserializeObject<JsonObject>(plantMaterials[matRealName].ToString());
                                    var shader = Shader.Find(matObject["shader"].ToString());
                                    var texture = material.mainTexture;

                                    ConfigureMaterial(material, shader, matObject, typeDict, getMatItem);
                                    material.SetTexture(MainTex, texture);
                            
                                    // var materialProperties = material.GetMaterialProperties();   
                                }   
                            }
                        }
                    }

                    try {
                        if (plantObject["crafting"] != null) {
                            var plantItem = CreatePieceRecipe(plantPrefab, plantObject);
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
    }
}
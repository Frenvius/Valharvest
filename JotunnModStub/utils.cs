﻿using System;
using System.Collections.Generic;
using System.IO;
using System.Reflection;
using Jotunn.Configs;
using Jotunn.Entities;
using Jotunn.Managers;
using Jotunn.Utils;
using SimpleJson;
using UnityEngine;
using static SimpleJson.SimpleJson;
using static Valharvest.Main;

namespace Valharvest {
    public static class Utils {
            public static readonly int MainTex = Shader.PropertyToID("_MainTex");
            public static readonly int Color = Shader.PropertyToID("_Color");

            private const string Name = "name";
            private const string Food = "food";
            private const string Weight = "weight";
            private const string Variants = "variants";
            private const string FoodRegen = "foodRegen";
            private const string FoodColor = "foodColor";
            private const string FoodStamina = "foodStamina";
            private const string Description = "description";
            private const string MaxStackSize = "maxStackSize";
            private const string FoodBurnTime = "foodBurnTime";
            private const string ConsumeStatusEffect = "consumeStatusEffect";
            private static readonly int Cull = Shader.PropertyToID("_Cull");
            private static readonly int Cutoff = Shader.PropertyToID("_Cutoff");
            private static readonly int Height = Shader.PropertyToID("_Height");
            private static readonly int AddSnow = Shader.PropertyToID("_AddSnow");
            private static readonly int AddRain = Shader.PropertyToID("_AddRain");
            private static readonly int CamCull = Shader.PropertyToID("_CamCull");
            private static readonly int SwaySpeed = Shader.PropertyToID("_SwaySpeed");
            private static readonly int MossNormal = Shader.PropertyToID("_MossNormal");
            private static readonly int Glossiness = Shader.PropertyToID("_Glossiness");
            private static readonly int RippleSpeed = Shader.PropertyToID("_RippleSpeed");
            private static readonly int SwayDistance = Shader.PropertyToID("_SwayDistance");
            private static readonly int PushDistance = Shader.PropertyToID("_PushDistance");
            private static readonly int MossTransition = Shader.PropertyToID("_MossTransition");
            private static readonly int RippleDistance = Shader.PropertyToID("_RippleDistance");
            private static readonly int FadeDistanceMin = Shader.PropertyToID("_FadeDistanceMin");
            private static readonly int FadeDistanceMax = Shader.PropertyToID("_FadeDistanceMax");
            private static readonly int RippleDeadzoneMin = Shader.PropertyToID("_RippleDeadzoneMin");
            private static readonly int RippleDeadzoneMax = Shader.PropertyToID("_RippleDeadzoneMax");

            private static Texture2D _texture;

            public static void ChangePlantShader(GameObject itemPrefab, string configObject) {
                Dictionary<Type, int> typeDict = GetTypeDict();
                Dictionary<string, int> getMatItem = GetMatItem();

                using var stream = Assembly.GetExecutingAssembly()
                    .GetManifestResourceStream("plantsShaderConfig.resources");

                if (stream != null) ConfigureShader(itemPrefab, configObject, stream, typeDict, getMatItem);
            }

            private static void ConfigureShader(GameObject itemPrefab, string configObject, Stream stream,
                Dictionary<Type, int> typeDict,
                Dictionary<string, int> getMatItem) {
                var content = GetContent(configObject, stream);
                var shader = Shader.Find(content["shader"].ToString());

                if (shader != null)
                    foreach (var renderer in ShaderHelper.GetRenderers(itemPrefab))
                        ConfigureRendererMaterials(renderer, content, shader, typeDict, getMatItem);
            }

            private static void ConfigureRendererMaterials(Renderer renderer, JsonObject content, Shader shader,
                Dictionary<Type, int> typeDict,
                Dictionary<string, int> getMatItem) {
                if (HasFoodName(renderer)) {
                    SetTexture(renderer, content);

                    foreach (var material in renderer.materials)
                        ConfigureMaterial(material, shader, content, typeDict, getMatItem);
                }
            }

            private static void ConfigureMaterial(Material material, Shader shader, JsonObject content,
                Dictionary<Type, int> typeDict,
                Dictionary<string, int> getMatItem) {
                material.shader = shader;
                
                if (material.HasProperty(MainTex)) material.SetTexture(MainTex, _texture);

                var materialConfig = DeserializeObject<JsonObject>(content["material"].ToString());

                foreach (var materialItem in materialConfig)
                    SetMaterialConfigItems(typeDict, materialItem, material, getMatItem);
            }

            private static void SetMaterialConfigItems(
                Dictionary<Type, int> typeDict, KeyValuePair<string, object> materialItem, Material material, Dictionary<string, int> getMatItem
            ) {
                switch (typeDict[materialItem.Value.GetType()]) {
                    case 0:
                        material.SetFloat(getMatItem[materialItem.Key], float.Parse(materialItem.Value.ToString()));
                        break;
                    case 1:
                        ColorUtility.TryParseHtmlString(materialItem.Value.ToString(), out var rgbColor);
                        material.SetColor(Color, rgbColor);
                        break;
                    case 2:
                        material.SetFloat(getMatItem[materialItem.Key], float.Parse(materialItem.Value.ToString()));
                        break;
                }
            }

            private static void SetTexture(Renderer rend, JsonObject content) {
                if (rend.name.Contains("unhealthy"))
                    _texture = modAssets.LoadAsset<Texture2D>(content["texture"] + "-unhealthy");
                else
                    _texture = modAssets.LoadAsset<Texture2D>(content["texture"].ToString());
            }

            private static bool HasFoodName(Renderer rend) {
                return (rend.name == "blast") |
                       (rend.name == "flower") |
                       (rend.name == "healthy") |
                       (rend.name == "unhealthy") |
                       (rend.name == "healthy_grown") |
                       (rend.name == "unhealthy_grown") |
                       (rend.name == "healthy_rice") |
                       (rend.name == "unhealthy_rice");
            }

            private static JsonObject GetContent(string configObject, Stream stream) {
                string fileContents = new StreamReader(stream).ReadToEnd();
                var json = DeserializeObject<JsonObject>(fileContents);
                var content = DeserializeObject<JsonObject>(json[configObject].ToString());
                return content;
            }

            private static Dictionary<Type, int> GetTypeDict() {
                return new Dictionary<Type, int> {{typeof(Int64), 0}, {typeof(string), 1}, {typeof(double), 2}};
            }

            private static Dictionary<string, int> GetMatItem() {
                return new Dictionary<string, int> {
                    {"glossiness", Glossiness},
                    {"cutoff", Cutoff},
                    {"mossNormal", MossNormal},
                    {"mossTransition", MossTransition},
                    {"addSnow", AddSnow},
                    {"addRain", AddRain},
                    {"height", Height},
                    {"swaySpeed", SwaySpeed},
                    {"swayDistance", SwayDistance},
                    {"rippleSpeed", RippleSpeed},
                    {"rippleDistance", RippleDistance},
                    {"rippleDeadzoneMin", RippleDeadzoneMin},
                    {"rippleDeadzoneMax", RippleDeadzoneMax},
                    {"pushDistance", PushDistance},
                    {"camCull", CamCull},
                    {"cull", Cull},
                    {"color", Color},
                    {"fadeDistanceMin", FadeDistanceMin},
                    {"fadeDistanceMax", FadeDistanceMax}
                };
            }

            public static void ChangeItemDrop(
                CustomItem item,
                string configObject
            ) {
                using var stream = Assembly.GetExecutingAssembly()
                    .GetManifestResourceStream("BoneAppetitBalance.resources");
                if (stream != null) {
                    string fileContents = new StreamReader(stream).ReadToEnd();
                    var json = DeserializeObject<JsonObject>(fileContents);
                    var content = DeserializeObject<JsonObject>(json[configObject].ToString());
                    var itemDrop = item.ItemDrop.m_itemData.m_shared;

                    SetItemDropFromContent(content, itemDrop);
                }
            }
            
            public static void LoadNewFood() {
                using var stream = Assembly.GetExecutingAssembly().GetManifestResourceStream("newFoodsConfig.resources");
                if (stream != null) {
                    TextReader tr = new StreamReader(stream);
                    string fileContents = tr.ReadToEnd();
                    var foodJson = DeserializeObject<JsonObject>(fileContents);
                    foreach (var food in foodJson) {
                        var foodName = food.Key;
                        var foodObject = DeserializeObject<JsonObject>(food.Value.ToString());
                        var requirementsArr = DeserializeObject<RequirementConfig[]>(foodObject["requirements"].ToString());
                        CustomItem foodItem = null;
                        try {
                            var foodFab = foodAssets.LoadAsset<GameObject>(foodName);
                            foodItem = new CustomItem(foodFab, true,
                                new ItemConfig {
                                    Name = foodObject["name"].ToString(),
                                    Enabled = true,
                                    Amount = Convert.ToInt32(foodObject["amount"].ToString()),
                                    CraftingStation = foodObject["craftingStation"].ToString(),
                                    Requirements = requirementsArr
                                });

                            ChangeFoodDrop(foodItem, foodName);
                        } catch (Exception ex) {
                            Jotunn.Logger.LogError($"Error while loading {foodName}: {ex.Message}");
                        } finally {
                            // Jotunn.Logger.LogInfo($"{foodName} Loaded.");
                            ItemManager.Instance.AddItem(foodItem);
                        }
                    }
                    PrefabManager.OnVanillaPrefabsAvailable -= LoadNewFood;
                }
            }
            
            public static void LoadBalancedFood() {
                using var stream = Assembly.GetExecutingAssembly().GetManifestResourceStream("BoneAppetitBalance.resources");
                if (stream != null) {
                    TextReader tr = new StreamReader(stream);
                    string fileContents = tr.ReadToEnd();
                    var foodJson = DeserializeObject<JsonObject>(fileContents);
                    foreach (var food in foodJson) {
                        var foodName = food.Key;
                        var foodObject = DeserializeObject<JsonObject>(food.Value.ToString());
                        var requirementsArr = DeserializeObject<RequirementConfig[]>(foodObject["requirements"].ToString());
                        RegisterFood(foodName, foodName, Convert.ToInt32(foodObject["amount"].ToString()),
                            foodObject["craftingStation"].ToString(), requirementsArr);
                    }
                    ItemManager.OnItemsRegisteredFejd -= LoadBalancedFood;
                }
            }
            
            public static void ChangeFoodDrop(
                CustomItem item,
                string configObject
            ) {
                using var stream = Assembly.GetExecutingAssembly()
                    .GetManifestResourceStream("newFoodsConfig.resources");
                if (stream != null) {
                    string fileContents = new StreamReader(stream).ReadToEnd();
                    var json = DeserializeObject<JsonObject>(fileContents);
                    var content = DeserializeObject<JsonObject>(json[configObject].ToString());
                    var itemDrop = item.ItemDrop.m_itemData.m_shared;

                    SetItemDropFromContent(content, itemDrop);
                }
            }

            private static void SetItemDropFromContent(JsonObject content, ItemDrop.ItemData.SharedData itemDrop) {
                if (content[Name] != null) itemDrop.m_name = content[Name].ToString();
                if (content[Description] != null) itemDrop.m_description = content[Description].ToString();
                if (content[Weight] != null) itemDrop.m_weight = float.Parse(content[Weight].ToString());
                if (content[MaxStackSize] != null)
                    itemDrop.m_maxStackSize = int.Parse(content[MaxStackSize].ToString());
                if (content[Variants] != null) itemDrop.m_variants = int.Parse(content[Variants].ToString());
                if (content[Food] != null) itemDrop.m_food = float.Parse(content[Food].ToString());
                if (content[FoodStamina] != null) itemDrop.m_foodStamina = float.Parse(content[FoodStamina].ToString());
                if (content[FoodBurnTime] != null)
                    itemDrop.m_foodBurnTime = float.Parse(content[FoodBurnTime].ToString()) * 60;
                if (content[FoodRegen] != null) itemDrop.m_foodRegen = float.Parse(content[FoodRegen].ToString());
                
                if (content[ConsumeStatusEffect] != null) {
                    var statusEffect = modAssets.LoadAsset<StatusEffect>(content[ConsumeStatusEffect].ToString());
                    itemDrop.m_consumeStatusEffect = statusEffect;
                }
                if (content[FoodColor] != null) {
                    ColorUtility.TryParseHtmlString(content[FoodColor].ToString(), out var rgbColor);
                    itemDrop.m_foodColor = rgbColor;
                }
            }

            public static void RegisterFood(string name, string prefab, int amount, string craftingStation,
                RequirementConfig[] requirements) {
                var itemPrefab = ItemManager.Instance.GetItem(prefab);
                ItemManager.Instance.RemoveRecipe("Recipe_" + prefab);
                try {
                    var prefabRecipe = new CustomRecipe(new RecipeConfig {
                        Item = prefab,
                        Amount = amount,
                        CraftingStation = craftingStation,
                        Requirements = requirements
                    });
                    ChangeItemDrop(itemPrefab, prefab);
                    ItemManager.Instance.AddRecipe(prefabRecipe);
                } catch (Exception ex) {
                    Jotunn.Logger.LogError($"Error while loading {name}: {ex.Message}");
                }
            }
            
            public static string ReadEmbeddedFile(string embeddedName) {
                using var stream = Assembly.GetExecutingAssembly().GetManifestResourceStream(embeddedName);
                if (stream == null) return null;
                TextReader tr = new StreamReader(stream);
                var fileContents = tr.ReadToEnd();
                return fileContents;
            }

            public static void GetRequirements(string requirements) {
                Jotunn.Logger.LogInfo($"requirements1: {requirements}");
                var requirementsArr = DeserializeObject<List<RequirementConfig>>(requirements);
                Jotunn.Logger.LogInfo($"requirements2: {requirementsArr}");
                foreach (var req in requirementsArr) {
                    Jotunn.Logger.LogInfo($"item: {req}");
                }
                // var requirements = new RequirementConfig[];
            }
        }
}
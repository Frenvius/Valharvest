using System;
using System.Reflection;
using BepInEx;
using BepInEx.Configuration;
using HarmonyLib;
using Jotunn.Entities;
using Jotunn.Managers;
using Jotunn.Utils;
using UnityEngine;
using static Valharvest.Utils;
using static Valharvest.Scripts.Loaders;
using static Valharvest.WorldGen.Plants;
using static Valharvest.WorldGen.PlantUtils;
using static Valharvest.Scripts.BoneAppetitBalance;

namespace Valharvest {
    [BepInPlugin(ModGuid, ModName, Version)]
    [BepInDependency(Jotunn.Main.ModGuid, "2.4.0")]
    [BepInDependency("com.rockerkitten.boneappetit", "3.0.2")]
    [NetworkCompatibility(CompatibilityLevel.EveryoneMustHaveMod, VersionStrictness.Minor)]
    public class Main : BaseUnityPlugin {
        public const string ModGuid = "com.frenvius.Valharvest";
        public const string ModName = "Valharvest";
        public const string Version = "2.0.0";

        public static AssetBundle modAssets;
        public static AssetBundle foodAssets;
        public static AssetBundle plantAssets;
        public static AssetBundle pieceAssets;

        public static EffectList loxMilkSfx;
        public static EffectList boneMealVfx;
        public static Texture2D newEggTexture;
        public static Texture2D pizzaPlateTexture;
        public static Sprite newEggSprite;
        public static Sprite pizzaSprite;
        public static ConfigEntry<bool> kabobName;
        public static ConfigEntry<bool> dropEnabled;

        public GameObject greydwarfFab;
        public GameObject trollFab;
        public GameObject goblinFab;
        public GameObject draugrFab;

        public GameObject greydwarfBruteFab;

        public GameObject draugrEliteFab;
        
        public static GameObject milkBottleFab;

        private Harmony _h;
        public ConfigEntry<int> garlicChange;

        public ConfigEntry<int> pepperChance;
        public ConfigEntry<int> riceChange;

        public void Awake() {
            LoadEmbeddedAssembly("CustomScripts.resources");
            CreatConfigValues();
            AssetLoad();
            LoadItems();
            LoadPieces();

            PrefabManager.OnVanillaPrefabsAvailable += LoadNewFood;
            PrefabManager.OnVanillaPrefabsAvailable += LoadSounds;
            PrefabManager.OnVanillaPrefabsAvailable += AddCustomPlantsPrefab;
            ItemManager.OnItemsRegisteredFejd += LoadBalancedFood;
            ItemManager.OnItemsRegisteredFejd += LoadBalance;
            ZoneManager.OnVanillaLocationsAvailable += AddCustomPlants;

            if (dropEnabled.Value) PrefabManager.OnVanillaPrefabsAvailable += NewDrops;

            _h = new Harmony("mod.valharvest");
            _h.PatchAll();
        }

        private void OnDestroy() {
            Jotunn.Logger.LogInfo("Valharvest: OnDestroy");
            _h.UnpatchSelf();
        }

        // @formatter:wrap_lines false
        public void CreatConfigValues() {
            Config.SaveOnConfigSet = true;

            // pepperChance = Config.Bind("Pepper", "Enable", 40, new ConfigDescription("Chance of drop Spicy Pepper", null, new ConfigurationManagerAttributes {IsAdminOnly = true}));
            // garlicChange = Config.Bind("Garlic", "Enable", 40, new ConfigDescription("Chance of drop Garlic", null, new ConfigurationManagerAttributes {IsAdminOnly = true}));
            // riceChange = Config.Bind("Rice", "Enable", 40, new ConfigDescription("Chance of drop Rice", null, new ConfigurationManagerAttributes {IsAdminOnly = true}));

            kabobName = Config.Bind("Kabob", "Enable", true, new ConfigDescription("Change Kabob name to Kebab", null, new ConfigurationManagerAttributes {IsAdminOnly = true}));
            dropEnabled = Config.Bind("Vegetables Drop", "Enable", false, new ConfigDescription("Keep vegetables in monster drops", null, new ConfigurationManagerAttributes {IsAdminOnly = true}));
        }
        // @formatter:wrap_lines restore
        
        public void LoadBalance() {
            try {
                SeagullEgg();
                Kabob();
                Pizza();
            } catch (Exception ex) {
                Jotunn.Logger.LogError($"Error while running OnItemsRegistered: {ex.Message}");
            } finally {
                Jotunn.Logger.LogInfo("Load Complete.");
                ItemManager.OnItemsRegistered -= LoadBalance;
            }
        }

        public void AssetLoad() {
            modAssets = AssetUtils.LoadAssetBundleFromResources("valharvest", Assembly.GetExecutingAssembly());
            foodAssets = AssetUtils.LoadAssetBundleFromResources("valharvestfoods", Assembly.GetExecutingAssembly());
            plantAssets = AssetUtils.LoadAssetBundleFromResources("valharvestplants", Assembly.GetExecutingAssembly());
            pieceAssets = AssetUtils.LoadAssetBundleFromResources("valharvestpieces", Assembly.GetExecutingAssembly());
            newEggTexture = modAssets.LoadAsset<Texture2D>("egg");
            newEggSprite = modAssets.LoadAsset<Sprite>("eggsprite");
            pizzaPlateTexture = modAssets.LoadAsset<Texture2D>("pizza_plate");
            pizzaSprite = modAssets.LoadAsset<Sprite>("pizzaSprite");

            Jotunn.Logger.LogInfo("Preparing the plants...");
        }

        public void LoadSounds() {
            var poursMilk = modAssets.LoadAsset<GameObject>("sfx_pours_milk");
            PrefabManager.Instance.AddPrefab(new CustomPrefab(poursMilk, true));
            var loxPetMilk = modAssets.LoadAsset<GameObject>("sfx_lox_pet_milk");
            PrefabManager.Instance.AddPrefab(new CustomPrefab(loxPetMilk, true));
            var loxPet = modAssets.LoadAsset<GameObject>("sfx_lox_pet");
            PrefabManager.Instance.AddPrefab(new CustomPrefab(loxPet, true));
            var boneMealSpark = modAssets.LoadAsset<GameObject>("vfx_bone_meal");
            PrefabManager.Instance.AddPrefab(new CustomPrefab(boneMealSpark, true));
            loxMilkSfx = new EffectList {
                m_effectPrefabs = new[] {
                    new EffectList.EffectData {m_prefab = loxPetMilk, m_enabled = true},
                    new() {m_prefab = poursMilk, m_enabled = true}
                }
            };
            boneMealVfx = new EffectList {
                m_effectPrefabs = new[] {new EffectList.EffectData {m_prefab = boneMealSpark, m_enabled = true}}
            };

            PrefabManager.OnVanillaPrefabsAvailable -= LoadSounds;
        }

        public void NewDrops() {
            greydwarfFab = PrefabManager.Instance.GetPrefab("Greydwarf");
            trollFab = PrefabManager.Instance.GetPrefab("Troll");
            goblinFab = PrefabManager.Instance.GetPrefab("Goblin");
            draugrFab = PrefabManager.Instance.GetPrefab("Draugr");
            greydwarfBruteFab = PrefabManager.Instance.GetPrefab("Greydwarf_Elite");
            draugrEliteFab = PrefabManager.Instance.GetPrefab("Draugr_Elite");
            var pepperFabDrop = PrefabManager.Instance.GetPrefab("pepper");
            var tomatoFabDrop = PrefabManager.Instance.GetPrefab("tomato");
            var garlicFabDrop = PrefabManager.Instance.GetPrefab("garlic");
            var riceFabDrop = PrefabManager.Instance.GetPrefab("rice");
            var potatoFabDrop = PrefabManager.Instance.GetPrefab("potato");
            var pumpkinFabDrop = PrefabManager.Instance.GetPrefab("pumpkin");
            milkBottleFab = PrefabManager.Instance.GetPrefab("milk_bottle");

            greydwarfFab.GetComponent<CharacterDrop>().m_drops.Add(new CharacterDrop.Drop {
                m_prefab = pepperFabDrop,
                m_amountMin = 1,
                m_amountMax = 2,
                m_chance = 0.4f,
                m_levelMultiplier = true,
                m_onePerPlayer = false
            });

            trollFab.GetComponent<CharacterDrop>().m_drops.Add(new CharacterDrop.Drop {
                m_prefab = garlicFabDrop,
                m_amountMin = 1,
                m_amountMax = 3,
                m_chance = 0.4f,
                m_levelMultiplier = true,
                m_onePerPlayer = false
            });

            goblinFab.GetComponent<CharacterDrop>().m_drops.Add(new CharacterDrop.Drop {
                m_prefab = riceFabDrop,
                m_amountMin = 1,
                m_amountMax = 1,
                m_chance = 0.4f,
                m_levelMultiplier = true,
                m_onePerPlayer = false
            });

            draugrFab.GetComponent<CharacterDrop>().m_drops.Add(new CharacterDrop.Drop {
                m_prefab = tomatoFabDrop,
                m_amountMin = 1,
                m_amountMax = 1,
                m_chance = 0.4f,
                m_levelMultiplier = true,
                m_onePerPlayer = false
            });

            greydwarfBruteFab.GetComponent<CharacterDrop>().m_drops.Add(new CharacterDrop.Drop {
                m_prefab = potatoFabDrop,
                m_amountMin = 1,
                m_amountMax = 1,
                m_chance = 0.4f,
                m_levelMultiplier = true,
                m_onePerPlayer = false
            });

            draugrEliteFab.GetComponent<CharacterDrop>().m_drops.Add(new CharacterDrop.Drop {
                m_prefab = pumpkinFabDrop,
                m_amountMin = 1,
                m_amountMax = 1,
                m_chance = 0.4f,
                m_levelMultiplier = true,
                m_onePerPlayer = false
            });

            PrefabManager.OnVanillaPrefabsAvailable -= NewDrops;
        }
    }
}
using System;
using System.Linq;
using System.Reflection;
using BepInEx;
using BepInEx.Bootstrap;
using BepInEx.Configuration;
using HarmonyLib;
using Jotunn.Configs;
using Jotunn.Entities;
using Jotunn.Managers;
using Jotunn.Utils;
using UnityEngine;
using Valharvest.Scripts;
using static Valharvest.Utils;
using static Valharvest.Scripts.Loaders;
using static Valharvest.WorldGen.Plants;
using static Valharvest.WorldGen.PlantUtils;
using static Valharvest.Scripts.BoneAppetitBalance;

namespace Valharvest;

[BepInPlugin(ModGuid, ModName, Version)]
[BepInDependency(Jotunn.Main.ModGuid, "2.7.0")]
[BepInDependency("com.rockerkitten.boneappetit", BepInDependency.DependencyFlags.SoftDependency)]
[NetworkCompatibility(CompatibilityLevel.EveryoneMustHaveMod, VersionStrictness.Minor)]
public class Main : BaseUnityPlugin {
    public const string ModGuid = "com.frenvius.Valharvest";
    public const string ModName = "Valharvest";
    public const string Version = "3.3.2";

    public static AssetBundle modAssets;

    public static EffectList loxMilkSfx;
    public static EffectList boneMealVfx;
    public static Texture2D newEggTexture;
    public static Texture2D pizzaPlateTexture;
    public static Sprite newEggSprite;
    public static Sprite pizzaSprite;

    public static bool IsFarmingModInstalled;
    public static KeyboardShortcut MassPlantShortcut;

    public GameObject greydwarfFab;
    public GameObject trollFab;
    public GameObject goblinFab;
    public GameObject draugrFab;
    public GameObject greydwarfBruteFab;
    public GameObject draugrEliteFab;
    public GameObject seagullFab;
    private Harmony _h;

    public void Awake() {
        CreateConfigValues();
        AssetLoad();
        LoadItems();
        LoadPieces();

        PrefabManager.OnVanillaPrefabsAvailable += LoadNewFood;
        PrefabManager.OnVanillaPrefabsAvailable += LoadSounds;
        PrefabManager.OnVanillaPrefabsAvailable += AddCustomPlantsPrefab;

        if (BoneAppetitCompat.IsInstalled) {
            ItemManager.OnItemsRegisteredFejd += LoadBalancedFood;
            ItemManager.OnItemsRegisteredFejd += LoadBalance;
        }

        ItemManager.OnItemsRegisteredFejd += LoadCookingStations;
        PrefabManager.OnVanillaPrefabsAvailable += AddCustomPlants;
        PrefabManager.OnVanillaPrefabsAvailable += CustomDrops;
        PrefabManager.OnVanillaPrefabsAvailable += CheckIfFarmingModInstalled;
        PrefabManager.OnVanillaPrefabsAvailable += HandlePrefabComponent.ZNetViewAwakePatch;
        PrefabManager.OnPrefabsRegistered += HandlePrefabComponent.PrepTableSmokePatch;
        PrefabManager.OnPrefabsRegistered += HandlePrefabComponent.FoodCrateEffectsPatch;
        PrefabManager.OnPrefabsRegistered += CustomFeed;
        PrefabManager.OnPrefabsRegistered += PrepTableRecipeCopier.CopyVanillaPrepTableRecipes;

        CommandManager.Instance.AddConsoleCommand(new ExportItemsCommand());

        if (Configurations.Valharvest.DropEnabled.Value) PrefabManager.OnVanillaPrefabsAvailable += NewDrops;

        _h = new Harmony("mod.valharvest");
        _h.PatchAll();
    }

    private void OnDestroy() {
        Jotunn.Logger.LogInfo("Valharvest: OnDestroy");
        _h.UnpatchSelf();
    }

    private static void CheckIfFarmingModInstalled() {
        Chainloader.Start();

        var plugins = Chainloader.PluginInfos;
        foreach (var plugin in plugins.Where(plugin => plugin.Value.Metadata.GUID == "org.bepinex.plugins.farming")) {
            IsFarmingModInstalled = true;
            break;
        }

        if (IsFarmingModInstalled) {
            var mod = Chainloader.PluginInfos["org.bepinex.plugins.farming"];
            var config = mod.Instance.Config;
            foreach (var key in config.Keys) {
                if (!key.ToString().Contains("Toggle Mass Plant")) continue;
                var value = config[key];
                var keyCode = (KeyCode) Enum.Parse(typeof(KeyCode), value.BoxedValue.ToString());
                MassPlantShortcut = new KeyboardShortcut(keyCode);
            }
        }
        PrefabManager.OnVanillaPrefabsAvailable -= CheckIfFarmingModInstalled;
    }

    public void CreateConfigValues() {
        Config.SaveOnConfigSet = true;

        var valharvest = new Configurations.Valharvest(Config);
    }

    public void LoadBalance() {
        if (!BoneAppetitCompat.IsInstalled) {
            ItemManager.OnItemsRegistered -= LoadBalance;
            return;
        }

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
                new EffectList.EffectData {
                    m_prefab = loxPetMilk,
                    m_enabled = true
                },
                new() {
                    m_prefab = poursMilk,
                    m_enabled = true
                }
            }
        };
        boneMealVfx = new EffectList {
            m_effectPrefabs = new[] {
                new EffectList.EffectData {
                    m_prefab = boneMealSpark,
                    m_enabled = true
                }
            }
        };

        PrefabManager.OnVanillaPrefabsAvailable -= LoadSounds;

        var conversionConfig = new CookingConversionConfig {
            FromItem = "apple",
            ToItem = "baked_apple",
            CookTime = 5f,
            Station = "piece_cookingstation"
        };
        ItemManager.Instance.AddItemConversion(new CustomItemConversion(conversionConfig));
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

    public void CustomDrops() {
        if (!BoneAppetitCompat.IsInstalled) {
            try {
                var seagullFab = PrefabManager.Instance.GetPrefab("Seagal");
                if (seagullFab != null) {
                    var eggPrefab = PrefabManager.Instance.GetPrefab(BoneAppetitCompat.GetEggItem());
                    if (eggPrefab != null) {
                        var dropComponent = seagullFab.GetComponent<DropOnDestroyed>();
                        if (dropComponent != null) {
                            dropComponent.m_dropWhenDestroyed.m_drops.Add(new DropTable.DropData {
                                m_item = eggPrefab,
                                m_stackMin = 1,
                                m_stackMax = 2,
                                m_weight = 0.75f
                            });
                            Jotunn.Logger.LogInfo($"Added vh_egg drop to seagulls (BoneAppetit not installed)");
                        }
                    }
                }
            } catch (System.Exception ex) {
                Jotunn.Logger.LogWarning($"Failed to add egg drops to seagulls: {ex.Message}");
            }
        }

        // Keep raw_seagull commented out
        // seagullFab = PrefabManager.Instance.GetPrefab("Seagal");
        // var seagullFabDrop = PrefabManager.Instance.GetPrefab("raw_seagull");
        //
        // seagullFab.GetComponent<DropOnDestroyed>().m_dropWhenDestroyed.m_drops.Add(new DropTable.DropData
        // {
        //     m_item = seagullFabDrop,
        //     m_stackMin = 1,
        //     m_stackMax = 1,
        //     m_weight = 1f
        // });

        PrefabManager.OnVanillaPrefabsAvailable -= CustomDrops;
    }

    public void AddConsumableItemsToCreature(string[] items, string creature) {
        var boarAI = PrefabManager.Instance.GetPrefab(creature).GetComponent<MonsterAI>();

        foreach (string item in items) {
            var itemFab = PrefabManager.Instance.GetPrefab(item);
            boarAI.m_consumeItems.Add(itemFab.GetComponent<ItemDrop>());
        }
    }

    public void CustomFeed() {
        string[] consumableItemsBoar = new string[] {
            "apple",
            "tomato",
            "potato"
        };
        AddConsumableItemsToCreature(consumableItemsBoar, "Boar");

        string[] consumableItemsWolf = new[] {
	        BoneAppetitCompat.GetPorkItem(),
        };
        AddConsumableItemsToCreature(consumableItemsWolf, "Wolf");

        PrefabManager.OnVanillaPrefabsAvailable -= CustomFeed;
    }
}

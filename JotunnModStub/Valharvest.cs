using System;
using System.Reflection;
using BepInEx;
using BepInEx.Configuration;
using HarmonyLib;
using Jotunn.Configs;
using Jotunn.Entities;
using Jotunn.Managers;
using Jotunn.Utils;
using UnityEngine;
using static Valharvest.Utils;
using static Valharvest.Scripts.BoneAppetitBalance;

namespace Valharvest {
    [BepInPlugin(ModGuid, ModName, Version)]
    [BepInDependency(Jotunn.Main.ModGuid, "2.3.7")]
    [BepInDependency("com.rockerkitten.boneappetit", "3.0.2")]
    [NetworkCompatibility(CompatibilityLevel.EveryoneMustHaveMod, VersionStrictness.Minor)]
    public class Main : BaseUnityPlugin {
        public const string ModGuid = "com.frenvius.Valharvest";
        public const string ModName = "Valharvest";
        public const string Version = "1.7.1";

        public static AssetBundle modAssets;
        public static AssetBundle foodAssets;
        public static AssetBundle plantAssets;
        public static AssetBundle pieceAssets;

        public static EffectList loxMilkSfx;
        public static EffectList loxPetSfx;
        public static Texture2D newEggTexture;
        public static Texture2D pizzaPlateTexture;
        public static Sprite newEggSprite;
        public static Sprite pizzaSprite;
        public static ConfigEntry<bool> kabobName;

        public GameObject pepperSeedsFab;
        public GameObject rawPastaFab;

        public GameObject greydwarfFab;
        public GameObject trollFab;
        public GameObject goblinFab;
        public GameObject draugrFab;

        public GameObject pepperFab;
        public GameObject tomatoFab;
        public GameObject saltFab;
        public GameObject garlicFab;
        public GameObject riceFab;
        public GameObject garlicPlantPiece;
        public GameObject garlicPlantFab;
        public GameObject ricePlantPiece;
        public GameObject ricePlantFab;
        public GameObject pepperPlantPiece;
        public GameObject wellFab;
        public GameObject cultivator;
        public GameObject smallCultivator;
        public GameObject tomatoBox;
        public GameObject pepperPlantFab;
        public GameObject tomatoPlantPiece;
        public GameObject tomatoPlantFab;
        public GameObject potatoPlantFab;
        public GameObject potatoPlantPiece;

        public GameObject milkBottle;

        public GameObject emptyBottleFab;

        public GameObject potatoFab;

        public GameObject pumpkinPlantFab;

        public GameObject pumpkinPlantPiece;

        public GameObject pumpkinFab;
        private CustomItem _emptyBottle;
        private Harmony _h;
        private CustomItem _pepperSeeds;
        private CustomItem _rawPasta;
        public ConfigEntry<int> garlicChange;

        public ConfigEntry<int> pepperChance;
        public ConfigEntry<int> riceChange;

        public void Awake() {
            CreatConfigValues();
            AssetLoad();
            PrefabManager.OnVanillaPrefabsAvailable += LoadFood;
            PrefabManager.OnVanillaPrefabsAvailable += LoadNewFood;
            PrefabManager.OnVanillaPrefabsAvailable += NewDrops;
            PrefabManager.OnVanillaPrefabsAvailable += LoadSounds;
            ItemManager.OnItemsRegisteredFejd += LoadBalancedFood;
            ItemManager.OnItemsRegisteredFejd += LoadBalance;

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
        }
        // @formatter:wrap_lines restore

        public void LoadFood() {
            try {
                PepperSeeds();
                RawPasta();
                EmptyBottle();
            } catch (Exception ex) {
                Jotunn.Logger.LogError($"Error while running OnVanillaLoad: {ex.Message}");
            } finally {
                Jotunn.Logger.LogInfo("Load Complete.");
                PrefabManager.OnVanillaPrefabsAvailable -= LoadFood;
            }
        }

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
            LoadPlantsFab();
        }

        public void LoadSounds() {
            var poursMilk = modAssets.LoadAsset<GameObject>("sfx_pours_milk");
            PrefabManager.Instance.AddPrefab(new CustomPrefab(poursMilk, true));
            var loxPetMilk = modAssets.LoadAsset<GameObject>("sfx_lox_pet_milk");
            PrefabManager.Instance.AddPrefab(new CustomPrefab(loxPetMilk, true));
            var loxPet = modAssets.LoadAsset<GameObject>("sfx_lox_pet");
            PrefabManager.Instance.AddPrefab(new CustomPrefab(loxPet, true));
            loxMilkSfx = new EffectList {
                m_effectPrefabs = new[] {
                    new EffectList.EffectData {m_prefab = loxPetMilk, m_enabled = true},
                    new() {m_prefab = poursMilk, m_enabled = true}
                }
            };
            loxPetSfx = new EffectList {
                m_effectPrefabs = new[] {new EffectList.EffectData {m_prefab = loxPet, m_enabled = true}}
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

        public GameObject greydwarfBruteFab;

        public GameObject draugrEliteFab;

        public void LoadPlantsFab() {
            cultivator = pieceAssets.LoadAsset<GameObject>("piece_cultivatedGround");
            smallCultivator = pieceAssets.LoadAsset<GameObject>("piece_cultivatedGround_small");
            tomatoBox = pieceAssets.LoadAsset<GameObject>("piece_tomatoBox");
            // PrefabManager.Instance.AddPrefab(new CustomPrefab(cultivator, true));

            wellFab = pieceAssets.LoadAsset<GameObject>("water_well");
            // PrefabManager.Instance.AddPrefab(new CustomPrefab(wellFab, true));
            
            jackopumpkinFab = pieceAssets.LoadAsset<GameObject>("piece_jackopumpkin");
            // PrefabManager.Instance.AddPrefab(new CustomPrefab(jackopumpkinFab, true));

            saltFab = modAssets.LoadAsset<GameObject>("salt");
            ItemManager.Instance.AddItem(new CustomItem(saltFab, true));

            pepperFab = modAssets.LoadAsset<GameObject>("pepper");
            ItemManager.Instance.AddItem(new CustomItem(pepperFab, true));

            tomatoFab = modAssets.LoadAsset<GameObject>("tomato");
            ItemManager.Instance.AddItem(new CustomItem(tomatoFab, true));

            potatoFab = modAssets.LoadAsset<GameObject>("potato");
            ItemManager.Instance.AddItem(new CustomItem(potatoFab, true));

            pumpkinFab = modAssets.LoadAsset<GameObject>("pumpkin");
            ItemManager.Instance.AddItem(new CustomItem(pumpkinFab, true));

            garlicFab = modAssets.LoadAsset<GameObject>("garlic");
            ItemManager.Instance.AddItem(new CustomItem(garlicFab, true));

            riceFab = modAssets.LoadAsset<GameObject>("rice");
            ItemManager.Instance.AddItem(new CustomItem(riceFab, true));

            milkBottle = modAssets.LoadAsset<GameObject>("milk_bottle");
            ItemManager.Instance.AddItem(new CustomItem(milkBottle, true));
            
            pepperPlantPiece = plantAssets.LoadAsset<GameObject>("Pickable_Pepper");
            PrefabManager.Instance.AddPrefab(new CustomPrefab(pepperPlantPiece, true));
            ChangePlantShader(pepperPlantPiece, "pepperP");

            garlicPlantPiece = plantAssets.LoadAsset<GameObject>("Pickable_garlic");
            PrefabManager.Instance.AddPrefab(new CustomPrefab(garlicPlantPiece, true));
            ChangePlantShader(garlicPlantPiece, "garlicP");

            pepperPlantFab = plantAssets.LoadAsset<GameObject>("sapling_pepper");
            ChangePlantShader(pepperPlantFab, "pepperF");

            garlicPlantFab = plantAssets.LoadAsset<GameObject>("sapling_garlic");
            ChangePlantShader(garlicPlantFab, "garlicF");

            ricePlantPiece = plantAssets.LoadAsset<GameObject>("Pickable_Rice");
            PrefabManager.Instance.AddPrefab(new CustomPrefab(ricePlantPiece, true));
            ChangePlantShader(ricePlantPiece, "riceP");

            ricePlantFab = plantAssets.LoadAsset<GameObject>("sapling_rice");
            ChangePlantShader(ricePlantFab, "riceF");

            tomatoPlantPiece = plantAssets.LoadAsset<GameObject>("Pickable_Tomato");
            PrefabManager.Instance.AddPrefab(new CustomPrefab(tomatoPlantPiece, true));
            ChangePlantShader(tomatoPlantPiece, "tomatoP");

            tomatoPlantFab = plantAssets.LoadAsset<GameObject>("sapling_tomato");
            ChangePlantShader(tomatoPlantFab, "tomatoF");

            potatoPlantPiece = plantAssets.LoadAsset<GameObject>("Pickable_Potato");
            PrefabManager.Instance.AddPrefab(new CustomPrefab(potatoPlantPiece, true));
            ChangePlantShader(potatoPlantPiece, "potatoP");

            potatoPlantFab = plantAssets.LoadAsset<GameObject>("sapling_potato");
            ChangePlantShader(potatoPlantFab, "potatoF");

            pumpkinPlantPiece = plantAssets.LoadAsset<GameObject>("Pickable_Pumpkin");
            PrefabManager.Instance.AddPrefab(new CustomPrefab(pumpkinPlantPiece, true));
            ChangePlantShader(pumpkinPlantPiece, "pumpkinP");

            pumpkinPlantFab = plantAssets.LoadAsset<GameObject>("sapling_pumpkin");
            ChangePlantShader(pumpkinPlantFab, "pumpkinF");

            LoadItem();
        }

        public GameObject jackopumpkinFab;

        private void LoadItem() {
            var pepperPlant = new CustomPiece(pepperPlantFab, true,
                new PieceConfig {
                    AllowedInDungeons = false,
                    Enabled = true,
                    PieceTable = "_CultivatorPieceTable",
                    Requirements = new[] {new RequirementConfig {Item = "PepperSeeds", Amount = 1, Recover = true}}
                });
            PieceManager.Instance.AddPiece(pepperPlant);

            var tomatoPlant = new CustomPiece(tomatoPlantFab, true,
                new PieceConfig {
                    AllowedInDungeons = false,
                    Enabled = true,
                    PieceTable = "_CultivatorPieceTable",
                    Requirements = new[] {new RequirementConfig {Item = "tomato", Amount = 1, Recover = true}}
                });
            PieceManager.Instance.AddPiece(tomatoPlant);

            var garlicPlant = new CustomPiece(garlicPlantFab, true,
                new PieceConfig {
                    AllowedInDungeons = false,
                    Enabled = true,
                    PieceTable = "_CultivatorPieceTable",
                    Requirements = new[] {new RequirementConfig {Item = "garlic", Amount = 1, Recover = true}}
                });
            PieceManager.Instance.AddPiece(garlicPlant);

            var ricePlant = new CustomPiece(ricePlantFab, true,
                new PieceConfig {
                    AllowedInDungeons = false,
                    Enabled = true,
                    PieceTable = "_CultivatorPieceTable",
                    Requirements = new[] {new RequirementConfig {Item = "rice", Amount = 1, Recover = true}}
                });
            PieceManager.Instance.AddPiece(ricePlant);

            var potatoPlant = new CustomPiece(potatoPlantFab, true,
                new PieceConfig {
                    AllowedInDungeons = false,
                    Enabled = true,
                    PieceTable = "_CultivatorPieceTable",
                    Requirements = new[] {new RequirementConfig {Item = "potato", Amount = 1, Recover = true}}
                });
            PieceManager.Instance.AddPiece(potatoPlant);

            var pumpkinPlant = new CustomPiece(pumpkinPlantFab, true,
                new PieceConfig {
                    AllowedInDungeons = false,
                    Enabled = true,
                    PieceTable = "_CultivatorPieceTable",
                    Requirements = new[] {new RequirementConfig {Item = "pumpkin", Amount = 1, Recover = true}}
                });
            PieceManager.Instance.AddPiece(pumpkinPlant);

            // var well = new CustomPiece(wellFab, true,
            //     new PieceConfig {
            //         AllowedInDungeons = false,
            //         Enabled = true,
            //         PieceTable = "_HammerPieceTable",
            //         CraftingStation = "piece_workbench",
            //         Requirements = new[] {new RequirementConfig {Item = "Dandelion", Amount = 1, Recover = true}}
            //     });
            // PieceManager.Instance.AddPiece(well);
            
            var jackopumpkin = new CustomPiece(jackopumpkinFab, true,
                new PieceConfig {
                    AllowedInDungeons = false,
                    Enabled = true,
                    PieceTable = "_HammerPieceTable",
                    CraftingStation = "piece_workbench",
                    Requirements = new[] {
                        new RequirementConfig {Item = "pumpkin", Amount = 1, Recover = true},
                        new RequirementConfig {Item = "Resin", Amount = 2, Recover = true}
                    }
                });
            PieceManager.Instance.AddPiece(jackopumpkin);
            
            var cultivatorPiece = new CustomPiece(cultivator, true,
                new PieceConfig {
                    AllowedInDungeons = false,
                    Enabled = true,
                    PieceTable = "_HammerPieceTable",
                    CraftingStation = "piece_workbench",
                    Requirements = new[] {
                        new RequirementConfig {Item = "Wood", Amount = 10, Recover = true},
                        new RequirementConfig {Item = "Tar", Amount = 20, Recover = true}
                    }
                });
            PieceManager.Instance.AddPiece(cultivatorPiece);
            
            var smallCultivatorPiece = new CustomPiece(smallCultivator, true,
                new PieceConfig {
                    AllowedInDungeons = false,
                    Enabled = true,
                    PieceTable = "_HammerPieceTable",
                    CraftingStation = "piece_workbench",
                    Requirements = new[] {
                        new RequirementConfig {Item = "Wood", Amount = 5, Recover = true},
                        new RequirementConfig {Item = "Tar", Amount = 10, Recover = true}
                    }
                });
            PieceManager.Instance.AddPiece(smallCultivatorPiece);
            
            var tomatoBoxPiece = new CustomPiece(tomatoBox, true,
                new PieceConfig {
                    AllowedInDungeons = false,
                    Enabled = true,
                    PieceTable = "_HammerPieceTable",
                    CraftingStation = "piece_workbench",
                    Requirements = new[] {
                        new RequirementConfig {Item = "tomato", Amount = 50, Recover = true}
                    }
                });
            PieceManager.Instance.AddPiece(tomatoBoxPiece);
        }

        private void PepperSeeds() {
            try {
                pepperSeedsFab = modAssets.LoadAsset<GameObject>("PepperSeeds");
                _pepperSeeds = new CustomItem(pepperSeedsFab, true,
                    new ItemConfig {
                        Name = "Spicy Pepper Seeds",
                        Enabled = true,
                        Amount = 3,
                        CraftingStation = "",
                        Requirements = new[] {new RequirementConfig {Item = "pepper", Amount = 1}}
                    });
            } catch (Exception ex) {
                Jotunn.Logger.LogError($"Error while loading PepperSeeds: {ex.Message}");
            } finally {
                Jotunn.Logger.LogInfo("PepperSeeds Loaded.");
                ItemManager.Instance.AddItem(_pepperSeeds);
            }
        }

        private void RawPasta() {
            try {
                rawPastaFab = modAssets.LoadAsset<GameObject>("raw_pasta");
                _rawPasta = new CustomItem(rawPastaFab, true,
                    new ItemConfig {
                        Name = "Raw Pasta",
                        Enabled = true,
                        Amount = 2,
                        CraftingStation = "rk_prep",
                        Requirements = new[] {
                            new RequirementConfig {Item = "BarleyFlour", Amount = 1},
                            new RequirementConfig {Item = "salt", Amount = 1},
                            new RequirementConfig {Item = "rk_egg", Amount = 2}
                        }
                    });
            } catch (Exception ex) {
                Jotunn.Logger.LogError($"Error while loading RawPasta: {ex.Message}");
            } finally {
                Jotunn.Logger.LogInfo("RawPasta Loaded.");
                ItemManager.Instance.AddItem(_rawPasta);
            }
        }

        private void EmptyBottle() {
            try {
                emptyBottleFab = pieceAssets.LoadAsset<GameObject>("empty_bottle");
                _emptyBottle = new CustomItem(emptyBottleFab, true,
                    new ItemConfig {
                        Name = "Empty Bottle",
                        Enabled = true,
                        Amount = 2,
                        CraftingStation = "piece_workbench",
                        Requirements = new[] {new RequirementConfig {Item = "Crystal", Amount = 1}}
                    });
            } catch (Exception ex) {
                Jotunn.Logger.LogError($"Error while loading Empty Bottle: {ex.Message}");
            } finally {
                Jotunn.Logger.LogInfo("Empty Bottle Loaded.");
                ItemManager.Instance.AddItem(_emptyBottle);
            }
        }
    }
}
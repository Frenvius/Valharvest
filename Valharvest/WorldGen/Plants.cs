using System;
using System.Collections.Generic;
using Jotunn.Configs;
using Jotunn.Entities;
using Jotunn.Managers;
using SimpleJson;
using static Valharvest.Utils;
using static SimpleJson.SimpleJson;

namespace Valharvest.WorldGen {
    public static class Plants {
        private const string Min = "min";
        private const string Max = "max";
        private const string Biome = "biome";
        private const string InForest = "inForest";
        private const string BiomeArea = "biomeArea";
        private const string GroupRadius = "groupRadius";
        private const string GroupSizeMin = "groupSizeMin";
        private const string GroupSizeMax = "groupSizeMax";
        private const string MinAltitude = "minAltitude";
        private const string MaxAltitude = "maxAltitude";
        private const string ForestThresholdMin = "forestThresholdMin";
        private const string ForestThresholdMax = "forestThresholdMax";
        private const Heightmap.Biome Swamp = Heightmap.Biome.Swamp;
        private const Heightmap.Biome Plains = Heightmap.Biome.Plains;
        private const Heightmap.Biome Meadows = Heightmap.Biome.Meadows;
        private const Heightmap.Biome Mountain = Heightmap.Biome.Mountain;
        private const Heightmap.Biome Mistlands = Heightmap.Biome.Mistlands;
        private const Heightmap.Biome BlackForest = Heightmap.Biome.BlackForest;
        private const Heightmap.BiomeArea Edge = Heightmap.BiomeArea.Edge;
        private const Heightmap.BiomeArea Median = Heightmap.BiomeArea.Median;
        private const Heightmap.BiomeArea Everything = Heightmap.BiomeArea.Everything;

        public static void AddCustomPlants() {
            var jsonContent = ReadEmbeddedFile("plantsLocations.resources");
            if (jsonContent == null) return;
            var plantJson = DeserializeObject<JsonObject>(jsonContent);
            foreach (var plant in plantJson) {
                var plantPrefab = PrefabManager.Instance.CreateClonedPrefab(plant.Key + "_wild", plant.Key);
                var plantObject = DeserializeObject<JsonObject>(plant.Value.ToString());
                try {
                    ZoneManager.Instance.AddCustomVegetation(new CustomVegetation(plantPrefab, false, GetVegetationConfig(plantObject)));
                } catch (Exception ex) {
                    Jotunn.Logger.LogError($"Error while loading {plant.Key}: {ex.Message}");
                } finally {
                    ZoneManager.OnVanillaLocationsAvailable -= AddCustomPlants;
                }
            }
        }

        private static VegetationConfig GetVegetationConfig(JsonObject content) {
            var vegConfig = new VegetationConfig();
            var getBiome = GetBiome();
            var getBiomeArea = GetBiomeArea();

            if (content[Min] != null) vegConfig.Min = float.Parse(content[Min].ToString());
            if (content[Max] != null) vegConfig.Max = float.Parse(content[Max].ToString());
            if (content[GroupSizeMin] != null) vegConfig.GroupSizeMin = int.Parse(content[GroupSizeMin].ToString());
            if (content[GroupSizeMax] != null) vegConfig.GroupSizeMax = int.Parse(content[GroupSizeMax].ToString());
            if (content[GroupRadius] != null) vegConfig.GroupRadius = float.Parse(content[GroupRadius].ToString());
            if (content[MinAltitude] != null) vegConfig.MinAltitude = float.Parse(content[MinAltitude].ToString());
            if (content[MaxAltitude] != null) vegConfig.MaxAltitude = float.Parse(content[MaxAltitude].ToString());
            if (content[Biome] != null) vegConfig.Biome = getBiome[content[Biome].ToString()];
            if (content[BiomeArea] != null) vegConfig.BiomeArea = getBiomeArea[content[BiomeArea].ToString()];
            if (content[InForest] != null) vegConfig.InForest = Convert.ToBoolean(content[InForest].ToString());
            if (content[ForestThresholdMin] != null) vegConfig.ForestThresholdMin = float.Parse(content[ForestThresholdMin].ToString());
            if (content[ForestThresholdMax] != null) vegConfig.ForestThresholdMax = float.Parse(content[ForestThresholdMax].ToString());

            return vegConfig;
        }

        private static Dictionary<string, Heightmap.Biome> GetBiome() {
            return new Dictionary<string, Heightmap.Biome> {
                {"swamp", Swamp},
                {"plains", Plains},
                {"meadows", Meadows},
                {"mountain", Mountain},
                {"mistlands", Mistlands},
                {"black_forest", BlackForest}
            };
        }
        
        private static Dictionary<string, Heightmap.BiomeArea> GetBiomeArea() {
            return new Dictionary<string, Heightmap.BiomeArea> {
                {"edge", Edge},
                {"median", Median},
                {"everything", Everything}
            };
        }
    }
}
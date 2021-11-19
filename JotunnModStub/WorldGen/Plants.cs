using Jotunn.Configs;
using Jotunn.Entities;
using Jotunn.Managers;

namespace Valharvest.WorldGen {
    public static class Plants {
        public static void AddCustomPlants() {
            try {
                AddPlants("Pickable_Pepper", 0, 0.5f, 1, 2, 5, Heightmap.Biome.BlackForest);
            } finally {
                ZoneManager.OnVanillaLocationsAvailable -= AddCustomPlants;
            }
        }

        private static void AddPlants(string prefabName, float min, float max, int gSizeMin, int gSizeMax,
            float gRadius, Heightmap.Biome biome) {
            var plantPrefab = PrefabManager.Instance.GetPrefab(prefabName);
            ZoneManager.Instance.AddCustomVegetation(new CustomVegetation(plantPrefab,
                new VegetationConfig {
                    Min = min,
                    Max = max,
                    GroupSizeMin = gSizeMin,
                    GroupSizeMax = gSizeMax,
                    GroupRadius = gRadius,
                    Biome = biome
                }));
        }
    }
}
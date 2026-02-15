using System.Collections.Generic;
using BepInEx.Configuration;

namespace Valharvest.Configurations;

public class Valharvest {
	public static ConfigEntry<bool> KabobName;
	public static ConfigEntry<bool> DropEnabled;
	public static ConfigEntry<int> AppleDropAmount;
	public static ConfigEntry<int> AppleRespawnTime;
	public static ConfigEntry<int> SaltDropPercentage;
	public static ConfigEntry<bool> PlantProgressEnabled;
	public static ConfigEntry<bool> PickableProgressEnabled;
	public static ConfigEntry<bool> UseBoneAppetitCookingStations;
	
	public static Dictionary<string, ConfigEntry<int>> GetDropConfigs() {
		return new Dictionary<string, ConfigEntry<int>> {
			{ "AppleDropAmount", AppleDropAmount }
		};
	}

	public static Dictionary<string, ConfigEntry<int>> GetRespawnTimeConfigs() {
		return new Dictionary<string, ConfigEntry<int>> {
			{ "AppleRespawnTime", AppleRespawnTime }
		};
	}

	public ConfigEntry<int> GarlicChance;
	public ConfigEntry<int> PepperChance;
	public ConfigEntry<int> RiceChance;

	internal Valharvest(ConfigFile config) {
		KabobName = config.Bind("Kabob", "Enable", true, new ConfigDescription("Change Kabob name to Kebab", null, new ConfigurationManagerAttributes { IsAdminOnly = true }));
		DropEnabled = config.Bind("Vegetables Drop", "Enable", false, new ConfigDescription("Keep vegetables in monster drops", null, new ConfigurationManagerAttributes { IsAdminOnly = true }));
		PlantProgressEnabled = config.Bind("Plant Progress", "EnableOnPlants", true, new ConfigDescription("Enable showing plant growth progress on plants", null, new ConfigurationManagerAttributes { IsAdminOnly = true }));
		PickableProgressEnabled = config.Bind("Plant Progress", "EnableOnPickable", true, new ConfigDescription("Enable showing plant growth progress on pickable", null, new ConfigurationManagerAttributes { IsAdminOnly = true }));
		AppleDropAmount = config.Bind("Drop Amount", "AppleQuantity", 1, new ConfigDescription("Amount of drop Apple from apple tree", null, new ConfigurationManagerAttributes { IsAdminOnly = true }));
		AppleRespawnTime = config.Bind("Respawn Time", "AppleRespawnTime", 5, new ConfigDescription("Time in hours for apple tree to respawn apples", null, new ConfigurationManagerAttributes { IsAdminOnly = true }));
		SaltDropPercentage = config.Bind("Salt", "Enable", 35, new ConfigDescription("Chance of drop Salt", null, new ConfigurationManagerAttributes { IsAdminOnly = true }));
		UseBoneAppetitCookingStations = config.Bind("Bone Appetit", "UseCookingStations", false, new ConfigDescription("Enable or disable the use of Bone Appetit mod's cooking stations", null, new ConfigurationManagerAttributes { IsAdminOnly = true }));
		// pepperChance = Config.Bind("Pepper", "Enable", 40, new ConfigDescription("Chance of drop Spicy Pepper", null, new ConfigurationManagerAttributes {IsAdminOnly = true}));
		// garlicChange = Config.Bind("Garlic", "Enable", 40, new ConfigDescription("Chance of drop Garlic", null, new ConfigurationManagerAttributes {IsAdminOnly = true}));
		// riceChange = Config.Bind("Rice", "Enable", 40, new ConfigDescription("Chance of drop Rice", null, new ConfigurationManagerAttributes {IsAdminOnly = true}));
	}
}

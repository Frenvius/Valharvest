using Jotunn;
using Jotunn.Entities;

namespace Valharvest.Scripts;

public class ExportItemsCommand : ConsoleCommand {
	public override string Name => "exportitems";
	public override string Help => "Exports all game items to itemdrops.json and recipes to recipes.json in the Valheim directory";

	public override void Run(string[] args) {
		Logger.LogInfo("Running exportitems command...");
		ConsumableItemExtractor.GenerateConsumableItemList();
	}
}

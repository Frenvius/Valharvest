using Logger = Jotunn.Logger;

namespace Valharvest.Scripts {
	public class FixPlantHealth : SlowUpdate {
		private const float GrowRadiusOverride = 0.4f;
		private const Heightmap.Biome BiomeOverride = (Heightmap.Biome)895;

		public override void Awake() {
			base.Awake();
		}

		public override void SUpdate(float currentTime, Vector2i referenceZone) {
			var plantComponent = GetComponent<Plant>();
			var plantPosition = transform.position;
			if (!IsPlantValid(plantComponent, currentTime)) return;
			if (!RaisedBed.CheckIfItemBellowIsCultivatedGround(plantPosition)) return;
			ApplyPlantOverrides(plantComponent);
		}

		private bool IsPlantValid(Plant plant, float currentTime) {
			return plant.m_nview.IsValid() && currentTime <= plant.m_updateTime;
		}

		private void ApplyPlantOverrides(Plant plant) {
			plant.m_tolerateHeat = true;
			plant.m_tolerateCold = true;
			plant.m_biome = BiomeOverride;
			plant.m_growRadius = GrowRadiusOverride;
		}
	}
}

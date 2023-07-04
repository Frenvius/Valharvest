using UnityEngine;

namespace Valharvest.Scripts;

public class FixPlantHealth : SlowUpdate {
    public override void SUpdate() {
        var plant = GetComponent<Plant>();
        var position = transform.position;
        if (!plant.m_nview.IsValid() || Time.time - (double)plant.m_updateTime < 10.0) return;
        // if (plant.m_biome == (Heightmap.Biome)27) return;
        if (!RaisedBed.CheckIfItemBellowIsCultivatedGround(position)) return;
        plant.m_biome = (Heightmap.Biome)27;
        plant.m_growRadius = 0.4f;
    }
}
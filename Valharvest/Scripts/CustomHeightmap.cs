using UnityEngine;

namespace Valharvest.Scripts;

public class CultivatedGround : Heightmap {
    private new void Awake() {
    }

    private new void Update() {
    }

    private new void OnEnable() {
        Regenerate();
    }

    private new void Regenerate() {
        Generate();
        m_dirty = true;
    }

    private new void Generate() {
        Initialize();
    }

    public new void Initialize() {
        var num1 = m_width + 1;
        var num2 = num1 * num1;
        if (m_heights.Count == num2)
            return;
        m_heights.Clear();
        for (var index = 0; index < num2; ++index)
            m_heights.Add(0.0f);
        m_paintMask = new Texture2D(m_width, m_width) {
            wrapMode = TextureWrapMode.Clamp
        };
    }
}
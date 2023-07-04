using UnityEngine;

namespace Valharvest.Scripts;

public static class Vector3Ext {
    public static Vector3 xz(this Vector3 v) {
        return new Vector3(v.x, 0f, v.z);
    }
}
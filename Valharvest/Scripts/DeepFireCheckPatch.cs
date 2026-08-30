using HarmonyLib;
using UnityEngine;

namespace Valharvest.Scripts;

[HarmonyPatch]
public static class DeepFireCheckPatch {
	private static int _characterTriggerMask;
	private static readonly Collider[] _hits = new Collider[32];

	private static int CharacterTriggerMask {
		get {
			if (_characterTriggerMask == 0) _characterTriggerMask = LayerMask.GetMask("character_trigger");
			return _characterTriggerMask;
		}
	}

	private static bool IsTarget(Object instance) {
		return instance != null && instance.name != null && instance.name.StartsWith("piece_cooking_pot");
	}

	[HarmonyPatch(typeof(CraftingStation), "CheckFire")]
	private static class CheckFirePatch {
		private static bool Prefix(CraftingStation __instance, ref bool ___m_haveFire) {
			if (!IsTarget(__instance)) return true;

			GameObject fireObj = __instance.m_haveFireObject;
			bool detected;

			Collider volume = fireObj != null ? fireObj.GetComponent<Collider>() : null;
			if (volume != null) {
				detected = HasBurningInVolume(volume);
			} else {
				Vector3 origin = fireObj != null
					? fireObj.transform.position
					: __instance.transform.position;
				float radius = Configurations.Valharvest.CookingPotFireCheckRadius != null
					? Configurations.Valharvest.CookingPotFireCheckRadius.Value
					: 0.5f;
				detected = EffectArea.IsPointInsideArea(origin, EffectArea.Type.Burning, radius) != null;
			}

			___m_haveFire = detected;
			if (fireObj != null) fireObj.SetActive(detected);
			return false;
		}
	}

	private static bool HasBurningInVolume(Collider volume) {
		Transform t = volume.transform;
		Vector3 lossy = t.lossyScale;
		int n = 0;

		if (volume is BoxCollider box) {
			Vector3 center = t.TransformPoint(box.center);
			Vector3 halfExtents = new Vector3(
				Mathf.Abs(box.size.x * lossy.x) * 0.5f,
				Mathf.Abs(box.size.y * lossy.y) * 0.5f,
				Mathf.Abs(box.size.z * lossy.z) * 0.5f);
			n = Physics.OverlapBoxNonAlloc(center, halfExtents, _hits, t.rotation,
				CharacterTriggerMask, QueryTriggerInteraction.Collide);
		} else if (volume is SphereCollider sph) {
			Vector3 center = t.TransformPoint(sph.center);
			float r = sph.radius * Mathf.Max(Mathf.Abs(lossy.x), Mathf.Abs(lossy.y), Mathf.Abs(lossy.z));
			n = Physics.OverlapSphereNonAlloc(center, r, _hits,
				CharacterTriggerMask, QueryTriggerInteraction.Collide);
		} else if (volume is CapsuleCollider cap) {
			Bounds b = cap.bounds;
			n = Physics.OverlapBoxNonAlloc(b.center, b.extents, _hits, Quaternion.identity,
				CharacterTriggerMask, QueryTriggerInteraction.Collide);
		} else {
			Bounds b = volume.bounds;
			n = Physics.OverlapBoxNonAlloc(b.center, b.extents, _hits, Quaternion.identity,
				CharacterTriggerMask, QueryTriggerInteraction.Collide);
		}

		for (int i = 0; i < n; i++) {
			EffectArea area = _hits[i].GetComponent<EffectArea>();
			if (area != null && (area.m_type & EffectArea.Type.Burning) != 0) return true;
		}
		return false;
	}
}

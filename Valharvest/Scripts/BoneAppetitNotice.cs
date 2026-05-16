using System.Collections;
using HarmonyLib;
using UnityEngine;

namespace Valharvest.Scripts;

[HarmonyPatch]
public static class BoneAppetitNotice {
	private const float MaxWaitSeconds = 10f;
	private const float PollInterval = 0.2f;
	private static bool _shownThisSession;

	[HarmonyPostfix]
	[HarmonyPatch(typeof(FejdStartup), nameof(FejdStartup.Awake))]
	public static void FejdStartupAwakePostfix(FejdStartup __instance) {
		if (_shownThisSession) return;
		if (!BoneAppetitCompat.IsInstalled) return;
		if (Configurations.Valharvest.UseBoneAppetitCookingStations.Value) return;
		if (Configurations.Valharvest.BoneAppetitNoticeShown.Value) return;

		_shownThisSession = true;
		__instance.StartCoroutine(ShowWhenAvailable());
	}

	private static IEnumerator ShowWhenAvailable() {
		float elapsed = 0f;
		while (!UnifiedPopup.IsAvailable() && elapsed < MaxWaitSeconds) {
			yield return new WaitForSeconds(PollInterval);
			elapsed += PollInterval;
		}

		if (!UnifiedPopup.IsAvailable()) yield break;

		while (UnifiedPopup.IsVisible()) yield return null;

		UnifiedPopup.Push(new WarningPopup(
			"$valharvest_ba_notice_title",
			"$valharvest_ba_notice_body",
			OnOk));
	}

	private static void OnOk() {
		Configurations.Valharvest.BoneAppetitNoticeShown.Value = true;
		UnifiedPopup.Pop();
	}
}

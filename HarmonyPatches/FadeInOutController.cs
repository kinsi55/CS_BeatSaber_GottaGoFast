using HarmonyLib;
using System;
using System.Reflection;

namespace GottaGoFast.HarmonyPatches {
	[HarmonyPatch]
	static class FadeInOutControllerPatch {
		static void Prefix(ref float duration, ref float startDelay) {
			startDelay = 0f;
			duration = Configuration.PluginConfig.Instance.MaxFadeInTransitionTime;
		}

		static MethodBase TargetMethod() => AccessTools.Method(typeof(FadeInOutController), nameof(FadeInOutController.Fade));
		static Exception Cleanup(MethodBase original, Exception ex) => Plugin.PatchFailed(original, ex);
	}
}

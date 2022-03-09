using HarmonyLib;
using System;
using System.Reflection;

namespace GottaGoFast.HarmonyPatches {
	[HarmonyPatch]
	static class FadeInOutControllerPatch {
		static bool Prefix(FadeInOutController __instance) {
			__instance.FadeIn(Configuration.PluginConfig.Instance.MaxFadeInTransitionTime);
			return false;
		}

		static MethodBase TargetMethod() => AccessTools.Method(typeof(FadeInOutController), nameof(FadeInOutController.FadeIn), new Type[] { });
		static Exception Cleanup(MethodBase original, Exception ex) => Plugin.PatchFailed(original, ex);
	}
}

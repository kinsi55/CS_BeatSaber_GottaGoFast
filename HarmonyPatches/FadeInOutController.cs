using HarmonyLib;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;

namespace GottaGoFast.HarmonyPatches {
	[HarmonyPatch]
	static class FadeInOutControllerPatch {
		static bool Prefix(FadeInOutController __instance) {
			__instance.FadeIn(Configuration.PluginConfig.Instance.MaxFadeInTransitionTime);
			return false;
		}

		static MethodBase TargetMethod() => AccessTools.Method(typeof(FadeInOutController), nameof(FadeInOutController.FadeIn), new Type[] { });
	}
}

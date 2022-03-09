using HarmonyLib;
using System;
using System.Reflection;

namespace GottaGoFast.HarmonyPatches {

	[HarmonyPatch(typeof(DefaultScenesTransitionsFromInit), nameof(DefaultScenesTransitionsFromInit.TransitionToNextScene))]
	static class PatchHealthWarning {
		static void Prefix(ref bool goStraightToMenu) {
			if(Configuration.PluginConfig.Instance.RemoveHealthWarning)
				goStraightToMenu = true;
		}
		static Exception Cleanup(MethodBase original, Exception ex) => Plugin.PatchFailed(original, ex);
	}
}

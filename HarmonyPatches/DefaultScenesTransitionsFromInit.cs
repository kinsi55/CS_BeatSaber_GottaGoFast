using HarmonyLib;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace GottaGoFast.HarmonyPatches {

	[HarmonyPatch(typeof(DefaultScenesTransitionsFromInit), nameof(DefaultScenesTransitionsFromInit.TransitionToNextScene))]
	static class PatchHealthWarning {
		static bool Prefix(ref bool goStraightToMenu) {
			if(Configuration.PluginConfig.Instance.RemoveHealthWarning)
				goStraightToMenu = true;
			return true;
		}
	}
}

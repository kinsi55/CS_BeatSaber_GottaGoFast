using HarmonyLib;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace GottaGoFast.HarmonyPatches {
	[HarmonyPatch(typeof(StandardLevelRestartController), nameof(StandardLevelRestartController.RestartLevel))]
	static class hookRestart {
		static void Prefix() {
			PatchGameScenesManager.isRestartingSong = true;
		}
	}
}

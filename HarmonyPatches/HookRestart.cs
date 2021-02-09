using HarmonyLib;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace GottaGoFast.HarmonyPatches {
	[HarmonyPatch(typeof(StandardLevelRestartController))]
	[HarmonyPatch("RestartLevel")]
	class hookRestart {
		static void Prefix() {
			PatchGameScenesManager.isRestartingSong = true;
		}
	}
}

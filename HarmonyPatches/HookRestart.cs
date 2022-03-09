using HarmonyLib;

namespace GottaGoFast.HarmonyPatches {
	[HarmonyPatch(typeof(StandardLevelRestartController), nameof(StandardLevelRestartController.RestartLevel))]
	static class hookRestart {
		static void Prefix() {
			PatchGameScenesManager.isRestartingSong = true;
		}
	}
}

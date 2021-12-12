using HarmonyLib;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;

namespace GottaGoFast.HarmonyPatches {
	[HarmonyPatch]
	static class PatchStandardLevelFailedController {
		static IEnumerable<CodeInstruction> Transpiler(IEnumerable<CodeInstruction> instructions) {
			if(!Helper.patchDelay(instructions.ElementAt(53), 2f, Configuration.PluginConfig.Instance.SongFailDisplayTime))
				Plugin.Log.Warn("Failed to patch map fail display time");

			return instructions;
		}

		static MethodBase TargetMethod() => Helper.getCoroutine(typeof(StandardLevelFailedController), "LevelFailedCoroutine");
	}

	[HarmonyPatch]
	static class PatchMissionLevelFailedController {
		static IEnumerable<CodeInstruction> Transpiler(IEnumerable<CodeInstruction> instructions) {
			if(!Helper.patchDelay(instructions.ElementAt(52), 2f, Configuration.PluginConfig.Instance.SongFailDisplayTime))
				Plugin.Log.Warn("Failed to patch mission fail display time");

			return instructions;
		}

		static MethodBase TargetMethod() => Helper.getCoroutine(typeof(MissionLevelFailedController), "LevelFailedCoroutine");
	}
}

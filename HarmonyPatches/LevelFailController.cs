using HarmonyLib;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;

namespace GottaGoFast.HarmonyPatches {
	[HarmonyPatch]
	class PatchStandardLevelFailedController {
		static IEnumerable<CodeInstruction> Transpiler(IEnumerable<CodeInstruction> instructions) {
			if(Helper.patchDelay(instructions.ElementAt(53), 2f, Configuration.PluginConfig.Instance.SongFailDisplayTime))
				Plugin.Log.Info("Patched map fail display time");

			return instructions;
		}
		
		[HarmonyTargetMethod]
		static MethodBase TargetMethod() {
			return Helper.getCoroutine(typeof(StandardLevelFailedController), "LevelFailedCoroutine");
		}
	}

	[HarmonyPatch]
	class PatchMissionLevelFailedController {
		static IEnumerable<CodeInstruction> Transpiler(IEnumerable<CodeInstruction> instructions) {
			if(Helper.patchDelay(instructions.ElementAt(52), 2f, Configuration.PluginConfig.Instance.SongFailDisplayTime))
				Plugin.Log.Info("Patched mission fail display time");

			return instructions;
		}

		[HarmonyTargetMethod]
		static MethodBase TargetMethod() {
			return Helper.getCoroutine(typeof(MissionLevelFailedController), "LevelFailedCoroutine");
		}
	}
}

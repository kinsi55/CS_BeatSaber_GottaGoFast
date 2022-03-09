using HarmonyLib;
using System;
using System.Collections.Generic;
using System.Reflection;
using System.Reflection.Emit;

namespace GottaGoFast.HarmonyPatches {
	[HarmonyPatch]
	static class PatchStandardLevelFailedController {
		static IEnumerable<CodeInstruction> Transpiler(IEnumerable<CodeInstruction> instructions) {
			var matcher = new CodeMatcher(instructions);

			matcher.Start().MatchForward(
				false,
				new CodeMatch(OpCodes.Ldarg_0),
				new CodeMatch(OpCodes.Ldc_R4, null, "delay"),
				new CodeMatch(OpCodes.Newobj),
				new CodeMatch(OpCodes.Stfld)
			).ThrowIfInvalid("Couldnt find Delay in StandardLevelFailedController")
			.NamedMatch("delay").operand = Configuration.PluginConfig.Instance.SongFailDisplayTime;

			return matcher.InstructionEnumeration();
		}

		static IEnumerable<MethodBase> TargetMethods() {
			yield return Helper.getCoroutine(typeof(StandardLevelFailedController), nameof(StandardLevelFailedController.LevelFailedCoroutine));
			yield return Helper.getCoroutine(typeof(MissionLevelFailedController), nameof(MissionLevelFailedController.LevelFailedCoroutine));
		}

		static Exception Cleanup(MethodBase original, Exception ex) => Plugin.PatchFailed(original, ex);
	}
}

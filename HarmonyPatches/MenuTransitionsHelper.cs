using HarmonyLib;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Reflection.Emit;

namespace GottaGoFast.HarmonyPatches {

	// This only patches the main game transitions as I'm unsure if it would have an impact on MP.

	[HarmonyPatch]
	static class PatchLevelStartTransition {
		static IEnumerable<CodeInstruction> Transpiler(IEnumerable<CodeInstruction> instructions) {
			var matcher = new CodeMatcher(instructions);

			matcher.End().MatchBack(
				true,
				new CodeMatch(OpCodes.Callvirt, AccessTools.Method(typeof(GameScenesManager), nameof(GameScenesManager.PushScenes)))
			).ThrowIfInvalid("No PushScenes??")
			.Advance(-10)
			.MatchForward(false, new CodeMatch(OpCodes.Ldc_R4))
			.Instruction.operand = Configuration.PluginConfig.Instance.SongStartTransition;

			return matcher.InstructionEnumeration();
		}

		static IEnumerable<MethodBase> TargetMethods() {
			yield return typeof(MenuTransitionsHelper).GetMethods().Where(x => x.Name == nameof(MenuTransitionsHelper.StartStandardLevel)).ElementAt(1);
			yield return AccessTools.Method(typeof(MenuTransitionsHelper), nameof(MenuTransitionsHelper.StartMissionLevel));
		}

		static Exception Cleanup(MethodBase original, Exception ex) {
			if(original != null && ex != null)
				Plugin.Log.Warn(string.Format("Patching {0} {1} failed: {2}", original.ReflectedType, original.Name, ex));
			return null;
		}
	}

	[HarmonyPatch(typeof(MenuTransitionsHelper))]
	static class PatchLevelRestartTransition {
		[HarmonyPatch(nameof(MenuTransitionsHelper.HandleMainGameSceneDidFinish))]
		[HarmonyPatch(nameof(MenuTransitionsHelper.HandleMissionLevelSceneDidFinish))]
		static IEnumerable<CodeInstruction> Transpiler(IEnumerable<CodeInstruction> instructions) {
			var matcher = new CodeMatcher(instructions);

			matcher.MatchForward(true,
				new CodeMatch(OpCodes.Ldc_R4, null, "restartdelay"),
				new CodeMatch(OpCodes.Ldnull),
				new CodeMatch(OpCodes.Ldnull),
				new CodeMatch(OpCodes.Callvirt, AccessTools.Method(typeof(GameScenesManager), nameof(GameScenesManager.ReplaceScenes)))
			).ThrowIfInvalid("restartdelay not found");

			matcher.NamedMatch("restartdelay").operand = Configuration.PluginConfig.Instance.SongRestartTransition;

			matcher.End().MatchBack(
				true,
				new CodeMatch(OpCodes.Callvirt, AccessTools.Method(typeof(GameScenesManager), nameof(GameScenesManager.PopScenes)))
			).ThrowIfInvalid("No PopScenes??")
			.Advance(-10)
			.MatchForward(
				true,
				new CodeMatch(x => x.opcode == OpCodes.Beq_S || x.opcode == OpCodes.Beq || x.opcode == OpCodes.Bne_Un || x.opcode == OpCodes.Bne_Un_S),
				new CodeMatch(OpCodes.Ldc_R4, null, "exitdelay"),
				new CodeMatch(x => x.opcode == OpCodes.Br || x.opcode == OpCodes.Br_S),
				new CodeMatch(OpCodes.Ldc_R4, null, "failpassdelay")
			).ThrowIfInvalid("!");

			matcher.NamedMatch("exitdelay").operand = Configuration.PluginConfig.Instance.SongRestartTransition;
			matcher.NamedMatch("failpassdelay").operand = Configuration.PluginConfig.Instance.SongPassFailTransition;


			return matcher.InstructionEnumeration();
		}

		static Exception Cleanup(MethodBase original, Exception ex) => Plugin.PatchFailed(original, ex);
	}
}

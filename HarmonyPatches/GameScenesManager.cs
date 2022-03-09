using HarmonyLib;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Reflection;
using System.Reflection.Emit;
using UnityEngine;

namespace GottaGoFast.HarmonyPatches {
	[HarmonyPatch]
	static class PatchGameScenesManager {
		public static bool skipGc = false;

		static IEnumerable<CodeInstruction> Transpiler(IEnumerable<CodeInstruction> instructions, ILGenerator il) {
			var match = new CodeMatcher(instructions, il);

			match.End().MatchBack(
				false,
				new CodeMatch(OpCodes.Call, AccessTools.Method(typeof(GC), nameof(GC.Collect)), "GC Clear")
			)
			.ThrowIfInvalid("GC Clear not found")
			.ThrowIfNotMatchForward(
				"Expected RNG Init not found",
				new CodeMatch(OpCodes.Ldc_I4_0),
				new CodeMatch(OpCodes.Call, AccessTools.Method(typeof(UnityEngine.Random), nameof(UnityEngine.Random.InitState)))
			).CreateLabel(out var GcCallLabel)
			.Operand = AccessTools.Method(typeof(PatchGameScenesManager), nameof(__PostFix));

			match.End().MatchBack(
				false,
				new CodeMatch(OpCodes.Call, AccessTools.Method(typeof(Resources), nameof(Resources.UnloadUnusedAssets)), "UnloadCall")
			// Check if Optimizations are enabled ..
			).InsertAndAdvance(
				match.Instruction.Clone(AccessTools.Method(typeof(PatchGameScenesManager), nameof(__IsEnabled))).MoveLabelsFrom(match.NamedMatch("UnloadCall"))
			// ..If no, jump to the original UnloadUnusedAssets() call (Which will be below the following IL)
			).InsertBranchAndAdvance(OpCodes.Brfalse, match.Pos);

			if(Configuration.PluginConfig.Instance.OptimizationsAlternativeMode) {
				Label exitLabel = new Label();

				match.InsertAndAdvance(
					new CodeInstruction(OpCodes.Ldarg_0),
					new CodeInstruction(OpCodes.Ldnull),
					new CodeInstruction(OpCodes.Br, exitLabel)
				).MatchForward(
					true,
					new CodeMatch(OpCodes.Ldarg_0),
					new CodeMatch(),
					new CodeMatch(OpCodes.Stfld, null, "__current setter"),
					new CodeMatch(OpCodes.Ldarg_0)
				).ThrowIfInvalid("Couldnt find __current set")
				.NamedMatch("__current setter").labels.Add(exitLabel);
			} else {
				match.Insert(new CodeInstruction(OpCodes.Br, GcCallLabel));
			}

			return match.InstructionEnumeration();
		}

		static Exception Cleanup(MethodBase original, Exception ex) {
			if(original != null && ex != null)
				Plugin.Log.Warn(string.Format("Patching {0} {1} failed: {2}", original.ReflectedType, original.Name, ex));
			return null;
		}

		static MethodBase TargetMethod() => Helper.getCoroutine(typeof(GameScenesManager), nameof(GameScenesManager.ScenesTransitionCoroutine));

		static byte gcSkipCounter = 3;

		public static bool isRestartingSong = false;

		public static bool __IsEnabled() {
#if DEBUG
			Plugin.Log.Info("__IsEnabled called");
#endif

			return Configuration.PluginConfig.Instance.EnableOptimizations;
		}

		public static void __PostFix() {
			if(!__IsEnabled()) {
				GC.Collect();
				return;
			}

			if(isRestartingSong) {
				if(Plugin.currentScene.name == "GameCore")
					isRestartingSong = false;
				return;
			}

			if(gcSkipCounter++ % Configuration.PluginConfig.Instance.GcSkips != 0)
				return;

			Plugin.Log.Info("Running GC");
			DoGc();
		}

		public static void DoGc() {
#if DEBUG
			Plugin.Log.Notice("Running GC / Cleanup...");
			var sw = new Stopwatch();
			sw.Start();
#endif
			/*
			 * This game is super leaky (Maybe its plugins, who knows). Click on 100 songs in the song browser, 
			 * go into / leave a song a couple of times, you will already be at multiple gigabytes. Spamming GC / 
			 * WaitForPendingFinalizers + UnloadUnusedAssets literally makes no dent in the memory usage. The only
			 * I GC here is because the basegame does that here, but from my tests it doesnt actually yield anything
			 */
			GC.Collect();

#if DEBUG
			Plugin.Log.Notice(String.Format("GC took {0}ms", sw.ElapsedMilliseconds));
			sw.Restart();
#endif
		}
	}
}

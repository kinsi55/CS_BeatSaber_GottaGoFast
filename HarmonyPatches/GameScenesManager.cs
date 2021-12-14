using HarmonyLib;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Reflection.Emit;

namespace GottaGoFast.HarmonyPatches {
	[HarmonyPatch]
	static class PatchGameScenesManager {
		public static bool skipGc = false;

		const int UNLOAD_UNUSED_ASSETS = 408;
		const int STFLD_STATE_7 = 413;

		static IEnumerable<CodeInstruction> Transpiler(IEnumerable<CodeInstruction> instructions, ILGenerator il) {
			// 1.16.2+
			if(!Helper.CheckIL(instructions, new Dictionary<int, OpCode>() {
					{ UNLOAD_UNUSED_ASSETS, OpCodes.Call },
					{ 409, OpCodes.Stloc_3 },
					{ STFLD_STATE_7 + 1, OpCodes.Ldc_I4_7 },
					{ 421, OpCodes.Stfld },
					{ 422, OpCodes.Call },
					{ 423, OpCodes.Ldc_I4_0 }
				})) {
				Plugin.Log.Warn("Couldn't patch GameScenesManager, unexpected existing OpCodes");
				return instructions;
			}

			var list = instructions.ToList();

			// Original GC call, replaced with our custom logic
			list[422].operand = AccessTools.Method(typeof(PatchGameScenesManager), nameof(__PostFix));


			// Make sure it has a label
			list[STFLD_STATE_7].labels.Add(new Label());

			var unloadConditional = new CodeInstruction(OpCodes.Call, AccessTools.Method(typeof(PatchGameScenesManager), nameof(__IsEnabled))).MoveLabelsFrom(list[UNLOAD_UNUSED_ASSETS]);

			// Re-Add a new label for original unload call to break to later
			var unloadLabel = new Label();
			list[UNLOAD_UNUSED_ASSETS].labels.Add(unloadLabel);

			// Add a skip conditional for the original UnloadUnusedAssets() call
			list.InsertRange(UNLOAD_UNUSED_ASSETS, new[] {
					// Check if Optimizations are enabled
					unloadConditional,
					// If no, jump to the original UnloadUnusedAssets() call (Which will be below the following IL)
					new CodeInstruction(OpCodes.Brfalse, unloadLabel),
					// If yes, set the __current to null..
					new CodeInstruction(OpCodes.Ldarg_0),
					new CodeInstruction(OpCodes.Ldnull),
					new CodeInstruction(OpCodes.Stfld, list[412].operand),
					// And jump to the IL where it sets the state to 7, which will then go to the exit of the coroutine next frame
					new CodeInstruction(OpCodes.Br, list[STFLD_STATE_7].labels[0])
				});

			Plugin.Log.Info("Patched GameScenesManager");

			return list;
		}

		static MethodBase TargetMethod() => Helper.getCoroutine(typeof(GameScenesManager), nameof(GameScenesManager.ScenesTransitionCoroutine));

		static byte gcSkipCounter = 3;

		public static bool isRestartingSong = false;

		public static bool __IsEnabled() => Configuration.PluginConfig.Instance.EnableOptimizations;

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

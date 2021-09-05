using System;
using System.Collections.Generic;
using System.Linq;
using HarmonyLib;
using System.Reflection.Emit;
using UnityEngine.Scripting;
using UnityEngine;
using System.Reflection;
using System.Diagnostics;
using System.Threading.Tasks;

namespace GottaGoFast.HarmonyPatches {
	[HarmonyPatch]
	static class PatchGameScenesManager {
		public static bool skipGc = false;

		private static int patchOffset = 408;

		static IEnumerable<CodeInstruction> Transpiler(IEnumerable<CodeInstruction> instructions, ILGenerator il) {
			// 1.16.2+
			if(!Helper.CheckIL(instructions, new Dictionary<int, OpCode>() {
				{ patchOffset, OpCodes.Call },
				{ 409, OpCodes.Stloc_3 },
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

			// Exit point for original UnloadUnusedAssets() call
			var moddedCleanupLabel = il.DefineLabel();
			list[413].labels.Add(moddedCleanupLabel);

			// Add a skip conditional for the original UnloadUnusedAssets() call
			list.InsertRange(patchOffset, new CodeInstruction[] {
				new CodeInstruction(OpCodes.Call, AccessTools.Method(typeof(PatchGameScenesManager), nameof(__IsEnabled))).MoveLabelsFrom(list[patchOffset]),
				new CodeInstruction(OpCodes.Brtrue, moddedCleanupLabel)
			});

			Plugin.Log.Info("Patched GameScenesManager");

			return list;
		}

		static MethodBase TargetMethod() => Helper.getCoroutine(typeof(GameScenesManager), "ScenesTransitionCoroutine");

		//static byte gcInterval = 5; //Maybe config this idk
		static byte gcSkipCounterGame = 1;
		static byte gcSkipCounterMenu = 3;

		//static bool wasInSong = false;
		public static bool isRestartingSong = false;
		public static bool isStartingSong = false;

		static bool isInSong = false;

		public static bool __IsEnabled() => Configuration.PluginConfig.Instance.EnableOptimizations;

		public static void __PostFix() {
			if(!__IsEnabled()) {
				Application.backgroundLoadingPriority = ThreadPriority.Low;
				GC.Collect();
				return;
			}

			if(isRestartingSong) {
				if(isStartingSong)
					isRestartingSong = isStartingSong = false;
				return;
			}

			var YES = Plugin.currentScene.name;

			if(isInSong && YES != "GameCore") {
				Application.backgroundLoadingPriority = UnityEngine.ThreadPriority.High;
				isInSong = false;
				if(gcSkipCounterMenu++ % Configuration.PluginConfig.Instance.GcInterval != 0) return;
				Plugin.Log.Info("Running GC because Leaving song");
				//return;
				// The second condition is a failsafe
			} else if(YES == "GameCore") {
				isStartingSong = false;
				isInSong = true;
				if(gcSkipCounterGame++ % Configuration.PluginConfig.Instance.GcInterval != 0) return;
				Plugin.Log.Info("Running GC because Starting song");
			} else {
				return;
			}

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

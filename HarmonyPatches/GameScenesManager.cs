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

		static IEnumerable<CodeInstruction> Transpiler(IEnumerable<CodeInstruction> instructions) {
			if(!Configuration.PluginConfig.Instance.EnableOptimizations) {
				Plugin.Log.Info("Not patching GameScenesManager because optimizations are disabled");
				return instructions;
			}

			var yeetAmount = 0;

			// 1.16.2
			if(Helper.CheckIL(instructions, new Dictionary<int, OpCode>() {
				{ patchOffset, OpCodes.Call },
				{ 409, OpCodes.Stloc_3 },
				{ patchOffset + 14, OpCodes.Call }
			}) && (instructions.ElementAt(418).opcode == OpCodes.Leave || instructions.ElementAt(418).opcode == OpCodes.Leave_S)) {
				yeetAmount = 15;
			// <1.16.1
			} else if(Helper.CheckIL(instructions, new Dictionary<int, OpCode>() {
				{ patchOffset, OpCodes.Call },
				{ patchOffset + 1, OpCodes.Call },
				{ patchOffset + 2, OpCodes.Pop },
				{ patchOffset + 3, OpCodes.Call }
			})) {
				yeetAmount = 3;

				// For 1.16.1 specificall we also need to yeet the shaderwarmup
				if(IPA.Utilities.UnityGame.GameVersion.ToString() == "1.16.1") {
					Plugin.Log.Info("Yeeting ShaderWarmup because we are on 1.16.1");
					yeetAmount = 4;
				}
			} else {
				Plugin.Log.Warn("Couldn't patch GameScenesManager, unexpected existing OpCodes");
				return instructions;
			}

			var list = instructions.ToList();
			var labels = new List<Label>();

			for(var i = patchOffset; i < patchOffset + yeetAmount; i++) {
				if(list[i].labels.Count > 0)
					labels.AddRange(list[i].labels);
			}

			list.RemoveRange(patchOffset, yeetAmount);

			var inst = new CodeInstruction(OpCodes.Call, typeof(PatchGameScenesManager).GetMethod("__PostFix", BindingFlags.Static | BindingFlags.Public));

			inst.labels.AddRange(labels.Distinct());

			list.Insert(patchOffset, inst);

			Plugin.Log.Info("Patched GameScenesManager");

			return list.AsEnumerable();
		}

		static MethodBase TargetMethod() => Helper.getCoroutine(typeof(GameScenesManager), "ScenesTransitionCoroutine");

		static byte gcInterval = 5; //Maybe config this idk
		static byte gcSkipCounterGame = 1;
		static byte gcSkipCounterMenu = 3;

		//static bool wasInSong = false;
		public static bool isRestartingSong = false;
		public static bool isStartingSong = false;

		static bool isInSong = false;

		static void IsInSong() {
			if(IPA.Loader.PluginManager.EnabledPlugins.Any(x => x.Name == "Runtime Unity Editor (BSIPA)")) {
				GarbageCollector.GCMode = GarbageCollector.Mode.Enabled;
			} else {
				GarbageCollector.GCMode = GarbageCollector.Mode.Disabled;
			}
			Application.backgroundLoadingPriority = ThreadPriority.Low;
		}

		public static void __PostFix() {
			if(isRestartingSong) {
				if(isStartingSong)
					isRestartingSong = isStartingSong = false;
				return;
			}

			bool unload = false;

			var YES = Plugin.currentScene.name;

			if(isInSong && YES != "GameCore" && YES != "EmptyTransition") {
				GarbageCollector.GCMode = GarbageCollector.Mode.Enabled;
				isInSong = false;
				Application.backgroundLoadingPriority = ThreadPriority.High;
				if(gcSkipCounterMenu++ % gcInterval != 0) return;
				Plugin.Log.Info("Running GC because Leaving song");
				//return;
				// The second condition is a failsafe
			} else if((isStartingSong && (YES == "EmptyTransition" || YES == "GameCore")) || (!isInSong && YES == "GameCore")) {
				IsInSong();

				isStartingSong = false;
				isInSong = true;
				if(gcSkipCounterGame++ % gcInterval != 0) return;
				Plugin.Log.Info("Running GC because Starting song");

				// This was kind of an experiment to see if it helps with memory usage, doesnt look like it.
				//unload = true;
				//return;
			} else {
				return;
			}


			DoGc(unload);
		}

		public static void DoGc(bool doUnload) {
#if DEBUG
			Plugin.Log.Notice("Running GC / Cleanup...");
			var sw = new Stopwatch();
			sw.Start();
#endif

			//GarbageCollector.GCMode = GarbageCollector.Mode.Enabled;
			GC.Collect();

			if(isInSong)
				IsInSong();

#if DEBUG
			Plugin.Log.Notice(String.Format("GC took {0}ms", sw.ElapsedMilliseconds));
			sw.Restart();
#endif
			if(!doUnload)
				return;

			Resources.UnloadUnusedAssets().completed += delegate {
				Task.Delay(50).ContinueWith(x => {
#if DEBUG
					sw.Restart();
#endif
					//https://forum.unity.com/threads/resources-unloadunusedassets-vs-gc-collect.358597/
					//  "liortal's guess is correct, Resources.UnloadUnusedAssets is indeed calling GC.Collect inside. 
					// So if you already calling Resources.UnloadUnusedAssets, you shouldn't call GC.Collect"
					//GC.Collect();
#if DEBUG
					Plugin.Log.Notice(String.Format("GC(2) took {0}ms", sw.ElapsedMilliseconds));
					sw.Stop();
#endif
				});
#if DEBUG
				Plugin.Log.Notice(String.Format("Cleanup took {0}ms", sw.ElapsedMilliseconds));
#endif
			};
		}
	}
}

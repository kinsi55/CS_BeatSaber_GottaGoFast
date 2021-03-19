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
	
	class PatchGameScenesManager {
		public static bool skipGc = false;

		private static OpCode[] expectedOpcodes = { OpCodes.Call, OpCodes.Call, OpCodes.Pop };
		private static int patchOffset = 408;

		static IEnumerable<CodeInstruction> Transpiler(IEnumerable<CodeInstruction> instructions) {
			if(instructions.Count() < patchOffset + expectedOpcodes.Length + 1) {
				Plugin.Log.Warn(String.Format("Couldn't patch GameScenesManager, expected at least {1} OpCodes, found {0}", instructions.Count(), patchOffset + expectedOpcodes.Length + 1));
				return instructions;
			}

			var list = instructions.ToList();

			for(int i = 0; i < expectedOpcodes.Length; i++) {
				if(list[i + patchOffset].opcode != expectedOpcodes[i]) {
					Plugin.Log.Warn(String.Format("Couldn't patch GameScenesManager, expected IL {0} at {1}, found {2}", expectedOpcodes[i], i + patchOffset, list[i + patchOffset].opcode));
					return instructions;
				}
			}

			list.RemoveRange(patchOffset, expectedOpcodes.Length);

			list.Insert(patchOffset, new CodeInstruction(OpCodes.Call, typeof(PatchGameScenesManager).GetMethod("__PostFix", BindingFlags.Static | BindingFlags.Public)));
			list[patchOffset].labels = instructions.ElementAt(patchOffset).labels;

			return list.AsEnumerable();
		}

		static byte gcInterval = 5; //Maybe config this idk
		static byte gcSkipCounterGame = 1;
		static byte gcSkipCounterMenu = 3;

		static bool wasInSong = false;
		public static bool isRestartingSong = false;
		public static bool isStartingSong = false;

		static bool isInSong = false;

		static void IsInSong() {
			if(IPA.Loader.PluginManager.EnabledPlugins.Any(x => x.Name == "Runtime Unity Editor (BSIPA)")) {
				GarbageCollector.GCMode = GarbageCollector.Mode.Enabled;
			} else {
				GarbageCollector.GCMode = GarbageCollector.Mode.Disabled;
			}
		}

		public static void __PostFix() {
			if(isRestartingSong) {
				if(isStartingSong)
					isRestartingSong = isStartingSong = false;
				return;
			}

			if(!wasInSong && isStartingSong) {
				wasInSong = true;
				return;
			} else if(isInSong && Plugin.currentScene.name != "GameCore" && Plugin.currentScene.name != "EmptyTransition") {
				GarbageCollector.GCMode = GarbageCollector.Mode.Enabled;
				isInSong = false;
				if(gcSkipCounterMenu++ % gcInterval != 0) return;
				Plugin.Log.Info("Running GC because Leaving song");
				//return;
				// The second condition is a failsafe
			} else if((isStartingSong && Plugin.currentScene.name == "EmptyTransition") || (!isInSong && Plugin.currentScene.name == "GameCore")) {
				IsInSong();

				isStartingSong = false;
				isInSong = true;
				wasInSong = true;
				if(gcSkipCounterGame++ % gcInterval != 0) return;
				Plugin.Log.Info("Running GC because Starting song");
				//return;
			} else if(wasInSong) {
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

			//GarbageCollector.GCMode = GarbageCollector.Mode.Enabled;
			GC.Collect();

			if(isInSong)
				IsInSong();

#if DEBUG
			Plugin.Log.Notice(String.Format("GC took {0}ms", sw.ElapsedMilliseconds));
			sw.Restart();
#endif

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
					// Disable GC while ingame to prevent GC lag spikes
					if(isInSong)
						GarbageCollector.GCMode = GarbageCollector.Mode.Disabled;
				});
#if DEBUG
				Plugin.Log.Notice(String.Format("Cleanup took {0}ms", sw.ElapsedMilliseconds));
#endif
			};
		}
	}
}

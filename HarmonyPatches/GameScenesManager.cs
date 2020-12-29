using System;
using System.Collections.Generic;
using System.Linq;
using HarmonyLib;
using System.Reflection.Emit;
using UnityEngine.Scripting;
using UnityEngine;
using System.Reflection;
using System.Diagnostics;

namespace GottaGoFast.HarmonyPatches {
	
	class PatchGameScenesManager {
		public static bool skipGc = false;

		private static OpCode[] expectedOpcodes = { OpCodes.Ldc_I4_1, OpCodes.Call, OpCodes.Call, OpCodes.Call, OpCodes.Pop };
		private static int patchOffset = 408;

		static IEnumerable<CodeInstruction> Transpiler(IEnumerable<CodeInstruction> instructions) {
			if(instructions.Count() < 413) {
				Plugin.Log.Warn(String.Format("Couldn't patch GameScenesManager, expected at least 413 OpCodes, found {0}", instructions.Count()));
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

		public static void __PostFix() {
			if(!skipGc)
				DoGc();
		}

		public static void DoGc() {
#if DEBUG
			Plugin.Log.Notice("Running GC / Cleanup...");
			var sw = new Stopwatch();
			sw.Start();
#endif

			GarbageCollector.GCMode = GarbageCollector.Mode.Enabled;
			GC.Collect();
#if DEBUG
			Plugin.Log.Notice(String.Format("Gc took {0}ms", sw.ElapsedMilliseconds));
			sw.Restart();
#endif

#if DEBUG
			Resources.UnloadUnusedAssets().completed += delegate {
				sw.Stop();
				Plugin.Log.Notice(String.Format("Cleanup took {0}ms", sw.ElapsedMilliseconds));
			};
#else
			Resources.UnloadUnusedAssets();
#endif
		}
	}
}

using HarmonyLib;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Reflection;
using System.Reflection.Emit;
using UnityEngine;

namespace GottaGoFast.HarmonyPatches {
	[HarmonyPatch(typeof(GameScenesManager), nameof(GameScenesManager.ScenesTransitionCoroutine))]
	static class PatchGameScenesManager {
		static byte gcSkipCounter = 3;

		static void Prefix(ref bool canTriggerGarbageCollector) {
			if(!canTriggerGarbageCollector || !Configuration.PluginConfig.Instance.EnableOptimizations)
				return;

			if(gcSkipCounter++ % Configuration.PluginConfig.Instance.GcSkips != 0)
				canTriggerGarbageCollector = false;
		}

		static Exception Cleanup(MethodBase original, Exception ex) {
			if(original != null && ex != null)
				Plugin.Log.Warn(string.Format("Patching {0} {1} failed: {2}", original.ReflectedType, original.Name, ex));
			return null;
		}
	}
}

using GottaGoFast.Configuration;
using HarmonyLib;
using System;
using System.Reflection;

namespace GottaGoFast.HarmonyPatches
{
    [HarmonyPatch(typeof(HealthWarningViewController), nameof(HealthWarningViewController.Init))]
    static class SkipHealthWarning
    {
        static void Postfix(HealthWarningViewController __instance)
        {
			if(!PluginConfig.Instance.RemoveHealthWarning || __instance._taskCompletionSource == null)
				return;

			__instance.Complete();
		}
		static Exception Cleanup(MethodBase original, Exception ex) => Plugin.PatchFailed(original, ex);
	}
}

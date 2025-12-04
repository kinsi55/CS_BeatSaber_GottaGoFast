using HarmonyLib;
using System;
using System.Reflection;

namespace GottaGoFast.HarmonyPatches {

	[HarmonyPatch(typeof(HealthWarningScenesTransitionSetupDataSO), nameof(HealthWarningScenesTransitionSetupDataSO.Init))]
	static class PatchHealthWarning {
		static void Prefix(ref HealthWarningSceneSetupData healthWarningSceneSetupData) {
			if(Configuration.PluginConfig.Instance.RemoveHealthWarning)
			{
                healthWarningSceneSetupData.taskCompletionSource.SetResult(true);
            }
		}
		static Exception Cleanup(MethodBase original, Exception ex) => Plugin.PatchFailed(original, ex);
	}
}

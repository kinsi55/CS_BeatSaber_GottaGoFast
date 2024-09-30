using GottaGoFast.Configuration;
using HarmonyLib;

namespace GottaGoFast.HarmonyPatches
{
    [HarmonyPatch(typeof(HealthWarningViewController), nameof(HealthWarningViewController.Init))]
    static class SkipHealthWarning
    {
        static void Postfix(HealthWarningViewController __instance)
        {
            if (PluginConfig.Instance.RemoveHealthWarning)
                __instance.DismissHealthAndSafety();
        }
    }
}

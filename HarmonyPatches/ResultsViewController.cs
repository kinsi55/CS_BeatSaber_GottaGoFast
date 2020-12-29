using HarmonyLib;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace GottaGoFast.HarmonyPatches {

	[HarmonyPatch(typeof(ResultsViewController))]
	[HarmonyPatch("RestartButtonPressed")]
	class PatchResultsViewController {
		static bool Prefix() {
			Plugin.preventNextGc();
			return true;
		}
	}
}

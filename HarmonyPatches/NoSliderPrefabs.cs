using HarmonyLib;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Reflection.Emit;
using System.Text;
using System.Threading.Tasks;

namespace GottaGoFast.HarmonyPatches {
	[HarmonyPatch(typeof(BeatmapObjectsInstaller), "InstallBindings")]
	static class NoSliderPrefabs {
		static IEnumerable<CodeInstruction> Transpiler(IEnumerable<CodeInstruction> instructions) {
			var x = new CodeMatcher(instructions);
			var m = new CodeInstruction(OpCodes.Call, AccessTools.Method(typeof(NoSliderPrefabs), nameof(NoInitialSize)));

			x.MatchForward(false,
				new CodeMatch(OpCodes.Ldc_I4_S),
				new CodeMatch(y => (y.operand as MethodInfo)?.Name == "WithInitialSize"),
				new CodeMatch(OpCodes.Ldarg_0),
				new CodeMatch(y => {
					if(y.opcode != OpCodes.Ldfld)
						return false;

					if(!(y.operand is FieldInfo f))
						return false;

					return f.Name.IndexOf("slider", StringComparison.OrdinalIgnoreCase) != -1 || f.Name.IndexOf("line", StringComparison.OrdinalIgnoreCase) != -1;
				})
			).Repeat(y => {
				y.Advance(1).InsertAndAdvance(m);
			});

			return x.InstructionEnumeration();
		}

		static int NoInitialSize(int inSize) {
			if(Configuration.PluginConfig.Instance.ExperimentalLoadTimeImprovements)
				return 0;

			return inSize;
		}
	}
}

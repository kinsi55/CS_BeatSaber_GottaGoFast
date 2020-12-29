using HarmonyLib;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Reflection.Emit;
using System.Text;
using System.Threading.Tasks;

namespace GottaGoFast {
	static class Helper {
		public static bool patchDelay(CodeInstruction instruction, float expectedValue, float newValue = 0f) {
			if(instruction.opcode == OpCodes.Ldc_R4 && (float)instruction.operand == expectedValue) {
				instruction.operand = newValue;
				return true;
			}
			return false;
		}

		public static MethodInfo getCoroutine(Type targetClass, string coroutineName, string method = "MoveNext") {
			var enumeratorFn = AccessTools.FirstInner(targetClass, t => t.Name.StartsWith("<" + coroutineName))?.GetMethod(method, BindingFlags.NonPublic | BindingFlags.Instance);

			return enumeratorFn;
		}
	}
}

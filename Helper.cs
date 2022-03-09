using HarmonyLib;
using System;
using System.Reflection;
using System.Reflection.Emit;

namespace GottaGoFast {
	static class Helper {
		public static MethodInfo getCoroutine(Type targetClass, string coroutineName, string method = "MoveNext") {
			var enumeratorFn = AccessTools.FirstInner(targetClass, t => t.Name.StartsWith("<" + coroutineName))?.GetMethod(method, BindingFlags.NonPublic | BindingFlags.Instance);

			return enumeratorFn;
		}
	}
}

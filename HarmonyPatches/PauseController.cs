using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using HarmonyLib;
using UnityEngine.SceneManagement;
using System.Reflection;

namespace GottaGoFast.HarmonyPatches {

	//[HarmonyPatch(typeof(PauseController))]
	//[HarmonyPatch("HandlePauseMenuManagerDidPressRestartButton")]
	//class PatchRestartButton {
	//	static bool Prefix(PauseController __instance, ILevelRestartController ____levelRestartController) {
	//		if(!Plugin.PracticeMode)
	//			return true;

	//		var xd = typeof(SceneManager).GetMethod("Internal_ActiveSceneChanged", BindingFlags.Static | BindingFlags.NonPublic);
	//		if(xd == null)
	//			return true;

	//		Plugin._songAudio.time = 0f;
	//		SongSeekBeatmapHandler.OnSongTimeChanged(0.0f, 1f);



	//		typeof(BSEvents).GetMethod("InvokeAll", new Type[] { typeof(Action), typeof(object[]) }).Invoke(
	//			typeof(BSEvents).GetMember("Instance"),
	//			new object[] { typeof(BSEvents).GetMember("gameSceneLoaded"), null }
	//		);

	//		//xd.Invoke(null, new object[] { SceneManager.GetSceneByName("GameCore"), SceneManager.GetSceneByName("MenuViewControllers") });
	//		//Task.Delay(50).ContinueWith(t => {
	//		//	typeof(PauseController).GetMethod("HandlePauseMenuManagerDidPressContinueButton").Invoke(__instance, null);
	//		//	xd.Invoke(null, new object[] { SceneManager.GetSceneByName("MenuViewControllers"), SceneManager.GetSceneByName("GameCore") });
	//		//});

	//		typeof(PauseController).GetMethod("HandlePauseMenuManagerDidPressContinueButton").Invoke(__instance, null);
	//		//__instance.HandlePauseMenuManagerDidPressContinueButton();

	//		return false;
	//	}
	//}


	//[HarmonyPatch(typeof(SinglePlayerLevelSelectionFlowCoordinator))]
	//[HarmonyPatch("StartLevel")]
	//class xd {
	//	static bool Prefix(Action beforeSceneSwitchCallback) {
	//		Console.WriteLine("StartLevel_Pre {0} {1}", beforeSceneSwitchCallback, DateTimeOffset.Now.ToUnixTimeMilliseconds());
	//		return true;
	//	}
	//}



}
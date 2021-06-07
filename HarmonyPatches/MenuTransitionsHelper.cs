using System;
using System.Collections.Generic;
using System.Linq;
using HarmonyLib;
using System.Reflection.Emit;
using System.Reflection;

namespace GottaGoFast.HarmonyPatches {

	// This only patches the main game transitions as I'm unsure if it would have an impact on MP.
	
	[HarmonyPatch]
	static class PatchLevelStartTransition {
		static IEnumerable<CodeInstruction> Transpiler(IEnumerable<CodeInstruction> instructions) {
			if(Helper.patchDelay(instructions.ElementAt(32), 0.7f, Configuration.PluginConfig.Instance.SongStartTransition))
				Plugin.Log.Info("Patched map start transition time");

			return instructions;
		}

		static void Prefix() {
			PatchGameScenesManager.isStartingSong = true;
		}

		static MethodBase TargetMethod() => typeof(MenuTransitionsHelper).GetMethods().Where(x => x.Name == nameof(MenuTransitionsHelper.StartStandardLevel)).ElementAt(1);
	}

	[HarmonyPatch(typeof(MenuTransitionsHelper), nameof(MenuTransitionsHelper.HandleMainGameSceneDidFinish))]
	static class PatchLevelRestartTransition {
		static IEnumerable<CodeInstruction> Transpiler(IEnumerable<CodeInstruction> instructions) {
			if(Helper.patchDelay(instructions.ElementAt(29), 0.35f, Configuration.PluginConfig.Instance.SongRestartTransition))
				Plugin.Log.Info("Patched map restart transition time");
			if(Helper.patchDelay(instructions.ElementAt(31), 1.3f, Configuration.PluginConfig.Instance.SongPassFailTransition))
				Plugin.Log.Info("Patched map clear / fail transition time");

			return instructions;
		}
	}

	[HarmonyPatch(typeof(MenuTransitionsHelper), nameof(MenuTransitionsHelper.StartMissionLevel))]
	static class PatchMissionStartTransition {
		static IEnumerable<CodeInstruction> Transpiler(IEnumerable<CodeInstruction> instructions) {
			if(Helper.patchDelay(instructions.ElementAt(31), 0.7f, Configuration.PluginConfig.Instance.SongStartTransition))
				Plugin.Log.Info("Patched mission start transition time");

			return instructions;
		}
	}

	[HarmonyPatch(typeof(MenuTransitionsHelper), nameof(MenuTransitionsHelper.HandleMissionLevelSceneDidFinish))]
	static class PatchMissionRestartTransition {
		static IEnumerable<CodeInstruction> Transpiler(IEnumerable<CodeInstruction> instructions) {
			if(Helper.patchDelay(instructions.ElementAt(25), 0.35f, Configuration.PluginConfig.Instance.SongRestartTransition))
				Plugin.Log.Info("Patched mission restart transition time");
			if(Helper.patchDelay(instructions.ElementAt(27), 1.3f, Configuration.PluginConfig.Instance.SongPassFailTransition))
				Plugin.Log.Info("Patched mission clear / fail transition time");

			return instructions;
		}
	}

	/*
	 * I was experimenting before with restarts being ingame > credits > ingame instead of ingame > menu > ingame. 
	 * Didnt help much as I was poking for the load times in the wrong place, might come in handy some time, idk
	 */
	//public virtual void StartStandardLevel(string gameMode, IDifficultyBeatmap difficultyBeatmap, OverrideEnvironmentSettings overrideEnvironmentSettings, ColorScheme overrideColorScheme, GameplayModifiers gameplayModifiers, PlayerSpecificSettings playerSpecificSettings, PracticeSettings practiceSettings, string backButtonText, bool useTestNoteCutSoundEffects, Action beforeSceneSwitchCallback, Action<DiContainer> afterSceneSwitchCallback, Action<StandardLevelScenesTransitionSetupDataSO, LevelCompletionResults> levelFinishedCallback) {
	//	this._standardLevelFinishedCallback = levelFinishedCallback;
	//	this._standardLevelScenesTransitionSetupData.didFinishEvent -= this.HandleMainGameSceneDidFinish;
	//	this._standardLevelScenesTransitionSetupData.didFinishEvent += this.HandleMainGameSceneDidFinish;
	//	this._standardLevelScenesTransitionSetupData.Init(gameMode, difficultyBeatmap, overrideEnvironmentSettings, overrideColorScheme, gameplayModifiers, playerSpecificSettings, practiceSettings, backButtonText, useTestNoteCutSoundEffects);
	//	if(!this._gameScenesManager.IsSceneInStack("Credits")) {
	//		Console.WriteLine("Didnt have credits! {0}", DateTimeOffset.Now.ToUnixTimeMilliseconds());
	//		this._creditsScenesTransitionSetupData.Init();
	//		this._gameScenesManager.PushScenes(this._creditsScenesTransitionSetupData, 0f, null, delegate (DiContainer container) {
	//			Console.WriteLine("Credit setup post {0}", DateTimeOffset.Now.ToUnixTimeMilliseconds());
	//			this._gameScenesManager.PushScenes(this._standardLevelScenesTransitionSetupData, 0.05f, beforeSceneSwitchCallback, afterSceneSwitchCallback);
	//		});
	//		return;
	//	}
	//	Console.WriteLine("Had credits! {0}", DateTimeOffset.Now.ToUnixTimeMilliseconds());
	//	this._gameScenesManager.PushScenes(this._standardLevelScenesTransitionSetupData, 0.05f, beforeSceneSwitchCallback, afterSceneSwitchCallback);
	//}

	//public virtual void HandleMainGameSceneDidFinish(StandardLevelScenesTransitionSetupDataSO standardLevelScenesTransitionSetupData, LevelCompletionResults levelCompletionResults) {
	//	standardLevelScenesTransitionSetupData.didFinishEvent -= this.HandleMainGameSceneDidFinish;
	//	this._gameScenesManager.PopScenes((levelCompletionResults.levelEndStateType == LevelCompletionResults.LevelEndStateType.Failed || levelCompletionResults.levelEndStateType == LevelCompletionResults.LevelEndStateType.Cleared) ? 1.3f : 0.05f, null, delegate (DiContainer container) {
	//		if(levelCompletionResults.levelEndAction != LevelCompletionResults.LevelEndAction.Restart) {
	//			this._gameScenesManager.PopScenes(0f, null, null);
	//		}
	//		Action<StandardLevelScenesTransitionSetupDataSO, LevelCompletionResults> standardLevelFinishedCallback = this._standardLevelFinishedCallback;
	//		if(standardLevelFinishedCallback == null) {
	//			return;
	//		}
	//		standardLevelFinishedCallback(standardLevelScenesTransitionSetupData, levelCompletionResults);
	//	});
	//}

}

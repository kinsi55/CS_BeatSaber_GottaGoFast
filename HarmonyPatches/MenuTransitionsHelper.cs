using System;
using System.Collections.Generic;
using System.Linq;
using HarmonyLib;
using System.Reflection.Emit;

namespace GottaGoFast.HarmonyPatches {

	// This only patches the main game transitions as I'm unsure if it would have an impact on MP.

	//[HarmonyPatch(typeof(MenuTransitionsHelper))]
	//[HarmonyPatch("StartStandardLevel", 
	//	new Type[] { typeof(Action<DiContainer> afterSceneSwitchCallback), typeof(IDifficultyBeatmap), typeof(OverrideEnvironmentSettings), typeof(ColorScheme), typeof(GameplayModifiers), typeof(PlayerSpecificSettings), typeof(PracticeSettings), typeof(string), typeof(bool), typeof(Action), typeof(Action<DiContainer>), typeof(Action<StandardLevelScenesTransitionSetupDataSO, LevelCompletionResults>) }
	//)]
	class PatchLevelStartTransition {
		static IEnumerable<CodeInstruction> Transpiler(IEnumerable<CodeInstruction> instructions) {
			if(
				Helper.patchDelay(instructions.ElementAt(31), 0.7f, Configuration.PluginConfig.Instance.SongStartTransition) || //1.12
				Helper.patchDelay(instructions.ElementAt(30), 0.7f, Configuration.PluginConfig.Instance.SongStartTransition) //1.11
			)
				Plugin.Log.Info("Patched map start transition time");

			return instructions;
		}
	}

	[HarmonyPatch(typeof(MenuTransitionsHelper))]
	[HarmonyPatch("HandleMainGameSceneDidFinish")]
	class PatchLevelRestartTransition {
		static IEnumerable<CodeInstruction> Transpiler(IEnumerable<CodeInstruction> instructions) {
			if(Helper.patchDelay(instructions.ElementAt(29), 0.35f, Configuration.PluginConfig.Instance.SongRestartTransition))
				Plugin.Log.Info("Patched map restart transition time");
			if(Helper.patchDelay(instructions.ElementAt(31), 1.3f, Configuration.PluginConfig.Instance.SongPassFailTransition))
				Plugin.Log.Info("Patched map clear / fail transition time");

			return instructions;
		}
	}

	static class Helper {
		public static bool patchDelay(CodeInstruction instruction, float expectedValue, float newValue = 0f) {
			if(instruction.opcode == OpCodes.Ldc_R4 && (float)instruction.operand == expectedValue) {
				instruction.operand = newValue;
				return true;
			}
			return false;
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

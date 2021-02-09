using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using IPA;
using IPA.Config;
using IPA.Config.Stores;
using UnityEngine.SceneManagement;
using UnityEngine;
using IPALogger = IPA.Logging.Logger;

using HarmonyLib;
using System.Reflection;
using GottaGoFast.HarmonyPatches;
using UnityEngine.Scripting;
using System.Threading.Tasks;
using System.Threading;
using BeatSaberMarkupLanguage.Settings;
using System.Runtime.CompilerServices;

namespace GottaGoFast {

	[Plugin(RuntimeOptions.SingleStartInit)]
	public class Plugin {
		public const string HarmonyId = "Kinsi55.BeatSaber.GottaGoFast";

		internal static Plugin Instance { get; private set; }
		internal static IPALogger Log { get; private set; }

		public static Harmony harmony;

		[Init]
		/// <summary>
		/// Called when the plugin is first loaded by IPA (either when the game starts or when the plugin is enabled if it starts disabled).
		/// [Init] methods that use a Constructor or called before regular methods like InitWithConfig.
		/// Only use [Init] with one Constructor.
		/// </summary>
		public void Init(IPALogger logger, Config conf) {
			Instance = this;
			Log = logger;
			Log.Info("Gotta Go Fast initialized.");
			
			Configuration.PluginConfig.Instance = conf.Generated<Configuration.PluginConfig>();
		}

		[OnStart]
		public void OnApplicationStart() {
			Log.Debug("OnApplicationStart");
			new GameObject("GottaGoFastController").AddComponent<GottaGoFastController>();

			harmony = new Harmony(HarmonyId);

			if(Configuration.PluginConfig.Instance.EnableOptimizations) {
				var enumeratorFn = Helper.getCoroutine(typeof(GameScenesManager), "ScenesTransitionCoroutine");

				if(enumeratorFn == null) {
					Log.Warn("Unable to patch GameScenesManager, couldnt find method");
				} else {
					harmony.Patch(
						enumeratorFn,
						transpiler: new HarmonyMethod(typeof(PatchGameScenesManager).GetMethod("Transpiler", BindingFlags.NonPublic | BindingFlags.Static))
					);
					Log.Info("Patched GameScenesManager");

					SceneManager.activeSceneChanged += OnActiveSceneChanged;
				}
			}
		}

		#region Disableable
		[OnEnable]
		public void OnEnable() {
			BSMLSettings.instance.AddSettingsMenu("Gotta Go Fast", "GottaGoFast.Views.settings.bsml", Configuration.PluginConfig.Instance);
			harmony.PatchAll(Assembly.GetExecutingAssembly());
		}

		[OnDisable]
		public void OnDisable() {
			BSMLSettings.instance.RemoveSettingsMenu(Configuration.PluginConfig.Instance);
			harmony.UnpatchAll(HarmonyId);
		}
		#endregion
		
		public static Scene currentScene;

		public void OnActiveSceneChanged(Scene oldScene, Scene newScene) {
#if DEBUG
			Log.Warn($"{oldScene.name} -> {newScene.name}");
#endif
			currentScene = newScene;
		}

		[OnExit]
		public void OnApplicationQuit() {
			Log.Debug("OnApplicationQuit");

		}
	}
}

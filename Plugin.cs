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
using UnityEngine.Rendering;

namespace GottaGoFast {

	[Plugin(RuntimeOptions.SingleStartInit)]
	public class Plugin {
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
			Application.backgroundLoadingPriority = UnityEngine.ThreadPriority.High;
		}

		[OnStart]
		public void OnApplicationStart() {
			Log.Debug("OnApplicationStart");

			harmony = new Harmony("Kinsi55.BeatSaber.GottaGoFast");

			SceneManager.activeSceneChanged += OnActiveSceneChanged;

			harmony.PatchAll(Assembly.GetExecutingAssembly());
		}

		#region Disableable
		[OnEnable]
		public void OnEnable() {
			BSMLSettings.instance.AddSettingsMenu("Gotta Go Fast", "GottaGoFast.Views.settings.bsml", Configuration.PluginConfig.Instance);
		}

		[OnDisable]
		public void OnDisable() {
			BSMLSettings.instance.RemoveSettingsMenu(Configuration.PluginConfig.Instance);
			harmony.UnpatchAll(harmony.Id);
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

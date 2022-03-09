using BeatSaberMarkupLanguage.Settings;
using HarmonyLib;
using IPA;
using IPA.Config;
using IPA.Config.Stores;
using System;
using System.Reflection;
using UnityEngine.SceneManagement;
using IPALogger = IPA.Logging.Logger;

namespace GottaGoFast {

	[Plugin(RuntimeOptions.DynamicInit)]
	public class Plugin {
		internal static Plugin Instance { get; private set; }
		internal static IPALogger Log { get; private set; }

		public static Harmony harmony;

		static class BsmlWrapper {
			static readonly bool hasBsml = IPA.Loader.PluginManager.GetPluginFromId("BeatSaberMarkupLanguage") != null;

			public static void EnableUI() {
				void wrap() => BSMLSettings.instance.AddSettingsMenu("Gotta Go Fast", "GottaGoFast.Views.settings.bsml", Configuration.PluginConfig.Instance);

				if(hasBsml)
					wrap();
			}
			public static void DisableUI() {
				void wrap() => BSMLSettings.instance.RemoveSettingsMenu(Configuration.PluginConfig.Instance);

				if(hasBsml)
					wrap();
			}
		}

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
			harmony = new Harmony("Kinsi55.BeatSaber.GottaGoFast");
		}

		#region Disableable
		[OnEnable]
		public void OnEnable() {
			SceneManager.activeSceneChanged += OnActiveSceneChanged;

			harmony.PatchAll(Assembly.GetExecutingAssembly());
			BsmlWrapper.EnableUI();
		}

		[OnDisable]
		public void OnDisable() {
			SceneManager.activeSceneChanged -= OnActiveSceneChanged;

			harmony.UnpatchSelf();
			BsmlWrapper.DisableUI();
		}
		#endregion

		internal static Exception PatchFailed(MethodBase method, Exception ex) {
			if(method != null && ex != null)
				Plugin.Log.Warn(string.Format("Patching {0} {1} failed: {2}", method.ReflectedType, method.Name, ex));
			return null;
		}

		public static Scene currentScene;

		public void OnActiveSceneChanged(Scene oldScene, Scene newScene) {
#if DEBUG
			Log.Warn($"{oldScene.name} -> {newScene.name}");
#endif
			currentScene = newScene;
		}
	}
}

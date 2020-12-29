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

namespace GottaGoFast {

	[Plugin(RuntimeOptions.SingleStartInit)]
	public class Plugin {
		public const string MenuSceneName = "MenuViewControllers";
		public const string GameSceneName = "GameCore";
		public const string ContextSceneName = "GameplayCore";

		internal static Plugin Instance { get; private set; }
		internal static IPALogger Log { get; private set; }

		public static Harmony harmony;

		[Init]
		/// <summary>
		/// Called when the plugin is first loaded by IPA (either when the game starts or when the plugin is enabled if it starts disabled).
		/// [Init] methods that use a Constructor or called before regular methods like InitWithConfig.
		/// Only use [Init] with one Constructor.
		/// </summary>
		public void Init(IPALogger logger) {
			Instance = this;
			Log = logger;
			Log.Info("Gotta Go Fast initialized.");
		}

		#region BSIPA Config
		[Init]
		public void InitWithConfig(Config conf) {
			Configuration.PluginConfig.Instance = conf.Generated<Configuration.PluginConfig>();
			Log.Debug("Config loaded");
		}
		#endregion

		[OnStart]
		public void OnApplicationStart() {
			Log.Debug("OnApplicationStart");
			new GameObject("GottaGoFastController").AddComponent<GottaGoFastController>();

			harmony = new Harmony("Kinsi55.BeatSaber.GottaGoFast");
			harmony.PatchAll(Assembly.GetExecutingAssembly());

			harmony.Patch(
				typeof(MenuTransitionsHelper).GetMethods().Where(x => x.Name == "StartStandardLevel").ElementAt(1),
				transpiler: new HarmonyMethod(typeof(PatchLevelStartTransition).GetMethod("Transpiler", BindingFlags.NonPublic | BindingFlags.Static))
			);

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

			var enumeratorFn2 = Helper.getCoroutine(typeof(StandardLevelFailedController), "LevelFailedCoroutine");

			if(enumeratorFn2 != null) {
				harmony.Patch(
					enumeratorFn2,
					transpiler: new HarmonyMethod(typeof(PatchStandardLevelFailedController).GetMethod("Transpiler", BindingFlags.NonPublic | BindingFlags.Static))
				);
			}

			enumeratorFn2 = Helper.getCoroutine(typeof(MissionLevelFailedController), "LevelFailedCoroutine");

			if(enumeratorFn2 != null) {
				harmony.Patch(
					enumeratorFn2,
					transpiler: new HarmonyMethod(typeof(PatchMissionLevelFailedController).GetMethod("Transpiler", BindingFlags.NonPublic | BindingFlags.Static))
				);
			}
		}


		static CancellationTokenSource gcClearCancel;
		static bool weAreInMenu = false;

		static byte gcInterval = 6; //Maybe config this idk
		static byte gcSkipCounter = 1;

		public void OnActiveSceneChanged(Scene oldScene, Scene newScene) {
#if DEBUG
			Log.Info(String.Format("SWITCHED SCENE {2}: {0} -> {1}", oldScene.name, newScene.name, DateTimeOffset.Now.ToUnixTimeMilliseconds()));
#endif

			// We'll allow the heavy stuff to run until we've gotten into the main menu
			if(newScene.name == MenuSceneName) {
				gcClearCancel = new CancellationTokenSource();
				Task.Delay(1000, gcClearCancel.Token).ContinueWith(t => {
					if(gcClearCancel.IsCancellationRequested == true)
						return;

					weAreInMenu = true;
					PatchGameScenesManager.skipGc = true;
				});
			} else if(oldScene.name == MenuSceneName) {
				gcClearCancel?.Cancel();

				if(weAreInMenu && (gcSkipCounter++ % gcInterval) == 0)
					PatchGameScenesManager.skipGc = false;
#if DEBUG
				Log.Notice(String.Format("gcSkipCounter: {0}", gcSkipCounter));
#endif
			} else if(newScene.name == GameSceneName) {
				preventNextGc();
				// Not sure if BS does this by itself (I'm assuming yes?) but it cant hurt to go sure
				GarbageCollector.GCMode = GarbageCollector.Mode.Disabled;
			}
		}

		public static void preventNextGc() {
			gcClearCancel?.Cancel();
			PatchGameScenesManager.skipGc = true;
			weAreInMenu = false;
		}

		[OnExit]
		public void OnApplicationQuit() {
			Log.Debug("OnApplicationQuit");

		}
	}
}

using HarmonyLib;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using UnityEngine;
using static BeatmapSaveData;

namespace GottaGoFast {
	/*
		Funnily enough, because the game loads/restarts songs so fast now its possible theres still things processing the (hidden) transition to
		the menu. This can cause FPS drops in the beginning of songs. One could use something like Autopause Stealth but since usually the
		frame dips are over after 1-2 seconds *most* songs that dont start instantly dont need it (In fact, many people dont
		even experience any lags at all). This "emulates" autopause stealth, except it only pauses on restarts *and* only does so if
		notes are about to approach you within ~1 second.
	*/
	static class RestartLagBandaid {
		public static AudioTimeSyncController SongController;
		public static float songStartTime;

		public static IReadOnlyList<IReadonlyBeatmapLineData> beatmapLinesData;

		public static bool pauseOnNextStart = false;
		public static bool hasNotesInBeginning = false;

		public static bool disable = true;

		public static void reset() {
			beatmapLinesData = null;
			hasNotesInBeginning = false;
			pauseOnNextStart = false;
		}

		public static void songStarted(bool isMp) {
			disable = isMp;
		}
	}
}


namespace GottaGoFast.HarmonyPatches {
	[HarmonyPatch(typeof(BeatmapObjectCallbackController))]
	[HarmonyPatch("SetNewBeatmapData")]
	class PatchBeatmapObjectCallbackController {
		static void Postfix(IReadonlyBeatmapData beatmapData) {
			RestartLagBandaid.beatmapLinesData = beatmapData.beatmapLinesData;
		}
	}

	[HarmonyPatch(typeof(AudioTimeSyncController))]
	[HarmonyPatch("StartSong")]
	class PatchAudioTimeSyncController {
		static void Postfix(AudioTimeSyncController __instance, float ____startSongTime) {
			if(RestartLagBandaid.disable || Configuration.PluginConfig.Instance.SongRestartAntiLagDelay == 0f)
				return;

			RestartLagBandaid.SongController = __instance;
			RestartLagBandaid.songStartTime = ____startSongTime;


			if(RestartLagBandaid.pauseOnNextStart) {
				RestartLagBandaid.pauseOnNextStart = false;

				var x = Resources.FindObjectsOfTypeAll<PauseController>().LastOrDefault();
				__instance.Pause();

				var cancelToken = new CancellationTokenSource();
				x.didPauseEvent += delegate {
					cancelToken.Cancel();
				};
				
				Task.Delay((int)(Configuration.PluginConfig.Instance.SongRestartAntiLagDelay * 1000), cancelToken.Token).ContinueWith(t => {
					__instance.Resume();
				}, cancelToken.Token);
			}
		}
	}

	[HarmonyPatch(typeof(PauseController))]
	[HarmonyPatch("HandlePauseMenuManagerDidPressRestartButton")]
	class PatchRestartButton {
		static void Prefix() {
			if(RestartLagBandaid.hasNotesInBeginning) {
				RestartLagBandaid.pauseOnNextStart = true;
			} else if(RestartLagBandaid.beatmapLinesData != null && !RestartLagBandaid.disable && Configuration.PluginConfig.Instance.SongRestartAntiLagDelay > 0f) {
				foreach(var line in RestartLagBandaid.beatmapLinesData) {
					foreach(var beatmapObject in line.beatmapObjectsData) {
						if(beatmapObject.beatmapObjectType != BeatmapObjectType.Note)
							continue;

						if(beatmapObject.time > RestartLagBandaid.songStartTime) {
							if(beatmapObject.time - RestartLagBandaid.songStartTime > 0.6 + Configuration.PluginConfig.Instance.SongRestartAntiLagDelay)
								break;

							RestartLagBandaid.hasNotesInBeginning = true;
							RestartLagBandaid.pauseOnNextStart = true;
							return;
						}
					}
				}
			}
		}
	}
}
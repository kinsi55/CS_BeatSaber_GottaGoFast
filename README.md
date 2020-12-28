## Hate waiting seconds to (re)start / exit songs? Me too

This plugin completely removes or shortens scene transitions which are everywhere and applies some optimizations to significantly cut down on all "main" load times in the game (Song Load / Restart / Fail / Pass / Exit). Additonally, it can remove the health warning you get when starting up the game because after confirming it the 200th time you should be aware (Configurable).

---

Currently compatible game versions: **1.12/1.13 *and* 1.11**

Tested plattforms: PC - I have no idea if this is going to work on Quest or Linux, let me know I guess? :)

### Install

Click on releases, download the dll from the latest release and place it in your plugins folder. I'll be trying to get this on ModAssistant if no major issues should arise.

### Config

- `SongStartTransition`: Transition time when starting a song (Game default is 0.7)
- `SongRestartTransition`: Transition time when restarting a song (Game default is 0.35)
- `SongPassFailTransition`: Transition time when having failed or passed a song (Game default is 1.3)
- `RemoveHealthWarning`: When true skips the Health warning and just goes straight into the main menu
- `EnableOptimizations`: Can be used to disable the deeper optimizations of this plugin that go beyond shortening transitons incase you happen to encounter issues.

**⚠️ If you happen to find the sudden scene changes to be nauseating you should probably increase `SongStartTransition` and `SongRestartTransition` a bit**

**ℹ Transition times are always to be multiplied by two as the same amount of time is used to both fade out and wait on the following black screen.**

**ℹ If you use the shortened `SongPassFailTransition` you can remove the FastFail mod as this results in a fail transition so short that FastFail doesnt even kick in**

### Benchmark / Comparison:

Game on SSD / R9 3900x CPU, time until seeing the first frame after pressing the button

- First song load after game start: 500ms (Default: 2.1 Seconds, Subsequent ones should be slightly faster in both cases)
- Song restart: 475ms (Default: 2.7 Seconds)
- Song exit to menu: 175ms (Default: 1.1 Seconds)

List of mods: BeatSaver Loader / Voting, Camera+, ChatCore, Chroma, Counters+, CustomNotes, CustomSaber, HitSoundChanger, IntroSkip, MappingExtensions, NoodleExtensions, ParticleOverdrive, PP Counter for Counters+, RumbleMod, ScorePercentage, ScoreSaber, SongBrowser / SongCore / SongDataCore / SongPlayHistory, SRM

### Known issues / Caveats

- For now the shortened transition times do not apply for Multiplayer as I had no idea if this would cause any negative effects. Other optimizations are active.

### Incompatible mods

None that I've found so far - As mentioned above running FastFail alongside this isnt really necessary

#### Shoutout to BSMG general Chat for the help

![Kek](https://i.imgur.com/eWN3UQB.png)

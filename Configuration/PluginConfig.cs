using System.Runtime.CompilerServices;
using IPA.Config.Stores;

[assembly: InternalsVisibleTo(GeneratedStore.AssemblyVisibilityTarget)]
namespace GottaGoFast.Configuration {
	internal class PluginConfig {
		public static PluginConfig Instance { get; set; }
		public virtual float SongStartTransition { get; set; } = 0f;
		public virtual float SongRestartTransition { get; set; } = 0f;
		public virtual float SongPassFailTransition { get; set; } = 0.6f;
		public virtual bool RemoveHealthWarning { get; set; } = true;

		/// <summary>
		/// This is called whenever BSIPA reads the config from disk (including when file changes are detected).
		/// </summary>
		public virtual void OnReload() {
			// Do stuff after config is read from disk.
		}

		/// <summary>
		/// Call this to force BSIPA to update the config file. This is also called by BSIPA if it detects the file was modified.
		/// </summary>
		public virtual void Changed() {
			// Do stuff when the config is changed.
		}

		/// <summary>
		/// Call this to have BSIPA copy the values from <paramref name="other"/> into this config.
		/// </summary>
		public virtual void CopyFrom(PluginConfig other) {
			// This instance's members populated from other
		}
	}
}
// CopyRight (c) CMyna. All Rights Preserved.
// file "Mod.cs".
// Licensed under MIT License.

using Colossal.IO.AssetDatabase;
using Colossal.Logging;
using Game;
using Game.Modding;
using Game.SceneFlow;
using BetterMoonLight.Systems;
using BetterMoonLight.Utils;
using Colossal.PSI.Environment;
using System.IO;
using System.Linq;

namespace BetterMoonLight
{
    public class Mod : IMod
    {

        public static readonly string ModName = "BetterMoonLight";

        public static ILog log = LogManager.GetLogger($"{nameof(BetterMoonLight)}.{nameof(Mod)}").SetShowsErrorsInUI(false);
        public static Setting Setting;
        public static TextureLoader TextureLoader;
        public static TextureLoader.Config customConfig;

        public RemakeNightLightingSystem nightLightingSystem;


        public void OnLoad(UpdateSystem updateSystem)
        {
            log.Info(nameof(OnLoad));

            if (GameManager.instance.modManager.TryGetExecutableAsset(this, out var asset))
            {
                log.Info($"Current mod asset at {asset.path}");
            }

            TextureLoader = new TextureLoader();
            Setting = new Setting(this);

            Setting.RegisterInOptionsUI();
            GameManager.instance.localizationManager.AddSource("en-US", new LocaleEN(Setting));
            
            DebugUIController.BindOverwriteNightLighting(() => Setting);
            DebugUIController.BindTexSelectionEnum(() => TextureLoader, () => Setting);
            log.Info("UI Setting Object Loaded");

            updateSystem.UpdateAfter<RemakeNightLightingSystem>(SystemUpdatePhase.GameSimulation);
            updateSystem.UpdateAfter<RemakeNightLightingSystem>(SystemUpdatePhase.EditorSimulation);
            updateSystem.UpdateAfter<RemakeNightLightingSystem>(SystemUpdatePhase.Rendering);
            updateSystem.UpdateAt<BetterMoonLightUISystem>(SystemUpdatePhase.UIUpdate);

            AssetDatabase.global.LoadSettings(nameof(BetterMoonLight), Setting, new Setting(this));
            log.Info("Setting Loaded");

            TextureLoader.onLoadConfigs += (loader) =>
            {
                // load from local mod and subscribed mod
                loader.RecursiveLoadFromDir(Path.Combine(EnvPath.kUserDataPath, "Mods"));
                loader.RecursiveLoadFromDir(Path.Combine(EnvPath.kCacheDataPath, "Mods/mods_subscribed"));
            };
            // attach debug UI shown control listener
            Setting.onSettingsApplied += (setting) =>
            {
                DebugUIController.UpdateDebugOption(Mod.Setting.ShowOptionsInDeveloperPanel);
            };

            TextureLoader.LoadConfigs();
            Setting.Apply();
        }

        public void OnDispose()
        {
            log.Info(nameof(OnDispose));
            if (Setting != null)
            {
                Setting.UnregisterInOptionsUI();
                Setting = null;
            }
        }
    }
}

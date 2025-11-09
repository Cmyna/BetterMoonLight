// CopyRight (c) CMyna. All Rights Preserved.
// file "Mod.cs".
// Licensed under MIT License.

using Colossal.IO.AssetDatabase;
using Colossal.Logging;
using Game;
using Game.Modding;
using Game.Rendering;
using Game.SceneFlow;
using Game.Simulation;
using BetterMoonLight.Systems;
using Unity.Entities;
using BetterMoonLight.Utils;
using Colossal.PSI.Environment;
using System.IO;

namespace BetterMoonLight
{
    public class Mod : IMod
    {

        public static ILog log = LogManager.GetLogger($"{nameof(BetterMoonLight)}.{nameof(Mod)}").SetShowsErrorsInUI(false);
        public static Setting Setting;
        public static TextureLoader TextureLoader;

        public RemakeNightLightingSystem nightLightingSystem;


        public void OnLoad(UpdateSystem updateSystem)
        {
            log.Info(nameof(OnLoad));

            if (GameManager.instance.modManager.TryGetExecutableAsset(this, out var asset))
            {
                log.Info($"Current mod asset at {asset.path}");
            }


            TextureLoader = new TextureLoader();
            // load from local mod
            TextureLoader.RecursiveLoadFromDir(Path.Combine(EnvPath.kUserDataPath, "Mods"));
            // load from subscribed mod
            TextureLoader.RecursiveLoadFromDir(Path.Combine(EnvPath.kCacheDataPath, "Mods/mods_subscribed"));


            Setting = new Setting(this);
            Setting.RegisterInOptionsUI();
            GameManager.instance.localizationManager.AddSource("en-US", new LocaleEN(Setting));
            // attach debug UI shown control listener
            Setting.onSettingsApplied += (setting) =>
            {
                DebugUIController.UpdateDebugOption(Mod.Setting.ShowOptionsInDeveloperPanel);
            };
            DebugUIController.BindOverwriteNightLighting(() => Setting);
            DebugUIController.BindTexSelectionEnum(() => TextureLoader, () => Setting);
            log.Info("UI Setting Object Loaded");


            updateSystem.UpdateAfter<RemakeNightLightingSystem>(SystemUpdatePhase.GameSimulation);
            updateSystem.UpdateAfter<RemakeNightLightingSystem>(SystemUpdatePhase.EditorSimulation);
            updateSystem.UpdateAfter<RemakeNightLightingSystem>(SystemUpdatePhase.Rendering);

            // updateSystem.UpdateAt<RemakeNightLightingUISystem>(SystemUpdatePhase.UIUpdate);


            AssetDatabase.global.LoadSettings(nameof(BetterMoonLight), Setting, new Setting(this));
            Setting.Apply();
            log.Info("Setting Loaded");
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

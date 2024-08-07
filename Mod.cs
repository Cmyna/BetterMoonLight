﻿// CopyRight (c) CMyna. All Rights Preserved.
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

namespace BetterMoonLight
{
    public class Mod : IMod
    {

        public static ILog log = LogManager.GetLogger($"{nameof(BetterMoonLight)}.{nameof(Mod)}").SetShowsErrorsInUI(false);
        public static Setting Setting;

        public RemakeNightLightingSystem nightLightingSystem;


        public void OnLoad(UpdateSystem updateSystem)
        {
            log.Info(nameof(OnLoad));

            if (GameManager.instance.modManager.TryGetExecutableAsset(this, out var asset))
            {
                log.Info($"Current mod asset at {asset.path}");
            }


            Setting = new Setting(this);
            Setting.RegisterInOptionsUI();
            GameManager.instance.localizationManager.AddSource("en-US", new LocaleEN(Setting));
            log.Info("UI Setting Object Loaded");

            updateSystem.UpdateAfter<RemakeNightLightingSystem>(SystemUpdatePhase.GameSimulation);
            updateSystem.UpdateAfter<RemakeNightLightingSystem>(SystemUpdatePhase.EditorSimulation);
            updateSystem.UpdateAfter<RemakeNightLightingSystem>(SystemUpdatePhase.Rendering);

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

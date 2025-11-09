// CopyRight (c) CMyna. All Rights Preserved.
// file "MoonTextureController.cs".
// Licensed under MIT License.

using BetterMoonLight.MoonTextureRenderers;
using BetterMoonLight.Utils;
using Game;
using Game.Rendering;
using Game.Simulation;

namespace BetterMoonLight.Systems
{
    public partial class RemakeNightLightingSystem : GameSystemBase
    {
        private VanillaMimic defaultRenderer;

        private TextureLoader.Config customConfig;

        protected void OnInitTextureControl()
        {
            var cameraUpdateSystem = World.GetOrCreateSystemManaged<CameraUpdateSystem>();
            var planetarySystem = World.GetOrCreateSystemManaged<PlanetarySystem>();

            defaultRenderer = new VanillaMimic(cameraUpdateSystem, planetarySystem);
            OverrideRenderer = defaultRenderer;

            // init custom config
            customConfig = new TextureLoader.Config
            {
                name = "BetterMoonLight.Custom",
                caption = "Custom Texture",
                albedo = "albedo.png",
                normal = "normal.png",
                sphericalRender = Mod.Setting.CustomTextureSphereLit,
                FolderPath = Mod.Setting.CustomTextureDir,
            };
            Mod.TextureLoader.AddConfig(customConfig);

            // register setting change event listener
            Mod.Setting.onSettingsApplied += (s) =>
            {
                var setting = (Setting)s;
                UpdateCustomConfig(setting);
                UpdateTexture(setting.SelectedTexture);
            };
        }


        private void UpdateTexture(string key)
        {
            var albedo = Mod.TextureLoader.GetAlbedo(key);
            if (albedo == null) return;
            var normal = Mod.TextureLoader.GetNormal(key);
            if (normal == null) return;

            defaultRenderer.SetAlbedo(albedo);
            defaultRenderer.SetNormal(normal);
        }


        private void UpdateCustomConfig(Setting s)
        {
            if (s.CustomTextureDir != customConfig.FolderPath)
            {
                Mod.log.Info($"Update Custom Texture Dir: {s.CustomTextureDir}");
            }
            customConfig.FolderPath = s.CustomTextureDir;
            customConfig.sphericalRender = s.CustomTextureSphereLit;
        }
    }
}
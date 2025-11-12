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

        protected void OnInitTextureControl()
        {
            var cameraUpdateSystem = World.GetOrCreateSystemManaged<CameraUpdateSystem>();
            var planetarySystem = World.GetOrCreateSystemManaged<PlanetarySystem>();

            defaultRenderer = new VanillaMimic(cameraUpdateSystem, planetarySystem);
            OverrideRenderer = defaultRenderer;

            // register setting change event listener
            string selectedTextureCache = null;
            Mod.Setting.onSettingsApplied += (s) =>
            {
                var setting = (Setting)s;
                // it is a heavy job, so only selection change will do update
                if (selectedTextureCache != setting.SelectedTexture)
                {
                    selectedTextureCache = setting.SelectedTexture;
                    UpdateTexture(setting.SelectedTexture); 
                }
            };
        }


        private void UpdateTexture(string key)
        {
            var albedo = Mod.TextureLoader.GetAlbedo(key);
            if (albedo == null) return;
            var normal = Mod.TextureLoader.GetNormal(key);
            if (normal == null) return;

            // Mod.log.Info("RemakeNightLightingSystem: Update Texture " + key);
            defaultRenderer.SetAlbedo(albedo);
            defaultRenderer.SetNormal(normal);
        }
    }
}
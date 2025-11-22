// CopyRight (c) CMyna. All Rights Preserved.
// file "MoonTextureController.cs".
// Licensed under MIT License.

using BetterMoonLight.MoonTextureRenderers;
using Game;
using Game.Rendering;
using Game.Simulation;
using UnityEngine;

namespace BetterMoonLight.Systems
{
    public partial class RemakeNightLightingSystem : GameSystemBase
    {
        private VanillaMimic defaultRenderer;

        private Color moonTextureColor = Color.white;

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
                    moonSpecularLight.light.color = moonTextureColor;
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
            defaultRenderer.UseSphericalRender = () => Mod.TextureLoader.UseSphericalLitRender(key);
            moonTextureColor = CalcMoonTextureColor(albedo);
        }


        private Color CalcMoonTextureColor(Texture2D srcTexture)
        {
            var colors = srcTexture.GetPixels32();
            var pixelsCount = colors.Length;

            var avgColor = new Color(0, 0, 0, 0);
            foreach (var color in colors)
            {
                avgColor.r += color.r;
                avgColor.g += color.g;
                avgColor.b += color.b;
                avgColor.a += color.a;
            }

            avgColor.r /= pixelsCount * 255f;
            avgColor.g /= pixelsCount * 255f;
            avgColor.b /= pixelsCount * 255f;
            avgColor.a /= pixelsCount * 255f;

            return avgColor;
        }
    }
}
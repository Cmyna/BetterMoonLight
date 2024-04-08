// CopyRight (c) CMyna. All Rights Preserved.
// file "RemakeNightLightingSystem.cs".
// Licensed under MIT License.

using Game;
using Game.Objects;
using Game.Pathfind;
using Game.Simulation;
using System.Drawing;
using System.Numerics;
using Unity.Mathematics;
using UnityEngine;
using UnityEngine.Rendering;
using UnityEngine.Rendering.HighDefinition;
using static Game.Prefabs.CharacterGroup;

namespace BetterMoonLight.Systems
{
    public partial class RemakeNightLightingSystem : GameSystemBase
    {
        /// <summary>
        /// moon light just for physically based sky
        /// </summary>
        public const string kNightSkyLight = "MoonLightAtmosphere";
        public const string kMoonDiskLight = "MoonDiskLight";
        public const string kMoonDirectLight = "MoonLight"; // vanilla MoonLight GO
        public const string kNightAmbientLight = "NightLight"; // vanilla NightLight GO

        public float AmbientTemperature = 6750f;

        public float MoonTemperature = 6750f;

        public float AmbientIntensity = 1f;

        public float DirectLightIntensity = 1f;

        public float DirectLightAverager = 0.7f;

        public bool overwrite = true;

        public bool dirty = false;

        private PlanetarySystem planetarySystem;

        private LightDataEx nightSkyLight;

        private LightDataEx moonDiskLight;

        protected override void OnCreate()
        {
            base.OnCreate();
            planetarySystem = Unity.Entities.World.DefaultGameObjectInjectionWorld.GetOrCreateSystemManaged<PlanetarySystem>();
            InitNightSkyLight();
            InitMoonDiskLight();
        }


        protected override void OnUpdate()
        {
            if (dirty)
            {
                ToggleOverwrite(overwrite);
                UpdateTemperature(AmbientTemperature, MoonTemperature);
            }
            if (!overwrite) return;
            UpdateNightLightTransform();
            UpdateAmbientLight();
            UpdateNightSkyLightTransform();
            UpdateDirectMoonLight();
            UpdateMoonDisk();
        }

        private void UpdateNightLightTransform()
        {
            if (!TryGetLightData(out var nightLight, out var moonLight)) return;

            // re-transform idea: make night lighting y-angle fix to ensure consistent night intensity
            var pos = moonLight.transform.position;
            if (pos == null) return;
            pos.y = 0f;
            pos = UnityEngine.Vector3.Normalize(pos);
            pos.y = 10f;
            var lookatMat = float4x4.LookAt(pos, float3.zero, new float3(0f, 1f, 0f));
            nightLight.transform.rotation = new quaternion(lookatMat);
            nightLight.transform.position = pos;

            // Mod.log.Info("overwrite transform");
        }


        private void UpdateAmbientLight()
        {
            if (!TryGetLightData(out var nightLight, out var moonLight)) return;
            nightLight.light.intensity = AmbientIntensity;
        }



        private bool TryGetLightData(out PlanetarySystem.LightData nightLight, out PlanetarySystem.LightData moonLight)
        {
            nightLight = planetarySystem.NightLight;
            moonLight = planetarySystem.MoonLight;
            return nightLight.transform != null && 
                moonLight.transform != null &&
                nightLight.isValid && moonLight.isValid;
        }


        private void UpdateMoonDisk()
        {
            if (!TryGetLightData(out var nightLight, out var moonLight)) return;
            moonDiskLight.transform.position = moonLight.transform.position;
            moonDiskLight.transform.rotation = moonLight.transform.rotation;
            // follow the vanilla moon texture setting
            if (moonDiskLight.additionalData.surfaceTexture != moonLight.additionalData.surfaceTexture)
            {
                moonDiskLight.additionalData.surfaceTexture = moonLight.additionalData.surfaceTexture;
            }
        }


        private void UpdateDirectMoonLight()
        {
            if (!TryGetLightData(out var nightLight, out var moonLight)) return;

            var rotation = moonLight.transform.rotation;
            var multiplier = Utils.AntiLambertIntensity(rotation); // the value over 1
            // expression to ensure f(1, x) = 1, f(n, 1) = n, f(n, 0) = 1
            // where f(x, y), x => multiplier computed above, y => MoonLightAveragerStrength
            multiplier = (multiplier - 1) * DirectLightAverager + 1;
            moonLight.additionalData.intensity = DirectLightIntensity * multiplier;
        }

        private void InitNightSkyLight()
        {
            Utils.CreateDirectionalLight(kNightSkyLight, out nightSkyLight);
            var additionalData = nightSkyLight.additionalData;
            additionalData.affectDiffuse = false;
            additionalData.affectSpecular = true;
            additionalData.affectsVolumetric = true;
            additionalData.interactsWithSky = true;
            additionalData.angularDiameter = 0f;
            additionalData.lightlayersMask = LightLayerEnum.LightLayerDefault;
            additionalData.intensity = 1f;
            additionalData.lightDimmer = 0.5f;
        }


        private void InitMoonDiskLight()
        {
            Utils.CreateDirectionalLight(kMoonDiskLight, out moonDiskLight);
            var additionalData = moonDiskLight.additionalData;
            additionalData.affectDiffuse = false;
            additionalData.affectSpecular = true;
            additionalData.affectsVolumetric = true;
            additionalData.interactsWithSky = true;
            additionalData.lightlayersMask = LightLayerEnum.LightLayerDefault;
            additionalData.angularDiameter = 2f;
            additionalData.intensity = 0f;
            additionalData.lightUnit = LightUnit.Lux;
            additionalData.flareFalloff = 0;
            additionalData.flareSize = 0;
            additionalData.lightDimmer = 0.1f;

            moonDiskLight.light.color = Mathf.CorrelatedColorTemperatureToRGB(6000f);
        }


        private void UpdateNightSkyLightTransform()
        {
            if (planetarySystem.MoonLight.transform == null) return;
            if (nightSkyLight.transform == null) return;
            nightSkyLight.transform.position = planetarySystem.MoonLight.transform.position;
            nightSkyLight.transform.rotation = planetarySystem.MoonLight.transform.rotation;
            
        }


        public void ToggleOverwrite(bool doOverwrite)
        {
            overwrite = doOverwrite;
            if (!TryGetLightData(out var nightLight, out var moonLight))
            {
                // Mod.log.Warn("Could not get vanilla lighting data when trying to overwrite night lighting");
                dirty = true;
                return;
            }

            if (overwrite)
            {
                nightLight.additionalData.affectDiffuse = true;
                nightLight.additionalData.affectSpecular = false;
                nightLight.additionalData.affectsVolumetric = true;

                moonLight.additionalData.affectDiffuse = true;
                moonLight.additionalData.affectSpecular = false;
                moonLight.additionalData.affectsVolumetric = true;
                moonLight.additionalData.interactsWithSky = false;

                nightSkyLight.light.enabled = true;
                moonDiskLight.light.enabled = true;
            } else
            {
                nightLight.additionalData.affectDiffuse = true;
                nightLight.additionalData.affectSpecular = true;
                nightLight.additionalData.affectsVolumetric = true;
                nightLight.additionalData.lightDimmer = 1;
                nightLight.light.color = Mathf.CorrelatedColorTemperatureToRGB(6750f);

                moonLight.additionalData.affectDiffuse = true;
                moonLight.additionalData.affectSpecular = false;
                moonLight.additionalData.affectsVolumetric = true;
                moonLight.additionalData.lightDimmer = 1;
                moonLight.additionalData.interactsWithSky = true;
                moonLight.light.color = Mathf.CorrelatedColorTemperatureToRGB(6750f);

                nightSkyLight.light.enabled = false;
                moonDiskLight.light.enabled = false;
            }
            
        }

        public void UpdateMoonDisk(float size, float intensity)
        {
            moonDiskLight.additionalData.angularDiameter = size;
            // moon disk intensity should related to its size
            // relation is: standard_intensity = size * finetuneRatio
            var finetuneRatio = 0.01f;
            var finetuneBias = -0.0055f;
            moonDiskLight.additionalData.intensity = (size * finetuneRatio + finetuneBias) * intensity;
        }

        public void UpdateNightSky(float intensity)
        {
            nightSkyLight.light.intensity = intensity;
            // ensure increased night sky light not make specular to strong
            nightSkyLight.additionalData.lightDimmer = 0.5f / intensity;
        }

        public void UpdateTemperature(float ambientTemperature, float moonlightTemperature)
        {
            AmbientTemperature = ambientTemperature;
            MoonTemperature = moonlightTemperature;
            if (!TryGetLightData(out var nightLight, out var moonLight))
            {
                dirty = true;
                return;
            }
            nightLight.light.color = Mathf.CorrelatedColorTemperatureToRGB(AmbientTemperature);
            moonLight.light.color = Mathf.CorrelatedColorTemperatureToRGB(MoonTemperature);
            nightSkyLight.light.color = Mathf.CorrelatedColorTemperatureToRGB(MoonTemperature);
        }

    }


}

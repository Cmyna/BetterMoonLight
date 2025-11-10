// CopyRight (c) CMyna. All Rights Preserved.
// file "RemakeNightLightingSystem.cs".
// Licensed under MIT License.

using BetterMoonLight.API;
using BetterMoonLight.Utils;
using Game;
using Game.Rendering;
using Game.Simulation;
using System;
using System.Collections.Generic;
using System.Linq;
using Unity.Mathematics;
using UnityEngine;
using UnityEngine.Rendering;
using UnityEngine.Rendering.HighDefinition;

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
        public const string kMoonSpecularLight = "MoonSpecularLight";
        public const string kNightAmbientLight = "NightLight"; // vanilla NightLight GO
        public const string kVolume = "RemakeNightLightingSystemVolume";

        public const int VolumeDefaultPriority = 1500;

        public float VolumePriority
        {
            get {
                if (volume == null) return VolumeDefaultPriority; // default volume priority
                return volume.priority; 
            }
            set { if (volume != null) volume.priority = value;  }
        }

        public IMoonTextureRenderer OverrideRenderer { get; set; } = null;

        public IEnumerable<Func<bool>> PendingChanges;

        private PlanetarySystem planetarySystem;

        private LightDataEx nightSkyLight;

        private LightDataEx moonDiskLight;

        private LightDataEx moonSpecularLight;

        private Volume volume;

        private PhysicallyBasedSky sky;

        private Action RecoverSetting;

        protected override void OnCreate()
        {
            base.OnCreate();
            var world = Unity.Entities.World.DefaultGameObjectInjectionWorld;
            planetarySystem = world.GetOrCreateSystemManaged<PlanetarySystem>();

            InitNightSkyLight();
            InitMoonDiskLight();
            InitMoonSpecularLight();

            // create volume
            volume = VolumeHelper.CreateVolume(kVolume, VolumeDefaultPriority);
            VolumeHelper.GetOrCreateVolumeComponent(volume, ref sky);

            OnInitTextureControl();

            Mod.Setting.onSettingsApplied += (s) =>
            {
                ToggleOverwrite();
            };

            PendingChanges = new List<Func<bool>>();
            PendingChanges = PendingChanges.Append(ToggleOverwrite);

            Mod.log.Info("RemakeNightLightingSystem created");
        }


        protected override void OnUpdate()
        {
            if (PendingChanges != null && PendingChanges.Count() > 0)
            {
                PendingChanges = PendingChanges.Where(e => !e());
            }
            if (Mod.Setting.OverwriteNightLighting)
            {
                UpdateNightLightTransform();
                UpdateNightSky();
                UpdateDirectMoonLight();
                UpdateMoonSpecular();
                UpdateAmbientLight();
                UpdateMoonDisk();
                UpdateTemperature();
                UpdateSpaceTextureEmmision();
            }
            UpdateAurora();
        }

        private bool UpdateNightLightTransform()
        {
            if (!TryGetLightData(out var nightLight, out var moonLight)) return false;

            // re-transform idea: make night lighting y-angle fix to ensure consistent night intensity
            var pos = moonLight.transform.position;
            if (pos == null) return false;
            pos.y = 0f;
            pos = UnityEngine.Vector3.Normalize(pos);
            pos.y = 10f;
            var lookatMat = float4x4.LookAt(pos, float3.zero, new float3(0f, 1f, 0f));
            nightLight.transform.rotation = new quaternion(lookatMat);
            nightLight.transform.position = pos;

            // Mod.log.Info("overwrite transform");
            return true;
        }


        private bool UpdateAmbientLight()
        {
            if (!TryGetLightData(out var nightLight, out var moonLight)) return false;
            var ambientIntensity = Mod.Setting.AmbientLight;
            nightLight.light.intensity = ambientIntensity;
            return true;
        }


        private bool UpdateMoonDisk()
        {
            if (!TryGetLightData(out var nightLight, out var moonLight)) return false;
            moonDiskLight.transform.position = moonLight.transform.position;
            moonDiskLight.transform.rotation = moonLight.transform.rotation;

            if (Mod.Setting.DoZRotation)
            {
                moonDiskLight.transform.rotation *= Quaternion.Euler(0, 0, Mod.Setting.ZRotation);
            }
            
            // follow the vanilla moon texture setting
            if (moonDiskLight.additionalData.surfaceTexture != moonLight.additionalData.surfaceTexture)
            {
                moonDiskLight.additionalData.surfaceTexture = moonLight.additionalData.surfaceTexture;
            }
            var moonDiskSize = Mod.Setting.MoonDiskSize;
            var moonDiskIntensity = Mod.Setting.MoonDiskIntensity;
            moonDiskLight.additionalData.angularDiameter = moonDiskSize;
            // moon disk intensity should related to its size
            var finetuneRatio = 0.01f;
            var finetuneBias = -0.0055f;
            moonDiskLight.additionalData.intensity = ((float)math.pow(moonDiskSize, 1.5) * finetuneRatio + finetuneBias) * moonDiskIntensity;

            var targetTex = moonDiskLight.additionalData.surfaceTexture;
            if (Mod.Setting.OverrideTexture && OverrideRenderer != null && targetTex is RenderTexture targetRenderTex)
            {
                OverrideRenderer.Render(targetRenderTex);
            }
            
            return true;
        }


        private bool UpdateDirectMoonLight()
        {
            if (!TryGetLightData(out var nightLight, out var moonLight)) return false;
            var averagerStrength = Mod.Setting.MoonLightAveragerStrength;
            var intensity = Mod.Setting.MoonDirectionalLight;

            var rotation = moonLight.transform.rotation;
            var multiplier = Utils.AntiLambertIntensity(rotation); // the value over 1
            moonSpecularLight.additionalData.intensity = Mod.Setting.MoonDirectionalLight / multiplier / 3f;
            // expression to ensure f(1, x) = 1, f(n, 1) = n, f(n, 0) = 1
            // where f(x, y), x => multiplier computed above, y => MoonLightAveragerStrength
            multiplier = (multiplier - 1) * averagerStrength + 1;
            moonLight.additionalData.intensity = intensity * multiplier;

            
            return true;
        }



        private bool GetRecoverSetting()
        {
            if (!TryGetLightData(out var nightLight, out var moonLight))
            {
                return false;
            }

            var nightLight_affectDiffuse = nightLight.additionalData.affectDiffuse;
            var nightLight_affectSpecular = nightLight.additionalData.affectSpecular;
            var nightLight_affectsVolumetric = nightLight.additionalData.affectsVolumetric;
            var nightLight_lightDimmer = nightLight.additionalData.lightDimmer;

            var moonLight_affectDiffuse = moonLight.additionalData.affectDiffuse;
            var moonLightt_affectSpecular = moonLight.additionalData.affectSpecular;
            var moonLight_affectsVolumetric = moonLight.additionalData.affectsVolumetric;
            var moonLight_lightDimmer = moonLight.additionalData.lightDimmer;
            var moonLight_interactWithSky = moonLight.additionalData.interactsWithSky;

            var moonLight_color = moonLight.light.color;
            var nightLight_color = nightLight.light.color;

            RecoverSetting = () =>
            {
                nightLight.light.color = nightLight_color;
                nightLight.additionalData.affectDiffuse = nightLight_affectDiffuse;
                nightLight.additionalData.affectSpecular = nightLight_affectSpecular;
                nightLight.additionalData.affectsVolumetric = nightLight_affectsVolumetric;
                nightLight.additionalData.lightDimmer = nightLight_lightDimmer;

                moonLight.light.color = moonLight_color;
                moonLight.additionalData.affectDiffuse = moonLight_affectDiffuse;
                moonLight.additionalData.affectSpecular = moonLightt_affectSpecular;
                moonLight.additionalData.affectsVolumetric = moonLight_affectsVolumetric;
                moonLight.additionalData.interactsWithSky = moonLight_interactWithSky;
                moonLight.additionalData.lightDimmer = moonLight_lightDimmer;
            };

            return true;
        }
        

        /// <summary>
        /// 
        /// </summary>
        /// <returns>toggle success or not</returns>
        private bool ToggleOverwrite()
        {
            if (!TryGetLightData(out var nightLight, out var moonLight))
            {
                return false;
            }
            if (RecoverSetting == null && !GetRecoverSetting()) return false;

            if (Mod.Setting.OverwriteNightLighting)
            {
                nightLight.additionalData.affectDiffuse = true;
                nightLight.additionalData.affectSpecular = false;
                nightLight.additionalData.affectsVolumetric = true;

                moonLight.additionalData.affectDiffuse = true;
                moonLight.additionalData.affectSpecular = false;
                moonLight.additionalData.affectsVolumetric = true;
                moonLight.additionalData.interactsWithSky = false;
                moonLight.additionalData.volumetricDimmer = 0.25f;

                nightSkyLight.light.enabled = true;
                moonDiskLight.light.enabled = true;
                moonSpecularLight.light.enabled = true;
            } else
            {
                RecoverSetting();

                nightSkyLight.light.enabled = false;
                moonDiskLight.light.enabled = false;
                moonSpecularLight.light.enabled = false;
            }
            return true;
        }


        private void UpdateNightSky()
        {
            nightSkyLight.light.intensity = Mod.Setting.NightSkyLight;
            nightSkyLight.additionalData.lightDimmer = 0.5f / Mod.Setting.NightSkyLight;
        }

        private bool UpdateMoonSpecular()
        {
            if (planetarySystem.MoonLight.transform == null) return false;
            if (moonSpecularLight.transform == null) return false;
            moonSpecularLight.transform.position = planetarySystem.MoonLight.transform.position;
            moonSpecularLight.transform.rotation = planetarySystem.MoonLight.transform.rotation;
            return true;
        }

        private bool UpdateTemperature()
        {
            var nightLightTempature = Mod.Setting.NightLightTemperature;
            var directLightTemperature = Mod.Setting.MoonTemperature;
            if (!TryGetLightData(out var nightLight, out var moonLight))
            {
                return false;
            }
            nightLight.light.color = Mathf.CorrelatedColorTemperatureToRGB(nightLightTempature);
            moonLight.light.color = Mathf.CorrelatedColorTemperatureToRGB(directLightTemperature);
            moonSpecularLight.light.color = Mathf.CorrelatedColorTemperatureToRGB(directLightTemperature);
            nightSkyLight.light.color = Mathf.CorrelatedColorTemperatureToRGB(directLightTemperature);
            return true;
        }

        private void UpdateAurora()
        {
            var auroraOverwriteLevel = Mod.Setting.AuroraOverwriteLevel;
            var auroraIntensity = Mod.Setting.AuroraIntensity;
            sky.auroraBorealisEmissionMultiplier.overrideState = auroraOverwriteLevel > 0;
            sky.auroraBorealisEmissionMultiplier.value = auroraIntensity;
        }

        private void UpdateSpaceTextureEmmision()
        {
            var spaceEmissionMultiplier = Mod.Setting.StarfieldEmmisionStrength;
            sky.spaceEmissionMultiplier.Override(spaceEmissionMultiplier);
        }


        private bool TryGetLightData(out PlanetarySystem.LightData nightLight, out PlanetarySystem.LightData moonLight)
        {
            nightLight = planetarySystem.NightLight;
            moonLight = planetarySystem.MoonLight;
            return nightLight.transform != null &&
                moonLight.transform != null &&
                nightLight.isValid && moonLight.isValid;
        }


        private void InitNightSkyLight()
        {
            Utils.CreateDirectionalLight(kNightSkyLight, out nightSkyLight);
            var additionalData = nightSkyLight.additionalData;
            additionalData.affectDiffuse = false;
            additionalData.affectSpecular = false;
            additionalData.affectsVolumetric = true;
            additionalData.interactsWithSky = true;
            additionalData.angularDiameter = 0f;
            additionalData.lightlayersMask = LightLayerEnum.LightLayerDefault;
            additionalData.intensity = 1f;
            additionalData.lightDimmer = 0.5f;

            nightSkyLight.transform.position = Vector3.up;
            nightSkyLight.transform.LookAt(Vector3.zero);
        }


        private void InitMoonSpecularLight()
        {
            Utils.CreateDirectionalLight(kMoonSpecularLight, out moonSpecularLight);
            var additionalData = moonSpecularLight.additionalData;
            additionalData.affectDiffuse = false;
            additionalData.affectSpecular = true;
            additionalData.affectsVolumetric = true;
            additionalData.interactsWithSky = false;
            additionalData.angularDiameter = 0f;
            additionalData.lightlayersMask = LightLayerEnum.LightLayerDefault;
            additionalData.intensity = 1f;
            additionalData.lightDimmer = 0.1f;
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
            additionalData.lightUnit = UnityEngine.Rendering.HighDefinition.LightUnit.Lux;
            additionalData.flareFalloff = 0;
            additionalData.flareSize = 0;
            additionalData.lightDimmer = 0.1f;

            moonDiskLight.light.color = Mathf.CorrelatedColorTemperatureToRGB(6000f);
        }

    }


}

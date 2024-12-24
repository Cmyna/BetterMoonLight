// CopyRight (c) CMyna. All Rights Preserved.
// file "RemakeNightLightingSystem.cs".
// Licensed under MIT License.

using Game;
using Game.Debug;
using Game.Rendering;
using Game.Simulation;
using System.Collections.Generic;
using Unity.Mathematics;
using UnityEngine;
using UnityEngine.Rendering;
using UnityEngine.Rendering.HighDefinition;
using static UnityEngine.Rendering.DebugUI;

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

        private const string kBetterMoonLightDebugTab = "BetterMoonLight";

        public const int VolumeDefaultPriority = 1500;

        public float VolumePriority
        {
            get {
                if (volume == null) return VolumeDefaultPriority; // default volume priority
                return volume.priority; 
            }
            set { if (volume != null) volume.priority = value;  }
        }

        public bool dirty = false;


        public bool ShowDebugOptions
        {
            set
            {
                if (!value)
                {
                    var panel = DebugManager.instance.GetPanel(kBetterMoonLightDebugTab, createIfNull: false, groupIndex: 0, overrideIfExist: false);
                    DebugManager.instance.RemovePanel(panel);
                } else
                {
                    CreateDebugPanel(Mod.Setting);
                }
            }
        }

        private PlanetarySystem planetarySystem;

        private LightDataEx nightSkyLight;

        private LightDataEx moonDiskLight;

        private LightDataEx moonSpecularLight;

        private Volume volume;

        private PhysicallyBasedSky sky;

        protected override void OnCreate()
        {
            base.OnCreate();
            planetarySystem = Unity.Entities.World.DefaultGameObjectInjectionWorld.GetOrCreateSystemManaged<PlanetarySystem>();
            InitNightSkyLight();
            InitMoonDiskLight();
            InitMoonSpecularLight();

            // create volume
            volume = VolumeHelper.CreateVolume(kVolume, VolumeDefaultPriority);
            VolumeHelper.GetOrCreateVolumeComponent(volume, ref sky);

            Mod.log.Info("RemakeNightLightingSystem created");
        }


        protected override void OnUpdate()
        {
            if (dirty)
            {
                dirty = !ToggleOverwrite();
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
            // follow the vanilla moon texture setting
            if (moonDiskLight.additionalData.surfaceTexture != moonLight.additionalData.surfaceTexture)
            {
                moonDiskLight.additionalData.surfaceTexture = moonLight.additionalData.surfaceTexture;
            }
            var moonDiskSize = Mod.Setting.MoonDiskSize;
            var moonDiskIntensity = Mod.Setting.MoonDiskIntensity;
            moonDiskLight.additionalData.angularDiameter = moonDiskSize;
            // moon disk intensity should related to its size
            // relation is: standard_intensity = size * finetuneRatio
            var finetuneRatio = 0.01f;
            var finetuneBias = -0.0055f;
            moonDiskLight.additionalData.intensity = (moonDiskSize * finetuneRatio + finetuneBias) * moonDiskIntensity;
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


        public void SetDirty() { dirty = true; }


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


        private void CreateDebugPanel(Setting setting)
        {
            var panel = DebugManager.instance.GetPanel(kBetterMoonLightDebugTab, createIfNull: true, groupIndex: 0, overrideIfExist: true);
            panel.children.Add(new List<Widget>
            {
                new BoolField
                {
                    displayName=nameof(Setting.OverwriteNightLighting),
                    getter = () => setting.OverwriteNightLighting,
                    setter = (v) => {  setting.OverwriteNightLighting = v; SetDirty(); },
                },

                new Button
                {
                    displayName = nameof(Setting.ResetModSettings),
                    action = () =>
                    {
                        setting.SetDefaults();
                        setting.Apply();
                    },
                },


                new FloatField
                {
                    displayName=nameof(Setting.NightSkyLight),
                    getter = () => setting.NightSkyLight,
                    setter = (v) => { setting.NightSkyLight = v; SetDirty(); },
                    incStep = 0.05f, min = () => 0.05f, max = () => 15f
                },

                new FloatField
                {
                    displayName=nameof(Setting.AmbientLight),
                    getter = () => setting.AmbientLight,
                    setter = (v) => { setting.AmbientLight = v; SetDirty(); },
                    incStep = 0.05f, min = () => 0f, max = () => 15f
                },
                new FloatField
                {
                    displayName=nameof(Setting.NightLightTemperature),
                    getter = () => setting.NightLightTemperature,
                    setter = (v) => { setting.NightLightTemperature = v; SetDirty(); },
                    incStep = 100f, min = () => 3500f, max = () => 10000f
                },

                new FloatField
                {
                    displayName=nameof(Setting.MoonDirectionalLight),
                    getter = () => setting.MoonDirectionalLight,
                    setter = (v) => { setting.MoonDirectionalLight = v; SetDirty(); },
                    incStep = 0.05f, min = () => 0, max = () => 15
                },
                new FloatField
                {
                    displayName=nameof(Setting.MoonTemperature),
                    getter = () => setting.MoonTemperature,
                    setter = (v) => { setting.MoonTemperature = v; SetDirty(); },
                    incStep = 100, min = () => 3500, max = () => 10000
                },


                new FloatField
                {
                    displayName=nameof(Setting.MoonDiskSize),
                    getter = () => setting.MoonDiskSize,
                    setter = (v) => { setting.MoonDiskSize = v; SetDirty(); },
                    incStep = 0.05f, min = () => 0.05f, max = () => 10f
                },
                new FloatField
                {
                    displayName=nameof(Setting.MoonDiskIntensity),
                    getter = () => setting.MoonDiskIntensity,
                    setter = (v) => { setting.MoonDiskIntensity = v; SetDirty(); },
                    incStep = 0.1f, min = () => 0.1f, max = () => 10f
                },


                new FloatField
                {
                    displayName=nameof(Setting.MoonLightAveragerStrength),
                    getter = () => setting.MoonLightAveragerStrength,
                    setter = (v) => { setting.MoonLightAveragerStrength = v; SetDirty(); },
                    incStep = 0.1f, min = () => 0, max = () => 1
                },

                new FloatField
                {
                    displayName=nameof(Setting.StarfieldEmmisionStrength),
                    getter = () => setting.StarfieldEmmisionStrength,
                    setter = (v) => { setting.StarfieldEmmisionStrength = v; SetDirty(); },
                    incStep = 0.05f, min = () => 0, max = () => 1
                },


                new IntField
                {
                    displayName=nameof(Setting.AuroraOverwriteLevel),
                    getter = () => setting.AuroraOverwriteLevel,
                    setter = (v) => {  setting.AuroraOverwriteLevel = v; SetDirty(); },
                    incStep = 1, min = () => 0, max = () => 2
                },

                new FloatField
                {
                    displayName=nameof(Setting.AuroraIntensity),
                    getter = () => setting.AuroraIntensity,
                    setter = (v) => { setting.AuroraIntensity = v; SetDirty(); },
                    incStep = 0.05f, min = () => 0, max = () => 10
                },

            });
        }



    }


}

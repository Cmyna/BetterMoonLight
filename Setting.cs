// CopyRight (c) CMyna. All Rights Preserved.
// file "Setting.cs".
// Licensed under MIT License.

using Colossal;
using Colossal.IO.AssetDatabase;
using Colossal.IO.AssetDatabase.Internal;
using Game.Modding;
using Game.Settings;
using Game.UI;
using Game.UI.Widgets;
using BetterMoonLight.Systems;
using System.Collections.Generic;
using UnityEngine.Rendering.HighDefinition;
using UnityEngine.Rendering.LookDev;
using Game.UI.Localization;
using System.Collections;

namespace BetterMoonLight
{
    [FileLocation(nameof(BetterMoonLight))]
    [SettingsUIShowGroupName(kgBasic, kgNight, kgAurora)]
    public class Setting : ModSetting
    {
        public const string ksMain = "Main";

        public const string kgBasic = "Basic";
        public const string kgNight = "Night";
        public const string kgAurora = "Aurora";

        private readonly RemakeNightLightingSystem _nightLightingSystem;

        [SettingsUISection(ksMain, kgBasic)]
        public bool OverwriteNightLighting { get; set; } = true;


        [SettingsUISlider(min = 0, max = 15, step = 0.05f, unit = Unit.kFloatTwoFractions)]
        [SettingsUISection(ksMain, kgNight)]
        public float AmbientLight { get; set; } = 3.5f;

        /// <summary>
        /// control night sky light intensity
        /// </summary>
        [SettingsUISlider(min = 0.05f, max = 15, step = 0.05f, unit = Unit.kFloatTwoFractions)]
        [SettingsUISection(ksMain, kgNight)]
        public float NightSkyLight { get; set; } = 1f;


        [SettingsUISlider(min = 0, max = 15, step = 0.05f, unit = Unit.kFloatTwoFractions)]
        [SettingsUISection(ksMain, kgNight)]
        public float MoonDirectionalLight { get; set; } = 4f;


        [SettingsUISlider(min = 0.05f, max = 10f, step = 0.05f, unit = Unit.kFloatTwoFractions)]
        [SettingsUISection(ksMain, kgNight)]
        public float MoonDiskSize { get; set; } = 1.5f;

        [SettingsUISlider(min = 0.1f, max = 10f, step = 0.1f, unit = Unit.kFloatSingleFraction)]
        [SettingsUISection(ksMain, kgNight)]
        public float MoonDiskIntensity { get; set; } = 1f;


        [SettingsUISlider(min = 3500f, max = 10000f, step = 100f, unit = Unit.kFloatTwoFractions)]
        [SettingsUISection(ksMain, kgNight)]
        public float NightLightTemperature { get; set; } = 6750f;


        [SettingsUISlider(min = 3500f, max = 10000f, step = 100f, unit = Unit.kFloatTwoFractions)]
        [SettingsUISection(ksMain, kgNight)]
        public float MoonTemperature { get; set; } = 7200f;

        [SettingsUIHidden]
        public float MoonLightAveragerStrength { get; set; } = 0.7f;

        [SettingsUISlider(min = 0f, max = 1f, step = 0.05f, unit = Unit.kFloatTwoFractions)]
        [SettingsUISection(ksMain, kgNight)]
        public float StarfieldEmmisionStrength { get; set; } = 0.5f;


        [SettingsUISection(ksMain, kgNight)]
        public bool MoonLightIntensityBalance { 
            get { return MoonLightAveragerStrength > 0; }
            set { 
                if (value) MoonLightAveragerStrength = 0.7f;
                else MoonLightAveragerStrength = 0f;
            }
        }


        [SettingsUIDropdown(typeof(Setting), nameof(GetAuroraOverwriteLevels) )]
        [SettingsUISection(ksMain, kgAurora)]
        public int AuroraOverwriteLevel { get; set; } = 0;

        public DropdownItem<int>[] GetAuroraOverwriteLevels()
        {
            var optionsId = "OPTIONS.BetterMoonLight.OverwriteAuroraLevel";
            DropdownItem<int>[] items = {
                new DropdownItem<int>() { value = 0, displayName = optionsId + "[0]" },
                new DropdownItem<int>() { value = 1, displayName = optionsId + "[1]" },
                new DropdownItem<int>() { value = 2, displayName = optionsId + "[2]" }
            };
            return items;
        }


        [SettingsUISection(ksMain, kgAurora)]
        [SettingsUISlider(min = 0f, max = 10f, step = 0.05f, unit = Unit.kFloatSingleFraction)]
        public float AuroraIntensity { get; set; } = 0f;


        [SettingsUIHidden]
        public bool Contra { get; set; } = false;


        [SettingsUIButton]
        [SettingsUISection(ksMain, kgBasic)]
        [SettingsUIConfirmation]
        public bool ResetModSettings
        {
            set
            {
                SetDefaults(); // Apply defaults.
                Contra = true;
                Apply();
            }
        }


        public override void Apply()
        {
            base.Apply();
            _nightLightingSystem.ToggleOverwrite(OverwriteNightLighting);
            _nightLightingSystem.UpdateMoonDisk(MoonDiskSize, MoonDiskIntensity);
            _nightLightingSystem.UpdateNightSky(NightSkyLight);
            _nightLightingSystem.UpdateTemperature(NightLightTemperature, MoonTemperature);
            _nightLightingSystem.AmbientIntensity = AmbientLight;
            _nightLightingSystem.AmbientTemperature = NightLightTemperature;
            _nightLightingSystem.DirectLightIntensity = MoonDirectionalLight;
            _nightLightingSystem.DirectLightAverager = MoonLightAveragerStrength;
            _nightLightingSystem.MoonTemperature = MoonTemperature;

            _nightLightingSystem.VolumePriority = AuroraOverwriteLevel == 1 ? 1500 : 2500;
            _nightLightingSystem.UpdateAurora(AuroraOverwriteLevel > 0, AuroraIntensity);
            _nightLightingSystem.UpdateSpaceTextureEmmision(StarfieldEmmisionStrength);
        }



        public Setting(IMod mod, RemakeNightLightingSystem nightLightingSystem) : base(mod)
        {
            _nightLightingSystem = nightLightingSystem;
        }


        public override void SetDefaults()
        {
            AmbientLight = 3.5f;
            NightSkyLight = 1f;
            MoonDirectionalLight = 4f;
            MoonDiskSize = 1.5f;
            MoonDiskIntensity = 1f;
            NightLightTemperature = 6750f;
            MoonTemperature = 7200f;
            MoonLightIntensityBalance = true;
            StarfieldEmmisionStrength = 0.5f;
        }

    }

    public class LocaleEN : IDictionarySource
    {
        private readonly Setting m_Setting;
        public LocaleEN(Setting setting)
        {
            m_Setting = setting;
        }
        public IEnumerable<KeyValuePair<string, string>> ReadEntries(IList<IDictionaryEntryError> errors, Dictionary<string, int> indexCounts)
        {
            return new Dictionary<string, string>
            {
                { m_Setting.GetSettingsLocaleID(), "Better Moon Light" },

                { m_Setting.GetOptionGroupLocaleID(Setting.kgBasic), "Basic" },
                { m_Setting.GetOptionGroupLocaleID(Setting.kgNight), "Night Settings" },
                { m_Setting.GetOptionGroupLocaleID(Setting.kgAurora), "Aurora" },

                { m_Setting.GetOptionLabelLocaleID(nameof(Setting.OverwriteNightLighting) ), "Overwrite Night Lighting" },
                { m_Setting.GetOptionLabelLocaleID(nameof(Setting.AmbientLight) ), "Night Ambient Light Intensity" },
                { m_Setting.GetOptionLabelLocaleID(nameof(Setting.NightSkyLight) ), "Night Sky Light Intensity" },
                { m_Setting.GetOptionLabelLocaleID(nameof(Setting.MoonDirectionalLight) ), "Direct Moon Light Intensity" },
                { m_Setting.GetOptionLabelLocaleID(nameof(Setting.MoonLightAveragerStrength) ), "Moon Light Averager Strength" },

                { m_Setting.GetOptionLabelLocaleID(nameof(Setting.MoonDiskSize) ), "Moon Disk Size" },
                { m_Setting.GetOptionLabelLocaleID(nameof(Setting.MoonDiskIntensity) ), "Moon Disk Intensity" },

                { m_Setting.GetOptionLabelLocaleID(nameof(Setting.MoonTemperature) ), "Moon Light Temperature" },
                { m_Setting.GetOptionLabelLocaleID(nameof(Setting.NightLightTemperature) ), "Night Light Temperature" },

                { m_Setting.GetOptionLabelLocaleID(nameof(Setting.MoonLightIntensityBalance)), "Balance Moon Light Intensity" },

                { m_Setting.GetOptionLabelLocaleID(nameof(Setting.ResetModSettings)), "Reset To Default" },
                { m_Setting.GetOptionWarningLocaleID(nameof(Setting.ResetModSettings)), "Are you sure set to default?" },

                { m_Setting.GetOptionLabelLocaleID(nameof(Setting.AuroraOverwriteLevel) ), "Overwrite Aurora" },
                { m_Setting.GetOptionLabelLocaleID(nameof(Setting.AuroraIntensity) ), "Aurora Intensity" },

                { m_Setting.GetOptionLabelLocaleID(nameof(Setting.StarfieldEmmisionStrength) ), "Star Field Emission Strength" },

                // overwrite level drop down items locale
                { "OPTIONS.BetterMoonLight.OverwriteAuroraLevel[0]", "Not Overwrite" },
                { "OPTIONS.BetterMoonLight.OverwriteAuroraLevel[1]", "Overwrite Basic" },
                { "OPTIONS.BetterMoonLight.OverwriteAuroraLevel[2]", "Overwrite PhotoMode" },
            };
        }

        public void Unload()
        {

        }
    }
}

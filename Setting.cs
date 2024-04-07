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

namespace BetterMoonLight
{
    [FileLocation(nameof(BetterMoonLight))]
    [SettingsUIShowGroupName(kgBasic, kgNight)]
    public class Setting : ModSetting
    {
        public const string ksMain = "Main";

        public const string kgBasic = "Basic";
        public const string kgNight = "Night";

        private readonly RemakeNightLightingSystem _nightLightingSystem;

        [SettingsUISection(ksMain, kgBasic)]
        public bool OverwriteNightLighting { get; set; } = true;


        [SettingsUISlider(min = 0, max = 15, step = 0.05f, unit = Unit.kFloatTwoFractions)]
        [SettingsUISection(ksMain, kgNight)]
        public float AmbientLight { get; set; } = 3.5f;

        /// <summary>
        /// control night sky light intensity
        /// </summary>
        [SettingsUISlider(min = 0, max = 15, step = 0.05f, unit = Unit.kFloatTwoFractions)]
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


        [SettingsUISection(ksMain, kgNight)]
        public bool MoonLightIntensityBalance { 
            get { return MoonLightAveragerStrength > 0; }
            set { 
                if (value) MoonLightAveragerStrength = 0.7f;
                else MoonLightAveragerStrength = 0f;
            }
        }

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
            };
        }

        public void Unload()
        {

        }
    }
}

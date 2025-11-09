// CopyRight (c) CMyna. All Rights Preserved.
// file "Setting.cs".
// Licensed under MIT License.

using Colossal;
using Colossal.IO.AssetDatabase;
using Colossal.Json;
using Game.Modding;
using Game.Settings;
using Game.UI;
using Game.UI.Widgets;
using System;
using System.Collections.Generic;

namespace BetterMoonLight
{

    [FileLocation(nameof(BetterMoonLight))]
    [SettingsUIShowGroupName(kgBasic, kgNight, kgCustomTexture, kgAurora)]
    public partial class Setting : ModSetting
    {
        public const string ksMain = "Main";
        
        public const string kgBasic = "Basic";
        public const string kgNight = "Night";
        public const string kgAurora = "Aurora";
        public const string kgCustomTexture = "CustomTexture";

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
        public bool OverrideTexture { get; set; } = true;


        [SettingsUIHidden]
        public string SelectedTexture { get; set; } = "BetterMoonLight.Moon";

        [SettingsUIHidden]
        public bool DoZRotation { get; set; } = false;

        [SettingsUIHidden]
        public float ZRotation { get; set; } = 0f;



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


        // [SettingsUIHidden]
        // public bool Contra { get; set; } = false;
        [SettingsUISection(ksMain, kgBasic)]
        public bool ShowOptionsInDeveloperPanel { get; set; } = false;


        [SettingsUIButton]
        [SettingsUISection(ksMain, kgBasic)]
        [SettingsUIConfirmation]
        public bool ResetModSettings
        {
            set
            {
                SetDefaults(); // Apply defaults.
                // Contra = true;
                Apply();
            }
        }


        public Setting(IMod mod) : base(mod) {}


        public override void SetDefaults()
        {
            AmbientLight = 3.5f;
            NightSkyLight = 1f;
            MoonDirectionalLight = 4f;
            MoonDiskSize = 3f;
            MoonDiskIntensity = 1f;
            NightLightTemperature = 6750f;
            MoonTemperature = 7200f;
            MoonLightIntensityBalance = false;
            StarfieldEmmisionStrength = 0.5f;
        }


        public void Update(Action<Setting> update)
        {
            update(this);
            this.Apply();
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
                { m_Setting.GetOptionGroupLocaleID(Setting.kgCustomTexture), "Custom Texture" },

                { m_Setting.GetOptionLabelLocaleID(nameof(Setting.ShowOptionsInDeveloperPanel) ), "Show Options in Developer Panel" },
                { m_Setting.GetOptionDescLocaleID(nameof(Setting.ShowOptionsInDeveloperPanel) ), "Show same settings in developer panel (launch with argument '-developerMode' and open by hotkey tab). Then you can see a new tab 'BetterMoonLight' in panel" },

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

                { m_Setting.GetOptionLabelLocaleID(nameof(Setting.OverrideTexture)), "Override Texture" },

                { m_Setting.GetOptionLabelLocaleID(nameof(Setting.ResetModSettings)), "Reset To Default" },
                { m_Setting.GetOptionWarningLocaleID(nameof(Setting.ResetModSettings)), "Are you sure set to default?" },

                { m_Setting.GetOptionLabelLocaleID(nameof(Setting.AuroraOverwriteLevel) ), "Overwrite Aurora" },
                { m_Setting.GetOptionLabelLocaleID(nameof(Setting.AuroraIntensity) ), "Aurora Intensity" },

                { m_Setting.GetOptionLabelLocaleID(nameof(Setting.StarfieldEmmisionStrength) ), "Star Field Emission Strength" },

                // overwrite level drop down items locale
                { "OPTIONS.BetterMoonLight.OverwriteAuroraLevel[0]", "Not Overwrite" },
                { "OPTIONS.BetterMoonLight.OverwriteAuroraLevel[1]", "Overwrite Basic" },
                { "OPTIONS.BetterMoonLight.OverwriteAuroraLevel[2]", "Overwrite PhotoMode" },

                // CustomTexture
                { m_Setting.GetOptionLabelLocaleID(nameof(Setting.CustomTextureDir) ), "Textures Directory" },
                {
                    m_Setting.GetOptionDescLocaleID(nameof(Setting.CustomTextureDir) ), 
                    "Specify directory that stores Albedo&Normal images for moon." +
                    "The Albedo and Normal images need to be named 'albedo.png' and 'normal.png' (Only PNG Formate Supported)."  +
                    "Albedo image is necessary, Normal image file is optional."
                },
                { m_Setting.GetOptionLabelLocaleID(nameof(Setting.CustomTextureSphereLit) ), "Shpere Illumination" },
            };
        }

        public void Unload()
        {

        }
    }


    // Setting Property for Custom Texture 
    public partial class Setting : ModSetting
    {
        [SettingsUISection(ksMain, kgCustomTexture)]
        [SettingsUIDirectoryPicker]
        public string CustomTextureDir { get; set; } = "";

        [SettingsUISection(ksMain, kgCustomTexture)]
        public bool CustomTextureSphereLit { get; set; } = false;
    }
}

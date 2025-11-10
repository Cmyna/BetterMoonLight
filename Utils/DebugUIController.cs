// CopyRight (c) CMyna. All Rights Preserved.
// file "DebugPanel.cs".
// Licensed under MIT License.

using Game.Debug;
using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.Rendering;


namespace BetterMoonLight.Utils
{
    public delegate bool GetWidget(out DebugUI.Widget res);

    internal static class DebugUIController
    {

        public static GetWidget getTexSelection;

        public static GetWidget getOverrideNightLightingOption;

        private static DebugUI.Panel panel;

        public static void UpdateDebugOption(bool show)
        {
            if (!show && panel != null)
            {
                DebugManager.instance.RemovePanel(panel);
                panel = null;
            } else if (show)
            {
                Refresh();
            }
        }

        public static void Refresh()
        {
            Mod.log.Debug("Load Debug UI");
            panel = DebugManager.instance.GetPanel(
                "BetterMoonLight",
                createIfNull: true,
                groupIndex: 0,
                overrideIfExist: true
            );

            panel.children.Add(new DebugUI.BoolField
            {
                displayName = nameof(Setting.OverwriteNightLighting),
                getter = () => Mod.Setting.OverwriteNightLighting,
                setter = (v) => Mod.Setting.Update(s => s.OverwriteNightLighting = v),
            });
            panel.children.Add(new DebugUI.Button
            {
                displayName = nameof(Setting.ResetModSettings),
                action = () =>
                {
                    Mod.Setting.SetDefaults();
                    Mod.Setting.Apply();
                },
            });

            panel.children.Add(new List<DebugUI.Widget>
            {
                new DebugUI.FloatField
                {
                    displayName=nameof(Setting.NightSkyLight),
                    getter = () => Mod.Setting.NightSkyLight,
                    setter = (v) => { Mod.Setting.NightSkyLight = v; },
                    incStep = 0.05f, min = () => 0.05f, max = () => 15f
                },

                new DebugUI.FloatField
                {
                    displayName=nameof(Setting.AmbientLight),
                    getter = () => Mod.Setting.AmbientLight,
                    setter = (v) => { Mod.Setting.AmbientLight = v; },
                    incStep = 0.05f, min = () => 0f, max = () => 15f
                },
                new DebugUI.FloatField
                {
                    displayName=nameof(Setting.NightLightTemperature),
                    getter = () => Mod.Setting.NightLightTemperature,
                    setter = (v) => { Mod.Setting.NightLightTemperature = v; },
                    incStep = 100f, min = () => 3500f, max = () => 10000f
                },

                new DebugUI.FloatField
                {
                    displayName=nameof(Setting.MoonDirectionalLight),
                    getter = () => Mod.Setting.MoonDirectionalLight,
                    setter = (v) => { Mod.Setting.MoonDirectionalLight = v; },
                    incStep = 0.05f, min = () => 0, max = () => 15
                },
                new DebugUI.FloatField
                {
                    displayName=nameof(Setting.MoonTemperature),
                    getter = () => Mod.Setting.MoonTemperature,
                    setter = (v) => { Mod.Setting.MoonTemperature = v; },
                    incStep = 100, min = () => 3500, max = () => 10000
                },


                new DebugUI.FloatField
                {
                    displayName=nameof(Setting.MoonDiskSize),
                    getter = () => Mod.Setting.MoonDiskSize,
                    setter = (v) => { Mod.Setting.MoonDiskSize = v; },
                    incStep = 0.05f, min = () => 0.05f, max = () => 20f
                },
                new DebugUI.FloatField
                {
                    displayName=nameof(Setting.MoonDiskIntensity),
                    getter = () => Mod.Setting.MoonDiskIntensity,
                    setter = (v) => { Mod.Setting.MoonDiskIntensity = v; },
                    incStep = 0.1f, min = () => 0.1f, max = () => 10f
                },


                new DebugUI.FloatField
                {
                    displayName=nameof(Setting.MoonLightAveragerStrength),
                    getter = () => Mod.Setting.MoonLightAveragerStrength,
                    setter = (v) => { Mod.Setting.MoonLightAveragerStrength = v; },
                    incStep = 0.1f, min = () => 0, max = () => 1
                },

                new DebugUI.FloatField
                {
                    displayName=nameof(Setting.StarfieldEmmisionStrength),
                    getter = () => Mod.Setting.StarfieldEmmisionStrength,
                    setter = (v) => { Mod.Setting.StarfieldEmmisionStrength = v; },
                    incStep = 0.05f, min = () => 0, max = () => 1
                },


                new DebugUI.IntField
                {
                    displayName=nameof(Setting.AuroraOverwriteLevel),
                    getter = () => Mod.Setting.AuroraOverwriteLevel,
                    setter = (v) => {  Mod.Setting.AuroraOverwriteLevel = v; },
                    incStep = 1, min = () => 0, max = () => 2
                },

                new DebugUI.FloatField
                {
                    displayName=nameof(Setting.AuroraIntensity),
                    getter = () => Mod.Setting.AuroraIntensity,
                    setter = (v) => { Mod.Setting.AuroraIntensity = v; },
                    incStep = 0.05f, min = () => 0, max = () => 10
                },


                new DebugUI.BoolField
                {
                    displayName=nameof(Setting.DoZRotation),
                    getter = () => Mod.Setting.DoZRotation,
                    setter = (v) => {Mod.Setting.DoZRotation = v; },
                },

                new DebugUI.FloatField
                {
                    displayName=nameof(Setting.ZRotation),
                    getter = () => Mod.Setting.ZRotation,
                    setter = (v) => { Mod.Setting.ZRotation = v; },
                    incStep = 1f, min = () => 0, max = () => 360
                },


                new DebugUI.BoolField
                {
                    displayName=nameof(Setting.OverrideTexture),
                    getter = () => Mod.Setting.OverrideTexture,
                    setter = (v) => {Mod.Setting.OverrideTexture = v; }
                },

                new DebugUI.Button
                {
                    displayName="ReloadTextures",
                    action = () =>
                    {
                        Mod.TextureLoader?.LoadConfigs();
                        Refresh();
                    }
                }
            });

            DebugUI.Widget widget;
            if (getTexSelection != null && getTexSelection(out widget))
            {
                panel.children.Add(widget);
            }
        }


        public static void BindOverwriteNightLighting(Func<Setting> getSetting)
        {
            getOverrideNightLightingOption = (out DebugUI.Widget res) =>
            {
                res = null;
                var setting = getSetting();
                if (setting == null) return false;

                res = new DebugUI.BoolField
                {
                    displayName = nameof(Setting.OverwriteNightLighting),
                    getter = () => setting.OverwriteNightLighting,
                    setter = (v) => setting.Update(s => s.OverwriteNightLighting = v),
                };
                return true;
            };
        }


        public static void BindTexSelectionEnum(
            Func<TextureLoader> getTexLoader,
            Func<Setting> getSetting
        )
        {
            getTexSelection = (out DebugUI.Widget res) =>
            {
                res = null;
                var texLoader = getTexLoader();
                var setting = getSetting();

                if (texLoader == null || setting == null) return false;

                var values = texLoader.Selections.ToArray();

                int[] array = values.Select((_, idx) => idx).ToArray();

                res = new DebugUI.EnumField 
                {
                    displayName = "SelectTexture",
                    getter = () => GetIndex(),
                    setter = (v) => SetValue(v),
                    enumNames = values.Select(s => new GUIContent(s)).ToArray(),
                    enumValues = array,
                    getIndex = () => GetIndex(),
                    setIndex = (v) => SetValue(v),
                };
                return true;
                int GetIndex()
                {
                    return values.Select((v, idx) => new { v, idx })
                        .FirstOrDefault(x => setting.SelectedTexture.Equals(x.v))?.idx ?? values.Length - 1;
                }
                void SetValue(int index)
                {
                    var newTexture = values[index % values.Length];
                    setting.Update(s => s.SelectedTexture = newTexture);
                }
            };
        }


    }
}

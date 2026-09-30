// CopyRight (c) CMyna. All Rights Preserved.
// file "DebugPanel.cs".
// Licensed under MIT License.

using Game.Debug;
using Game.Common;
using Game.Simulation;
using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Reflection;
using Unity.Entities;
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
        private const int TicksPerDay = 262144;
        private static readonly MethodInfo UpdateTimeMethod = typeof(TimeSystem).GetMethod("UpdateTime", BindingFlags.Instance | BindingFlags.NonPublic);
        private static string targetTimeText = "3.230";
        private static string timeStatus = "Ready";

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
            panel.children.Add(new DebugUI.BoolField
            {
                displayName = "Fix dawn lighting blackout",
                getter = () => Mod.Setting.FixLightingTransition,
                setter = (v) => Mod.Setting.Update(s => s.FixLightingTransition = v),
            });
            panel.children.Add(new DebugUI.TextField
            {
                displayName = "Target time (decimal hours)",
                getter = () => targetTimeText,
                setter = value => targetTimeText = value ?? string.Empty,
            });
            panel.children.Add(new DebugUI.Button
            {
                displayName = "Set save time (keep date)",
                action = SetCurrentDayTime,
            });
            panel.children.Add(new DebugUI.Value
            {
                displayName = "Current save time",
                getter = GetCurrentSaveTime,
            });
            panel.children.Add(new DebugUI.Value
            {
                displayName = "Time setting status",
                getter = () => timeStatus,
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

        private static object GetCurrentSaveTime()
        {
            var world = World.DefaultGameObjectInjectionWorld;
            var time = world != null && world.IsCreated ? world.GetExistingSystemManaged<TimeSystem>() : null;
            if (time == null) return "Waiting for time system";
            return time.GetCurrentDateTime().ToString("yyyy-MM-dd", CultureInfo.InvariantCulture) + " " +
                   (time.normalizedTime * 24f).ToString("F6", CultureInfo.InvariantCulture) + " h";
        }

        private static void SetCurrentDayTime()
        {
            if (!double.TryParse(targetTimeText, NumberStyles.Float, CultureInfo.InvariantCulture, out var hours) ||
                double.IsNaN(hours) || double.IsInfinity(hours) || hours < 0d || hours >= 24d)
            {
                timeStatus = "Enter decimal hours from 0 to less than 24 (e.g. 3.230)";
                return;
            }

            var world = World.DefaultGameObjectInjectionWorld;
            if (world == null || !world.IsCreated) { timeStatus = "No game world"; return; }
            var time = world.GetExistingSystemManaged<TimeSystem>();
            if (time == null || UpdateTimeMethod == null) { timeStatus = "Time system unavailable"; return; }

            var manager = world.EntityManager;
            using (var query = manager.CreateEntityQuery(ComponentType.ReadWrite<TimeData>()))
            {
                if (query.CalculateEntityCount() != 1) { timeStatus = "Expected one TimeData entity"; return; }
                var entity = query.GetSingletonEntity();
                var original = manager.GetComponentData<TimeData>(entity);
                var date = time.GetCurrentDateTime().Date;
                var currentTicks = (long)Math.Round((double)time.normalizedTime * TicksPerDay);
                var targetTicks = (long)Math.Round(hours / 24d * TicksPerDay);
                var changed = original;
                changed.m_FirstFrame = unchecked((uint)((long)original.m_FirstFrame - (targetTicks - currentTicks)));

                try
                {
                    manager.SetComponentData(entity, changed);
                    UpdateTimeMethod.Invoke(time, null);
                    if (time.GetCurrentDateTime().Date != date)
                        throw new InvalidOperationException("Date changed; time jump was rolled back");

                    var planetary = world.GetExistingSystemManaged<PlanetarySystem>();
                    if (planetary != null) planetary.overrideTime = false;
                    timeStatus = "Set to " + hours.ToString("F6", CultureInfo.InvariantCulture) +
                                 " h on " + date.ToString("yyyy-MM-dd", CultureInfo.InvariantCulture);
                }
                catch (Exception exception)
                {
                    manager.SetComponentData(entity, original);
                    try { UpdateTimeMethod.Invoke(time, null); }
                    catch (Exception restoreException) { Mod.log.Warn("Could not refresh restored time: " + restoreException); }
                    timeStatus = "Failed: " + exception.GetBaseException().Message;
                }
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

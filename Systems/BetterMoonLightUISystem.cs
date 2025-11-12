// CopyRight (c) CMyna. All Rights Preserved.
// file "BetterMoonLightUISystem.cs".
// Licensed under MIT License.


using Colossal.UI.Binding;
using Game.UI;
using System;
using System.Collections.Generic;
using System.Linq;


namespace BetterMoonLight.Systems
{
    public partial class BetterMoonLightUISystem: UISystemBase
    {
        private bool ShowSetting { get; set; } = false;
        protected override void OnCreate()
        {
            base.OnCreate();
            UseBinding("ShowSetting", () => ShowSetting, (v) => ShowSetting = v);

            UseBinding<bool>("Reset", setter: (_) => Mod.Setting.ResetModSettings = true);
            UseBinding(
                "OverrideNightLighting",
                () => Mod.Setting.OverwriteNightLighting,
                (v) => Mod.Setting.Update(s => s.OverwriteNightLighting = v)
            );


            UseBinding(
                "AmbientLight",
                () => Mod.Setting.AmbientLight,
                (v) => Mod.Setting.AmbientLight = v
            );
            UseBinding(
                "NightSkyLight",
                () => Mod.Setting.NightSkyLight,
                (v) => Mod.Setting.NightSkyLight = v
            );
            UseBinding(
                "MoonDirectionalLight",
                () => Mod.Setting.MoonDirectionalLight,
                (v) => Mod.Setting.MoonDirectionalLight = v
            );
            UseBinding(
                "MoonDiskSize",
                () => Mod.Setting.MoonDiskSize,
                (v) => Mod.Setting.MoonDiskSize = v
            );
            UseBinding(
                "MoonDiskIntensity",
                () => Mod.Setting.MoonDiskIntensity,
                (v) => Mod.Setting.MoonDiskIntensity = v
            );
            UseBinding(
                "NightLightTemperature",
                () => Mod.Setting.NightLightTemperature,
                (v) => Mod.Setting.NightLightTemperature = v
            );
            UseBinding(
                "MoonTemperature",
                () => Mod.Setting.MoonTemperature,
                (v) => Mod.Setting.MoonTemperature = v
            );
            UseBinding(
                "MoonLightAveragerStrength",
                () => Mod.Setting.MoonLightAveragerStrength,
                (v) => Mod.Setting.MoonLightAveragerStrength = v
            );
            UseBinding(
                "StarfieldEmmisionStrength",
                () => Mod.Setting.StarfieldEmmisionStrength,
                (v) => Mod.Setting.StarfieldEmmisionStrength = v
            );

            UseBinding(
                "OverrideTexture", 
                () => Mod.Setting.OverrideTexture,
                (v) => Mod.Setting.OverrideTexture = v
            );

            var selectionUpdater = new SelectionsUpdater();
            AddUpdateBinding(
                new GetterValueBinding<IEnumerable<string>>(
                    Mod.ModName, 
                    "AvailableTextures",
                    () => Mod.TextureLoader?.Selections,
                    selectionUpdater
                    // selectionUpdater
                )
            );
            
            UseBinding(
                "SelectedTexture",
                () => Mod.Setting.SelectedTexture,
                (v) => Mod.Setting.Update(s => s.SelectedTexture = v)
            );


            Mod.log.Info("BetterMoonLightUISystem Loaded");
        }


        private void UseBinding<T>(string name, Func<T> getter = null, Action<T> setter = null)
        {
            if (getter != null)
            {
                var getterValueBinding = new GetterValueBinding<T>(Mod.ModName, name, getter);
                AddUpdateBinding(getterValueBinding);
            }
            if (setter != null)
            {
                var triggerBinding = new TriggerBinding<T>(Mod.ModName, "Set" + name, setter);
                AddBinding(triggerBinding);
            }
        }


        private class SelectionsUpdater : EqualityComparer<IEnumerable<string>>, IWriter<IEnumerable<string>>
        {
            public override bool Equals(IEnumerable<string> x, IEnumerable<string> y)
            {
                return x?.SequenceEqual(y) ?? false;
            }

            public override int GetHashCode(IEnumerable<string> obj)
            {
                return obj?.Sum(x => x.GetHashCode()) ?? 0;
            }

            public void Write(IJsonWriter writer, IEnumerable<string> selections)
            {
                writer.TypeBegin("BetterMoonLight.TextxureSelections");
                writer.PropertyName("selections");
                if (selections != null)
                {
                    writer.Write(selections.ToArray());
                }
                else
                {
                    writer.WriteEmptyArray();
                }
                writer.TypeEnd();
            }
        }


    }


    
}

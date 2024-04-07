// CopyRight (c) CMyna. All Rights Preserved.
// file "Utils.cs".
// Licensed under MIT License.

using System;
using System.Collections.Generic;
using System.Linq;
using System.Numerics;
using System.Text;
using System.Threading.Tasks;
using Unity.Mathematics;
using UnityEngine;
using UnityEngine.Rendering.HighDefinition;

namespace BetterMoonLight.Systems
{
    internal class Utils
    {
        public static float AntiLambertIntensity(UnityEngine.Quaternion lightRotation)
        {
            // lambertain intensity function: cos(theta) * I, while theta is angle between light vector and normal vector
            // here we assume normal vector is perpendicular to ground plane
            var eulerAngles = lightRotation.eulerAngles;
            var angle = eulerAngles.x;
            var theta = Mathf.PI / 2 - angle * Mathf.Deg2Rad; // lambert intensity function theta
            var multiplier = 1 / Mathf.Cos(theta);
            // Mod.log.Info("angle: " + angle + ", multiplier: " + multiplier + ", theta: " + theta + ", CosTheta: " + Mathf.Cos(theta));
            // multiplier = Mathf.Max(multiplier, 0.3f);
            //multiplier = Mathf.Clamp(multiplier, 0.3f, 1f);
            return multiplier;
        }

        public static void CreateDirectionalLight(string tag, out LightDataEx lightData)
        {
            var gameObject = new GameObject(tag);
            UnityEngine.Object.DontDestroyOnLoad(gameObject);
            // gameObject.tag = tag;
            var light = gameObject.AddComponent<Light>();
            var additionalData = gameObject.AddComponent<HDAdditionalLightData>();
            light.type = LightType.Directional;
            lightData = new LightDataEx()
            {
                tag = tag,
                lightObject = gameObject,
                light = light,
                additionalData = additionalData,
                transform = gameObject.transform
            };
            //Mod.log.Info(gameObject != null);
            //Mod.log.Info(lightData.transform != null);
        }
    }
}

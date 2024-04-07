// CopyRight (c) CMyna. All Rights Preserved.
// file "LightDataEx.cs".
// Licensed under MIT License.

using UnityEngine;
using UnityEngine.Rendering.HighDefinition;

namespace BetterMoonLight.Systems
{
    public struct LightDataEx
    {
        public string tag;
        public GameObject lightObject;
        public Light light;
        public HDAdditionalLightData additionalData;
        public UnityEngine.Transform transform;
    }

}

// CopyRight (c) CMyna. All Rights Preserved.
// file "VanillaMimic.cs".
// Licensed under MIT License.

/**
 * Substantial portions derived from game code Planetary System.
 * Copyright (c) Colossal Order and Paradox Interactive.
 */

using BetterMoonLight.API;
using Game.Rendering;
using Game.Simulation;
using System;
using UnityEngine;
using UnityEngine.Rendering;

namespace BetterMoonLight.MoonTextureRenderers
{

    public class VanillaMimic : IMoonTextureRenderer
    {
        private CameraUpdateSystem cameraUpdateSystem;

        private PlanetarySystem planetarySystem;

        private Material moonMaterial;

        private int clearPass;

        private int litPass;

        private Vector2 orenNayarCoefficents;

        private Texture2D albedo;

        private Texture2D normal;

        public Func<bool> UseSphericalRender { get; set; } = () => true;



        public VanillaMimic(
            CameraUpdateSystem cameraUpdateSystem,
            PlanetarySystem planetarySystem
        )
        {
            this.cameraUpdateSystem = cameraUpdateSystem;
            this.planetarySystem = planetarySystem;
            moonMaterial = CoreUtils.CreateEngineMaterial(Shader.Find("Hidden/Satellites"));
            moonMaterial.name = "MoonMimic";
            clearPass = moonMaterial.FindPass("Clear");
            litPass = moonMaterial.FindPass("LitSatellite");
            SetSurfaceRoughness(0.8f);
            
        }

        public void SetSurfaceRoughness(float v)
        {
            float num = MathF.PI / 2f * v;
            float num2 = num * num;
            orenNayarCoefficents = new Vector2(1f - 0.5f * num2 / (num2 + 0.33f), 0.45f * num2 / (num2 + 0.09f));
        }

        public void SetAlbedo(Texture2D albedo)
        {
            this.albedo = albedo;
        }

        public void SetNormal(Texture2D normal)
        {
            this.normal = normal;
        }

        public bool Render(RenderTexture target)
        {
            if (UseSphericalRender())
            {
                return SphericalLitRender(target);
            }
            else if (albedo != null)
            {
                Graphics.Blit(albedo, target);
                target.IncrementUpdateCount();
                return true;
            }
            return false;
        }


        private bool SphericalLitRender(RenderTexture target)
        {
            var moonLight = planetarySystem.MoonLight;
            var sunLight = planetarySystem.SunLight;
            if (!moonLight.isValid || !sunLight.isValid) return false;

            Camera activeCamera = cameraUpdateSystem.activeCamera;
            if (activeCamera == null) return false;

            if (albedo == null || normal == null)
            {
                Mod.log.Error("Albedo or Normal Texture are null");
                return false;
            }

            float num = Mathf.Tan(0.5f * activeCamera.fieldOfView * MathF.PI / 180f);
            Vector4 value = new Vector4(activeCamera.aspect * num, num, activeCamera.nearClipPlane, activeCamera.farClipPlane);
            moonMaterial.SetMatrix(ShaderIDs._Camera2World, activeCamera.cameraToWorldMatrix);
            moonMaterial.SetVector(ShaderIDs._CameraData, value);
            moonMaterial.SetVector(ShaderIDs._SunDirection, sunLight.transform.forward);
            moonMaterial.SetVector(ShaderIDs._Direction, moonLight.transform.forward);
            moonMaterial.SetVector(ShaderIDs._Tangent, moonLight.transform.right);
            moonMaterial.SetVector(ShaderIDs._BiTangent, moonLight.transform.up);
            moonMaterial.SetColor(ShaderIDs._Albedo, new UnityEngine.Color(1f, 1f, 1f, 1f));
            moonMaterial.SetVector(ShaderIDs._Corners, new Vector4(0f, 0f, 1f, 1f));
            moonMaterial.SetVector(ShaderIDs._OrenNayarCoefficients, orenNayarCoefficents);
            moonMaterial.SetFloat(ShaderIDs._Luminance, 10f);

            moonMaterial.SetTexture(ShaderIDs._TexDiffuse, albedo);
            moonMaterial.SetTexture(ShaderIDs._TexNormal, normal);

            Graphics.Blit(null, target, moonMaterial, clearPass);
            Graphics.Blit(null, target, moonMaterial, litPass);
            target.IncrementUpdateCount();

            return true;
        }


        public static class ShaderIDs
        {
            public static readonly int _Camera2World = Shader.PropertyToID("_Camera2World");

            public static readonly int _CameraData = Shader.PropertyToID("_CameraData");

            public static readonly int _SunDirection = Shader.PropertyToID("_SunDirection");

            public static readonly int _Luminance = Shader.PropertyToID("_Luminance");

            public static readonly int _Direction = Shader.PropertyToID("_Direction");

            public static readonly int _Tangent = Shader.PropertyToID("_Tangent");

            public static readonly int _BiTangent = Shader.PropertyToID("_BiTangent");

            public static readonly int _Albedo = Shader.PropertyToID("_Albedo");

            public static readonly int _OrenNayarCoefficients = Shader.PropertyToID("_OrenNayarCoefficients");

            public static readonly int _TexDiffuse = Shader.PropertyToID("_TexDiffuse");

            public static readonly int _TexNormal = Shader.PropertyToID("_TexNormal");

            public static readonly int _Corners = Shader.PropertyToID("_Corners");
        }
    }


    
}

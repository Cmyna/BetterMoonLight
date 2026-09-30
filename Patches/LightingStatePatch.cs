using System;
using System.Collections.Generic;
using System.Reflection;
using System.Reflection.Emit;
using Game.Rendering;
using HarmonyLib;
using Unity.Mathematics;

namespace BetterMoonLight.Patches
{
    internal static class LightingStatePatch
    {
        private const string PatchId = "Cmyna.BetterMoonLight.SafeDawnAngle";
        private static Harmony s_harmony;

        internal static void Install()
        {
            if (s_harmony != null) return;

            var target = AccessTools.DeclaredMethod(typeof(LightingSystem), "CalculateState",
                new[] { typeof(float3), typeof(float3), typeof(float).MakeByRefType() });
            if (target == null) throw new MissingMethodException("LightingSystem.CalculateState(float3, float3, out float)");

            var harmony = new Harmony(PatchId);
            try
            {
                harmony.Patch(target, transpiler: new HarmonyMethod(
                    AccessTools.DeclaredMethod(typeof(LightingStatePatch), nameof(Transpiler))));
                s_harmony = harmony;
                Mod.log.Info("Installed dawn lighting angle fix");
            }
            catch
            {
                harmony.UnpatchAll(PatchId);
                throw;
            }
        }

        internal static void Uninstall()
        {
            if (s_harmony == null) return;
            s_harmony.UnpatchAll(PatchId);
            s_harmony = null;
        }

        private static IEnumerable<CodeInstruction> Transpiler(IEnumerable<CodeInstruction> instructions)
        {
            var acos = AccessTools.DeclaredMethod(typeof(math), nameof(math.acos), new[] { typeof(float) });
            var safeAcos = AccessTools.DeclaredMethod(typeof(LightingStatePatch), nameof(ClampedAcos));
            if (acos == null || safeAcos == null) throw new MissingMethodException("Cannot resolve dawn angle calculation");

            var codes = new List<CodeInstruction>(instructions);
            var matches = 0;
            foreach (var code in codes)
            {
                if (code.opcode != OpCodes.Call || !(code.operand is MethodInfo method) || !method.Equals(acos)) continue;
                code.operand = safeAcos;
                matches++;
            }
            if (matches != 1) throw new InvalidOperationException("Expected one math.acos call in LightingSystem.CalculateState; found " + matches);
            return codes;
        }

        public static float ClampedAcos(float value)
        {
            if (Mod.Setting != null && !Mod.Setting.FixLightingTransition) return math.acos(value);
            return math.acos(math.clamp(value, -1f, 1f));
        }
    }
}

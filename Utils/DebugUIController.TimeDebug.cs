#if DEBUG
using Game.Common;
using Game.Debug;
using Game.Simulation;
using System;
using System.Globalization;
using System.Reflection;
using Unity.Entities;
using UnityEngine.Rendering;

namespace BetterMoonLight.Utils
{
    internal static partial class DebugUIController
    {
        private const int TicksPerDay = 262144;
        private static readonly MethodInfo UpdateTimeMethod = typeof(TimeSystem).GetMethod("UpdateTime", BindingFlags.Instance | BindingFlags.NonPublic);
        private static string targetTimeText = "3.230";
        private static string timeStatus = "Ready";

        static partial void AddTimeDebugWidgets(DebugUI.Panel panel)
        {
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
        }

        private static object GetCurrentSaveTime()
        {
            var world = World.DefaultGameObjectInjectionWorld;
            var time = world != null && world.IsCreated ? world.GetExistingSystemManaged<TimeSystem>() : null;
            if (time == null) return "Waiting for time system";
            var date = time.GetCurrentDateTime();
            return date.Year.ToString(CultureInfo.InvariantCulture) + " month " +
                   date.Day.ToString(CultureInfo.InvariantCulture) + " " +
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
            var simulation = world.GetExistingSystemManaged<SimulationSystem>();
            if (time == null || simulation == null || UpdateTimeMethod == null)
            {
                timeStatus = "Time system unavailable";
                return;
            }

            var manager = world.EntityManager;
            using (var query = manager.CreateEntityQuery(ComponentType.ReadWrite<TimeData>()))
            {
                if (query.CalculateEntityCount() != 1) { timeStatus = "Expected one TimeData entity"; return; }
                var entity = query.GetSingletonEntity();
                var original = manager.GetComponentData<TimeData>(entity);
                var date = time.GetCurrentDateTime().Date;
                var frame = simulation.frameIndex;
                var signedElapsed = unchecked((int)(frame - original.m_FirstFrame));
                var originalOffset = (long)Math.Round(original.TimeOffset * TicksPerDay);
                var daySinceStart = (long)Math.Floor((signedElapsed + originalOffset) / (double)TicksPerDay);
                var targetTicks = Math.Min(TicksPerDay - 1, (long)Math.Round(hours / 24d * TicksPerDay));

                // Move the starting clock close to the requested minute. Moving only m_FirstFrame
                // past frameIndex makes the HUD's unsigned tick calculation overflow to int.MinValue.
                var startMinute = Math.Min(1439, (int)Math.Floor(hours * 60d));
                var changed = original;
                changed.m_StartingHour = (byte)(startMinute / 60);
                changed.m_StartingMinutes = (byte)(startMinute % 60);
                var newOffset = (long)Math.Round(changed.TimeOffset * TicksPerDay);
                // TimeOffset includes the game's one-second bias; exact minute boundaries
                // round up to that bias, so keep the same displayed minute without wrapping.
                targetTicks = Math.Max(targetTicks, newOffset);
                var newElapsed = daySinceStart * TicksPerDay + targetTicks - newOffset;
                if (newElapsed < 0 || newElapsed > frame || newElapsed > int.MaxValue)
                {
                    timeStatus = "Cannot set time without wrapping the simulation frame";
                    return;
                }
                changed.m_FirstFrame = frame - (uint)newElapsed;

                try
                {
                    manager.SetComponentData(entity, changed);
                    UpdateTimeMethod.Invoke(time, null);
                    if (time.GetCurrentDateTime().Date != date)
                        throw new InvalidOperationException("Date changed; time jump was rolled back");
                    if (changed.m_FirstFrame > frame)
                        throw new InvalidOperationException("Clock frame wrapped; time jump was rolled back");

                    var planetary = world.GetExistingSystemManaged<PlanetarySystem>();
                    if (planetary != null) planetary.overrideTime = false;
                    timeStatus = "Set to " + hours.ToString("F6", CultureInfo.InvariantCulture) +
                                 " h in " + date.Year.ToString(CultureInfo.InvariantCulture) +
                                 " month " + date.Day.ToString(CultureInfo.InvariantCulture);
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
    }
}
#endif

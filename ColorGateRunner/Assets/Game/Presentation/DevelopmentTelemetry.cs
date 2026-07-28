using System;
using System.Globalization;
using System.IO;
using System.Text;
using ColorGateRunner.Core;
using UnityEngine;

namespace ColorGateRunner.Presentation
{
    internal sealed class DevelopmentTelemetry
    {
        private readonly bool _enabled;
        private readonly StringBuilder _rows = new StringBuilder();
        private readonly string _path;

        internal DevelopmentTelemetry(bool enabled)
        {
#if UNITY_EDITOR || DEVELOPMENT_BUILD
            _enabled = enabled;
#else
            _enabled = false;
#endif
            _path = Path.Combine(
                Application.persistentDataPath,
                "ColorGateRunner-DevelopmentTelemetry.csv");
            if (_enabled)
            {
                _rows.AppendLine(
                    "event,stage,seed,shield,booster,continued,time,gate,pattern,plannedColor,requiredColor,colorOverride,playerColor,reactionMargin,outcome,shieldActive,boosterActive");
            }
        }

        internal bool Enabled => _enabled;

        internal void Record(
            string eventName,
            StageSession session,
            int gateIndex,
            GatePlan plan,
            string outcome,
            float reactionMargin)
        {
            if (!_enabled || session == null)
            {
                return;
            }

            _rows.Append(eventName).Append(',')
                .Append(session.Stage.StageId).Append(',')
                .Append(session.Stage.Seed).Append(',')
                .Append(session.Items.Shield).Append(',')
                .Append(session.Items.Booster).Append(',')
                .Append(session.ContinueUsed).Append(',')
                .Append(session.ElapsedPlayingSeconds.ToString(
                    "0.###",
                    CultureInfo.InvariantCulture)).Append(',')
                .Append(gateIndex).Append(',')
                .Append(plan.Pattern).Append(',')
                .Append(plan.PlannedColor).Append(',')
                .Append(plan.Color).Append(',')
                .Append(plan.HasTemporaryColorOverride).Append(',')
                .Append(session.CurrentColor).Append(',')
                .Append(reactionMargin.ToString(
                    "0.###",
                    CultureInfo.InvariantCulture)).Append(',')
                .Append(outcome).Append(',')
                .Append(session.ShieldActive).Append(',')
                .Append(session.BoosterActive).AppendLine();
        }

        internal void Flush()
        {
            if (_enabled)
            {
                File.WriteAllText(_path, _rows.ToString());
            }
        }
    }
}

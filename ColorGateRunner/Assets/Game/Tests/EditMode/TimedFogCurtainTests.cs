using System;
using ColorGateRunner.Core;
using ColorGateRunner.Presentation;
using NUnit.Framework;

namespace ColorGateRunner.Tests.EditMode
{
    public sealed class TimedFogCurtainTests
    {
        [Test]
        public void Trigger_FadesInHoldsFullOpacityThenFadesToZero()
        {
            var state = new TimedFogCurtainState();
            var settings = new FogCurtainSettings(0.5f, 5f, 0.5f);

            Assert.That(state.TryTrigger(settings), Is.True);
            Assert.That(state.Alpha, Is.Zero);

            state.Advance(0.25f);
            Assert.That(state.Alpha, Is.EqualTo(0.5f).Within(0.0001f));

            state.Advance(0.25f);
            Assert.That(state.Alpha, Is.EqualTo(1f));

            state.Advance(5f);
            Assert.That(state.Alpha, Is.EqualTo(1f));

            state.Advance(0.25f);
            Assert.That(state.Alpha, Is.EqualTo(0.5f).Within(0.0001f));

            state.Advance(0.25f);
            Assert.That(state.Alpha, Is.Zero);
            Assert.That(state.IsActive, Is.False);
        }

        [Test]
        public void Trigger_IsOneShotUntilAttemptReset()
        {
            var state = new TimedFogCurtainState();
            var settings = FogCurtainSettings.CreateDefault();
            state.TryTrigger(settings);
            state.Advance(state.TotalSeconds);

            Assert.That(state.TryTrigger(settings), Is.False);
            Assert.That(state.Alpha, Is.Zero);

            state.Reset();
            Assert.That(state.HasTriggered, Is.False);
            Assert.That(state.TryTrigger(settings), Is.True);
            Assert.That(state.Alpha, Is.Zero);
        }

        [Test]
        public void Advance_RejectsNegativeTime()
        {
            var state = new TimedFogCurtainState();
            state.TryTrigger(FogCurtainSettings.CreateDefault());

            Assert.Throws<ArgumentOutOfRangeException>(
                () => state.Advance(-0.01f));
        }

        [Test]
        public void Settings_RejectNonPositivePhaseDurations()
        {
            Assert.Throws<ArgumentOutOfRangeException>(
                () => new FogCurtainSettings(0f, 5f, 0.5f));
            Assert.Throws<ArgumentOutOfRangeException>(
                () => new FogCurtainSettings(0.5f, 0f, 0.5f));
            Assert.Throws<ArgumentOutOfRangeException>(
                () => new FogCurtainSettings(0.5f, 5f, 0f));
        }
    }
}

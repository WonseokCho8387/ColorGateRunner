using System;
using ColorGateRunner.Presentation;
using NUnit.Framework;

namespace ColorGateRunner.Tests.EditMode
{
    public sealed class TimedFogCurtainTests
    {
        [Test]
        public void Trigger_HoldsFullOpacityThenFadesToZero()
        {
            var state = new TimedFogCurtainState();

            Assert.That(state.TryTrigger(), Is.True);
            Assert.That(state.Alpha, Is.EqualTo(1f));

            state.Advance(1.5f);
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
            state.TryTrigger();
            state.Advance(TimedFogCurtainState.TotalSeconds);

            Assert.That(state.TryTrigger(), Is.False);
            Assert.That(state.Alpha, Is.Zero);

            state.Reset();
            Assert.That(state.HasTriggered, Is.False);
            Assert.That(state.TryTrigger(), Is.True);
            Assert.That(state.Alpha, Is.EqualTo(1f));
        }

        [Test]
        public void Advance_RejectsNegativeTime()
        {
            var state = new TimedFogCurtainState();
            state.TryTrigger();

            Assert.Throws<ArgumentOutOfRangeException>(
                () => state.Advance(-0.01f));
        }
    }
}

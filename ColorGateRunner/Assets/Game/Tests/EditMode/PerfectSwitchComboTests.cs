using ColorGateRunner.Core;
using NUnit.Framework;

namespace ColorGateRunner.Tests.EditMode
{
    public sealed class PerfectSwitchComboTests
    {
        private const float Window = PerfectSwitchCombo.PerfectWindowSeconds;

        [Test]
        public void SwitchInsideWindow_CountsPerfectAndTracksMax()
        {
            var combo = new PerfectSwitchCombo();
            Assert.That(combo.Record(GateOutcome.Matched, Switch(0.1f)),
                Is.EqualTo(ComboStep.Perfect));
            Assert.That(combo.Record(GateOutcome.Matched, Switch(Window)),
                Is.EqualTo(ComboStep.Perfect));
            Assert.That(combo.Current, Is.EqualTo(2));
            Assert.That(combo.Max, Is.EqualTo(2));
            Assert.That(combo.PerfectCount, Is.EqualTo(2));
        }

        [Test]
        public void EarlySwitch_BreaksComboButKeepsMax()
        {
            var combo = new PerfectSwitchCombo();
            combo.Record(GateOutcome.Matched, Switch(0f));
            combo.Record(GateOutcome.Matched, Switch(0f));
            Assert.That(combo.Record(GateOutcome.Matched, Switch(Window + 0.01f)),
                Is.EqualTo(ComboStep.Broken));
            Assert.That(combo.Current, Is.Zero);
            Assert.That(combo.Max, Is.EqualTo(2));
            Assert.That(combo.PerfectCount, Is.EqualTo(2));
        }

        [Test]
        public void NoSwitchEchoAndBooster_HoldCombo()
        {
            var combo = new PerfectSwitchCombo();
            combo.Record(GateOutcome.Matched, Switch(0f));
            Assert.That(combo.Record(GateOutcome.Matched, default),
                Is.EqualTo(ComboStep.Held));
            Assert.That(combo.Record(GateOutcome.Echoed, Switch(1f)),
                Is.EqualTo(ComboStep.Held));
            Assert.That(combo.Record(GateOutcome.Boosted, Switch(1f)),
                Is.EqualTo(ComboStep.Held));
            Assert.That(combo.Record(GateOutcome.Ignored, Switch(0f)),
                Is.EqualTo(ComboStep.Ignored));
            Assert.That(combo.Current, Is.EqualTo(1));
        }

        [TestCase(GateOutcome.Shielded)]
        [TestCase(GateOutcome.Invulnerable)]
        [TestCase(GateOutcome.Mismatched)]
        public void ShieldContinueAndFailure_BreakCombo(GateOutcome outcome)
        {
            var combo = new PerfectSwitchCombo();
            combo.Record(GateOutcome.Matched, Switch(0f));
            Assert.That(combo.Record(outcome, Switch(0f)),
                Is.EqualTo(ComboStep.Broken));
            Assert.That(combo.Current, Is.Zero);
            Assert.That(combo.Max, Is.EqualTo(1));
        }

        [Test]
        public void Reset_ClearsAllCounts()
        {
            var combo = new PerfectSwitchCombo();
            combo.Record(GateOutcome.Matched, Switch(0f));
            combo.Reset();
            Assert.That(combo.Current, Is.Zero);
            Assert.That(combo.Max, Is.Zero);
            Assert.That(combo.PerfectCount, Is.Zero);
        }

        [Test]
        public void Session_MeasuresLastSwitchBeforeGate()
        {
            StageSession session = StartPlaying(5);
            session.TryToggleColor();
            session.Advance(0.5f, 1f);
            session.TryToggleColor();
            session.Advance(0.2f, 1f);
            Assert.That(session.ResolveGate(session.CurrentColor),
                Is.EqualTo(GateOutcome.Matched));
            Assert.That(session.LastGateSwitchTiming.Switched, Is.True);
            Assert.That(session.LastGateSwitchTiming.SecondsBeforeGate,
                Is.EqualTo(0.2f).Within(0.0001f));

            session.Advance(0.4f, 1f);
            session.ResolveGate(session.CurrentColor);
            Assert.That(session.LastGateSwitchTiming.Switched, Is.False);
        }

        [Test]
        public void Session_RetryClearsSwitchTiming()
        {
            StageSession session = StartPlaying(5);
            session.TryToggleColor();
            session.ResolveGate(session.CurrentColor);
            Assert.That(session.LastGateSwitchTiming.Switched, Is.True);
            session.RetryToSelection();
            Assert.That(session.LastGateSwitchTiming.Switched, Is.False);
        }

        private static GateSwitchTiming Switch(float secondsBeforeGate) =>
            new GateSwitchTiming(true, secondsBeforeGate);

        private static StageSession StartPlaying(int stageNumber)
        {
            var session = new StageSession(StageCatalog.GetByDisplayNumber(stageNumber));
            Assert.That(session.BeginCountdown(), Is.True);
            Assert.That(session.CompleteCountdown(), Is.True);
            Assert.That(session.FlowState, Is.EqualTo(StageFlowState.Playing));
            return session;
        }
    }
}

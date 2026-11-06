using ColorGateRunner.Core;
using NUnit.Framework;

namespace ColorGateRunner.Tests.EditMode
{
    public sealed class PerfectSwitchComboTests
    {
        private const float Window = PerfectSwitchCombo.PerfectWindowSeconds;
        private const float GreatWindow = PerfectSwitchCombo.GreatWindowSeconds;

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
        public void Grade_UsesWindowBoundariesAndTreatsHoldingAsPerfect()
        {
            Assert.That(PerfectSwitchCombo.Grade(default), Is.EqualTo(ComboStep.Perfect));
            Assert.That(PerfectSwitchCombo.Grade(Switch(Window)),
                Is.EqualTo(ComboStep.Perfect));
            Assert.That(PerfectSwitchCombo.Grade(Switch(Window + 0.01f)),
                Is.EqualTo(ComboStep.Great));
            Assert.That(PerfectSwitchCombo.Grade(Switch(GreatWindow)),
                Is.EqualTo(ComboStep.Great));
            Assert.That(PerfectSwitchCombo.Grade(Switch(GreatWindow + 0.01f)),
                Is.EqualTo(ComboStep.Good));
        }

        [Test]
        public void PerfectAndGreat_ContinueCombo()
        {
            var combo = new PerfectSwitchCombo();
            combo.Record(GateOutcome.Matched, Switch(0f));
            Assert.That(combo.Record(GateOutcome.Matched, Switch(GreatWindow)),
                Is.EqualTo(ComboStep.Great));
            Assert.That(combo.Record(GateOutcome.Matched, default),
                Is.EqualTo(ComboStep.Perfect));
            Assert.That(combo.Current, Is.EqualTo(3));
            Assert.That(combo.Max, Is.EqualTo(3));
            Assert.That(combo.PerfectCount, Is.EqualTo(2));
            Assert.That(combo.GreatCount, Is.EqualTo(1));
            Assert.That(combo.ComboHitCount, Is.EqualTo(3));
            Assert.That(combo.BreakCount, Is.Zero);
        }

        [Test]
        public void Good_BreaksComboButKeepsMax()
        {
            var combo = new PerfectSwitchCombo();
            combo.Record(GateOutcome.Matched, Switch(0f));
            combo.Record(GateOutcome.Matched, Switch(0f));
            Assert.That(combo.Record(GateOutcome.Matched, Switch(GreatWindow + 0.01f)),
                Is.EqualTo(ComboStep.Good));
            Assert.That(combo.Current, Is.Zero);
            Assert.That(combo.Max, Is.EqualTo(2));
            Assert.That(combo.BreakCount, Is.EqualTo(1));
        }

        [TestCase(GateOutcome.Shielded)]
        [TestCase(GateOutcome.Invulnerable)]
        [TestCase(GateOutcome.Mismatched)]
        public void GatesWithoutGrade_BreakCombo(GateOutcome outcome)
        {
            var combo = new PerfectSwitchCombo();
            combo.Record(GateOutcome.Matched, Switch(0f));
            Assert.That(combo.Record(outcome, Switch(0f)),
                Is.EqualTo(ComboStep.Broken));
            Assert.That(combo.Current, Is.Zero);
            Assert.That(combo.Max, Is.EqualTo(1));
            Assert.That(combo.BreakCount, Is.EqualTo(1));
        }

        [TestCase(GateOutcome.Boosted)]
        [TestCase(GateOutcome.Echoed)]
        public void BoosterAndEchoPasses_ArePerfectAndContinueCombo(
            GateOutcome outcome)
        {
            var combo = new PerfectSwitchCombo();
            combo.Record(GateOutcome.Matched, Switch(0f));
            // An early switch would be Good on a matched gate; it never
            // downgrades a Booster or Echo pass.
            Assert.That(combo.Record(outcome, Switch(GreatWindow + 1f)),
                Is.EqualTo(ComboStep.Perfect));
            Assert.That(combo.Current, Is.EqualTo(2));
            Assert.That(combo.PerfectCount, Is.EqualTo(2));
            Assert.That(combo.BreakCount, Is.Zero);
        }

        [Test]
        public void Ignored_LeavesComboUntouched()
        {
            var combo = new PerfectSwitchCombo();
            combo.Record(GateOutcome.Matched, Switch(0f));
            Assert.That(combo.Record(GateOutcome.Ignored, Switch(1f)),
                Is.EqualTo(ComboStep.Ignored));
            Assert.That(combo.Current, Is.EqualTo(1));
            Assert.That(combo.BreakCount, Is.Zero);
        }

        [Test]
        public void Reset_ClearsAllCounts()
        {
            var combo = new PerfectSwitchCombo();
            combo.Record(GateOutcome.Matched, Switch(GreatWindow));
            combo.Record(GateOutcome.Mismatched, default);
            combo.Reset();
            Assert.That(combo.Current, Is.Zero);
            Assert.That(combo.Max, Is.Zero);
            Assert.That(combo.PerfectCount, Is.Zero);
            Assert.That(combo.GreatCount, Is.Zero);
            Assert.That(combo.BreakCount, Is.Zero);
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
            var session = new StageSession(AuthoredStage.Get(stageNumber));
            Assert.That(session.BeginCountdown(), Is.True);
            Assert.That(session.CompleteCountdown(), Is.True);
            Assert.That(session.FlowState, Is.EqualTo(StageFlowState.Playing));
            return session;
        }
    }
}

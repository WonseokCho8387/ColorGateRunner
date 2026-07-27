using System;
using ColorGateRunner.Core;
using NUnit.Framework;

namespace ColorGateRunner.Tests.EditMode
{
    public sealed class GameSessionTests
    {
        private const uint TestSeed = GameRules.DefaultSeed;

        [Test]
        public void NewSession_IsReady()
        {
            GameSession session = CreateSession();

            Assert.That(session.CurrentState, Is.EqualTo(RunState.Ready));
        }

        [Test]
        public void NewSession_ColorIsRed()
        {
            GameSession session = CreateSession();

            Assert.That(session.CurrentColor, Is.EqualTo(RunnerColor.Red));
        }

        [Test]
        public void NewSession_ScoreIsZero()
        {
            GameSession session = CreateSession();

            Assert.That(session.CurrentScore, Is.Zero);
        }

        [Test]
        public void NewSession_UsesUpdatedBaseSpeed()
        {
            GameSession session = CreateSession();

            Assert.That(session.CurrentSpeed, Is.EqualTo(6f));
        }

        [Test]
        public void Ready_StartsPlaying()
        {
            GameSession session = CreateSession();

            bool started = session.StartRun();

            Assert.That(started, Is.True);
            Assert.That(session.CurrentState, Is.EqualTo(RunState.Playing));
        }

        [Test]
        public void StartRun_DoesNotToggleInitialColor()
        {
            GameSession session = CreateSession();

            session.StartRun();

            Assert.That(session.CurrentColor, Is.EqualTo(RunnerColor.Red));
        }

        [Test]
        public void Red_TogglesToBlue()
        {
            GameSession session = CreatePlayingSession();

            bool toggled = session.TryToggleColor();

            Assert.That(toggled, Is.True);
            Assert.That(session.CurrentColor, Is.EqualTo(RunnerColor.Blue));
        }

        [Test]
        public void Blue_TogglesToRed()
        {
            GameSession session = CreatePlayingSession();
            session.TryToggleColor();

            bool toggled = session.TryToggleColor();

            Assert.That(toggled, Is.True);
            Assert.That(session.CurrentColor, Is.EqualTo(RunnerColor.Red));
        }

        [Test]
        public void Toggle_WhenReady_IsIgnored()
        {
            GameSession session = CreateSession();

            bool toggled = session.TryToggleColor();

            Assert.That(toggled, Is.False);
            Assert.That(session.CurrentColor, Is.EqualTo(RunnerColor.Red));
            Assert.That(session.CurrentState, Is.EqualTo(RunState.Ready));
        }

        [Test]
        public void Toggle_WhenDead_IsIgnored()
        {
            GameSession session = CreateDeadSession();
            RunnerColor colorAtDeath = session.CurrentColor;

            bool toggled = session.TryToggleColor();

            Assert.That(toggled, Is.False);
            Assert.That(session.CurrentColor, Is.EqualTo(colorAtDeath));
            Assert.That(session.CurrentState, Is.EqualTo(RunState.Dead));
        }

        [Test]
        public void MatchingGate_AddsExactlyOne()
        {
            GameSession session = CreatePlayingSession();

            GateOutcome outcome = session.ResolveGate(session.CurrentColor);

            Assert.That(outcome, Is.EqualTo(GateOutcome.Matched));
            Assert.That(session.CurrentScore, Is.EqualTo(1));
        }

        [Test]
        public void MatchingGate_KeepsSessionPlaying()
        {
            GameSession session = CreatePlayingSession();

            session.ResolveGate(session.CurrentColor);

            Assert.That(session.CurrentState, Is.EqualTo(RunState.Playing));
        }

        [Test]
        public void MismatchingGate_AddsNothing()
        {
            GameSession session = CreatePlayingSession();

            GateOutcome outcome = session.ResolveGate(OppositeOf(session.CurrentColor));

            Assert.That(outcome, Is.EqualTo(GateOutcome.Mismatched));
            Assert.That(session.CurrentScore, Is.Zero);
        }

        [Test]
        public void MismatchingGate_ChangesStateToDead()
        {
            GameSession session = CreatePlayingSession();

            session.ResolveGate(OppositeOf(session.CurrentColor));

            Assert.That(session.CurrentState, Is.EqualTo(RunState.Dead));
        }

        [Test]
        public void GateResolution_WhenNotPlaying_IsIgnored()
        {
            GameSession session = CreateSession();

            GateOutcome readyOutcome = session.ResolveGate(session.CurrentColor);
            session.StartRun();
            session.ResolveGate(OppositeOf(session.CurrentColor));
            GateOutcome deadOutcome = session.ResolveGate(session.CurrentColor);

            Assert.That(readyOutcome, Is.EqualTo(GateOutcome.Ignored));
            Assert.That(deadOutcome, Is.EqualTo(GateOutcome.Ignored));
            Assert.That(session.CurrentScore, Is.Zero);
            Assert.That(session.CurrentState, Is.EqualTo(RunState.Dead));
        }

        [TestCase(1, 6.01f)]
        [TestCase(5, 6.05f)]
        [TestCase(10, 6.1f)]
        [TestCase(25, 6.25f)]
        public void Speed_IncreasesWithScore(int score, float expectedSpeed)
        {
            GameSession session = CreatePlayingSession();

            ResolveMatchingGates(session, score);

            Assert.That(session.CurrentSpeed, Is.EqualTo(expectedSpeed).Within(0.0001f));
        }

        [Test]
        public void Speed_NeverExceedsFifteen()
        {
            GameSession session = CreatePlayingSession();

            ResolveMatchingGates(session, 500);
            session.Advance(1000f);

            Assert.That(session.CurrentSpeed, Is.EqualTo(GameRules.MaximumSpeed));
        }

        [Test]
        public void Advance_WhilePlaying_IncreasesElapsedTime()
        {
            GameSession session = CreatePlayingSession();

            session.Advance(1.25f);

            Assert.That(session.ElapsedPlayingSeconds, Is.EqualTo(1.25f));
        }

        [Test]
        public void Advance_WhileReady_DoesNotIncreaseElapsedTime()
        {
            GameSession session = CreateSession();

            session.Advance(1f);

            Assert.That(session.ElapsedPlayingSeconds, Is.Zero);
        }

        [Test]
        public void Advance_WhileDead_DoesNotIncreaseElapsedTime()
        {
            GameSession session = CreateDeadSession();

            session.Advance(1f);

            Assert.That(session.ElapsedPlayingSeconds, Is.Zero);
        }

        [Test]
        public void Advance_NegativeDelta_IsRejected()
        {
            GameSession session = CreatePlayingSession();

            Assert.Throws<ArgumentOutOfRangeException>(() => session.Advance(-0.01f));
        }

        [Test]
        public void Speed_IncreasesWithElapsedTime()
        {
            GameSession session = CreatePlayingSession();

            session.Advance(10f);

            Assert.That(session.CurrentSpeed, Is.EqualTo(10.5f).Within(0.001f));
        }

        [TestCase(0f, 6f)]
        [TestCase(5f, 9f)]
        [TestCase(10f, 10.5f)]
        [TestCase(20f, 12.5f)]
        [TestCase(30f, 14f)]
        [TestCase(45f, 15f)]
        public void NonlinearSpeed_HasMeasuredProgression(
            float elapsedSeconds,
            float expectedSpeed)
        {
            Assert.That(
                GameRules.CalculateSpeed(0, elapsedSeconds),
                Is.EqualTo(expectedSpeed).Within(0.0001f));
        }

        [Test]
        public void NonlinearSpeed_EarlyAccelerationExceedsLateAcceleration()
        {
            float atZero = GameRules.CalculateSpeed(0, 0f);
            float atTen = GameRules.CalculateSpeed(0, 10f);
            float atTwenty = GameRules.CalculateSpeed(0, 20f);
            float atThirty = GameRules.CalculateSpeed(0, 30f);

            Assert.That(atTen - atZero, Is.GreaterThan(atThirty - atTwenty));
            Assert.That(atThirty, Is.GreaterThan(atTen));
        }

        [Test]
        public void MovementSpeed_ContinuesIncreasingAfterTenSeconds()
        {
            float atTen = GameRules.CalculateSpeed(0, 10f);
            float atTwenty = GameRules.CalculateSpeed(0, 20f);
            float atThirty = GameRules.CalculateSpeed(0, 30f);

            Assert.That(atTwenty, Is.GreaterThan(atTen));
            Assert.That(atThirty - atTwenty, Is.GreaterThanOrEqualTo(1.5f));
        }

        [Test]
        public void MaximumSpeed_IsReachedOnlyAtLateStage()
        {
            Assert.That(
                GameRules.CalculateSpeed(0, 30f),
                Is.LessThan(GameRules.MaximumSpeed));
            Assert.That(
                GameRules.CalculateSpeed(0, 44f),
                Is.LessThan(GameRules.MaximumSpeed));
            Assert.That(
                GameRules.CalculateSpeed(0, 45f),
                Is.EqualTo(GameRules.MaximumSpeed));
        }

        [Test]
        public void EncounterInterval_DecreasesAcrossProgression()
        {
            float early = GameRules.CalculateTargetEncounterInterval(0f);
            float middle = GameRules.CalculateTargetEncounterInterval(20f);
            float late = GameRules.CalculateTargetEncounterInterval(45f);

            Assert.That(middle, Is.LessThan(early));
            Assert.That(late, Is.LessThan(middle));
            Assert.That(early - late, Is.GreaterThan(0.7f));
        }

        [Test]
        public void Shield_AutomaticScoreGrantIsRemoved()
        {
            GameSession session = CreatePlayingSession();

            ResolveMatchingGates(session, 10);

            Assert.That(session.ShieldActive, Is.False);
            Assert.That(session.ShieldPickupCollected, Is.False);
        }

        [Test]
        public void ShieldPickup_ActivatesAndProtectsExactlyOneMismatch()
        {
            GameSession session = CreatePlayingSession();
            Assert.That(session.CollectShieldPickup(), Is.True);
            Assert.That(session.ShieldActive, Is.True);
            int score = session.CurrentScore;

            Assert.That(
                session.ResolveGate(OppositeOf(session.CurrentColor)),
                Is.EqualTo(GateOutcome.Shielded));
            Assert.That(session.CurrentState, Is.EqualTo(RunState.ShieldRecovery));
            Assert.That(session.CurrentScore, Is.EqualTo(score));
            Assert.That(session.ShieldActive, Is.False);

            Assert.That(
                session.ResolveGate(OppositeOf(session.CurrentColor)),
                Is.EqualTo(GateOutcome.Invulnerable));
            session.Advance(GameRules.ShieldRecoveryDuration);
            Assert.That(
                session.ResolveGate(OppositeOf(session.CurrentColor)),
                Is.EqualTo(GateOutcome.Mismatched));
            Assert.That(session.CurrentState, Is.EqualTo(RunState.Dead));
        }

        [Test]
        public void Restart_ResetsShield()
        {
            GameSession session = CreatePlayingSession();
            session.CollectShieldPickup();
            session.Restart();
            Assert.That(session.ShieldActive, Is.False);
        }

        [Test]
        public void Shield_MatchingWhileActiveDoesNotCreateASecondCharge()
        {
            GameSession session = CreatePlayingSession();
            session.CollectShieldPickup();
            ResolveMatchingGates(session, 5);

            Assert.That(session.ShieldActive, Is.True);
            Assert.That(session.CollectShieldPickup(), Is.False);
            Assert.That(
                session.ResolveGate(OppositeOf(session.CurrentColor)),
                Is.EqualTo(GateOutcome.Shielded));
            Assert.That(
                session.ResolveGate(OppositeOf(session.CurrentColor)),
                Is.EqualTo(GateOutcome.Invulnerable));
            session.Advance(GameRules.ShieldRecoveryDuration);
            Assert.That(
                session.ResolveGate(OppositeOf(session.CurrentColor)),
                Is.EqualTo(GateOutcome.Mismatched));
        }

        [Test]
        public void ShieldBreak_EntersRecoveryAndReducesSpeed()
        {
            GameSession session = CreatePlayingSession();
            session.CollectShieldPickup();
            session.Advance(5f);
            float beforeBreak = session.CurrentSpeed;

            Assert.That(
                session.ResolveGate(OppositeOf(session.CurrentColor)),
                Is.EqualTo(GateOutcome.Shielded));
            Assert.That(session.CurrentState, Is.EqualTo(RunState.ShieldRecovery));
            Assert.That(session.IsInvulnerable, Is.True);
            Assert.That(
                session.CurrentSpeed,
                Is.EqualTo(beforeBreak * 0.7f).Within(0.001f));
        }

        [Test]
        public void ShieldRecovery_MismatchIsIgnoredWithoutScore()
        {
            GameSession session = CreatePlayingSession();
            session.CollectShieldPickup();
            session.ResolveGate(OppositeOf(session.CurrentColor));
            int score = session.CurrentScore;

            Assert.That(
                session.ResolveGate(OppositeOf(session.CurrentColor)),
                Is.EqualTo(GateOutcome.Invulnerable));
            Assert.That(session.CurrentScore, Is.EqualTo(score));
            Assert.That(session.CurrentState, Is.EqualTo(RunState.ShieldRecovery));
        }

        [Test]
        public void ShieldRecovery_ExpiresDeterministicallyAndSpeedRecovers()
        {
            GameSession session = CreatePlayingSession();
            session.CollectShieldPickup();
            session.ResolveGate(OppositeOf(session.CurrentColor));
            float reduced = session.CurrentSpeed;

            session.Advance(GameRules.ShieldRecoveryDuration * 0.5f);
            float midway = session.CurrentSpeed;
            Assert.That(session.CurrentState, Is.EqualTo(RunState.ShieldRecovery));
            Assert.That(midway, Is.GreaterThan(reduced));

            session.Advance(GameRules.ShieldRecoveryDuration * 0.5f);
            Assert.That(session.CurrentState, Is.EqualTo(RunState.Playing));
            Assert.That(session.IsInvulnerable, Is.False);
            Assert.That(session.CurrentSpeed, Is.GreaterThan(midway));
        }

        [Test]
        public void Restart_ClearsShieldRecovery()
        {
            GameSession session = CreatePlayingSession();
            session.CollectShieldPickup();
            session.ResolveGate(OppositeOf(session.CurrentColor));

            session.Restart();

            Assert.That(session.CurrentState, Is.EqualTo(RunState.Ready));
            Assert.That(session.ShieldRecoveryRemaining, Is.Zero);
            Assert.That(session.IsInvulnerable, Is.False);
        }

        [Test]
        public void Countdown_DoesNotAdvanceTimeAndCompletesOnce()
        {
            GameSession session = CreateSession();

            Assert.That(session.BeginCountdown(), Is.True);
            session.Advance(GameRules.DefaultCountdownDuration);
            Assert.That(session.ElapsedPlayingSeconds, Is.Zero);
            Assert.That(session.TryToggleColor(), Is.False);
            Assert.That(session.CompleteCountdown(), Is.True);
            Assert.That(session.CompleteCountdown(), Is.False);
            Assert.That(session.CurrentState, Is.EqualTo(RunState.Playing));
        }

        [Test]
        public void ThirdColor_IsInactiveBeforeMilestone()
        {
            GameSession session = CreatePlayingSession();
            ResolveMatchingGates(
                session,
                GameRules.ThirdColorScoreMilestone - 1);

            Assert.That(session.ActiveColorCount, Is.EqualTo(2));
            Assert.That(session.ThirdColorIntroduced, Is.False);
            for (int index = 0; index < 40; index++)
            {
                Assert.That(
                    session.GetNextGatePlan().Color,
                    Is.Not.EqualTo(RunnerColor.Green));
            }
        }

        [Test]
        public void ThirdColor_IntroducesExactlyOnceAtMilestone()
        {
            GameSession session = CreatePlayingSession();

            ResolveMatchingGates(
                session,
                GameRules.ThirdColorScoreMilestone);

            Assert.That(session.ActiveColorCount, Is.EqualTo(3));
            Assert.That(session.ThirdColorIntroduced, Is.True);
            Assert.That(
                session.ThirdColorTutorialGatesRemaining,
                Is.EqualTo(GameRules.ThirdColorTutorialGateCount));
            ResolveMatchingGates(session, 5);
            Assert.That(session.ActiveColorCount, Is.EqualTo(3));
        }

        [Test]
        public void ThirdColor_TutorialGatesFollowIntroduction()
        {
            GameSession session = CreatePlayingSession();
            ResolveMatchingGates(
                session,
                GameRules.ThirdColorScoreMilestone);

            GatePlan first = session.GetNextGatePlan();
            GatePlan second = session.GetNextGatePlan();

            Assert.That(
                first.Pattern,
                Is.EqualTo(GatePatternType.ThirdColorTutorial));
            Assert.That(first.Color, Is.EqualTo(RunnerColor.Green));
            Assert.That(
                second.Pattern,
                Is.EqualTo(GatePatternType.ThirdColorTutorial));
            Assert.That(second.Color, Is.EqualTo(RunnerColor.Red));
            Assert.That(
                first.TimeToGate,
                Is.GreaterThanOrEqualTo(1.35f));
        }

        [Test]
        public void ThreeColorCycle_OrderIsStable()
        {
            GameSession session = CreatePlayingSession();
            ResolveMatchingGates(
                session,
                GameRules.ThirdColorScoreMilestone);
            Assert.That(session.CurrentColor, Is.EqualTo(RunnerColor.Red));

            session.TryToggleColor();
            Assert.That(session.CurrentColor, Is.EqualTo(RunnerColor.Blue));
            session.TryToggleColor();
            Assert.That(session.CurrentColor, Is.EqualTo(RunnerColor.Green));
            session.TryToggleColor();
            Assert.That(session.CurrentColor, Is.EqualTo(RunnerColor.Red));
        }

        [Test]
        public void Restart_ResetsThirdColorIntroduction()
        {
            GameSession session = CreatePlayingSession();
            ResolveMatchingGates(
                session,
                GameRules.ThirdColorScoreMilestone);

            session.Restart();

            Assert.That(session.ActiveColorCount, Is.EqualTo(2));
            Assert.That(session.ThirdColorIntroduced, Is.False);
            Assert.That(session.ThirdColorTutorialGatesRemaining, Is.Zero);
        }

        [Test]
        public void SpeedPresentation_StagesIncreaseVisualOutputs()
        {
            SpeedPresentation start =
                GameRules.GetSpeedPresentation(0f, GameRules.CalculateSpeed(0, 0f));
            SpeedPresentation accelerating =
                GameRules.GetSpeedPresentation(6f, GameRules.CalculateSpeed(0, 6f));
            SpeedPresentation fast =
                GameRules.GetSpeedPresentation(15f, GameRules.CalculateSpeed(0, 15f));
            SpeedPresentation maximum =
                GameRules.GetSpeedPresentation(30f, GameRules.CalculateSpeed(0, 30f));

            Assert.That(accelerating.CameraFieldOfView, Is.GreaterThan(start.CameraFieldOfView));
            Assert.That(fast.TrailIntensity, Is.GreaterThan(accelerating.TrailIntensity));
            Assert.That(maximum.SpeedLineRate, Is.GreaterThan(fast.SpeedLineRate));
            Assert.That(maximum.MarkerIntensity, Is.GreaterThan(fast.MarkerIntensity));
        }

        [Test]
        public void SameAdvanceSequence_ProducesSameSpeed()
        {
            GameSession first = CreatePlayingSession();
            GameSession second = CreatePlayingSession();
            float[] deltas = { 0.016f, 0.02f, 0.033f, 0.5f, 1.25f };

            for (int index = 0; index < deltas.Length; index++)
            {
                first.Advance(deltas[index]);
                second.Advance(deltas[index]);
            }

            Assert.That(second.ElapsedPlayingSeconds, Is.EqualTo(first.ElapsedPlayingSeconds));
            Assert.That(second.CurrentSpeed, Is.EqualTo(first.CurrentSpeed));
        }

        [Test]
        public void Restart_ReturnsReady()
        {
            GameSession session = CreateDeadSession();

            session.Restart();

            Assert.That(session.CurrentState, Is.EqualTo(RunState.Ready));
        }

        [Test]
        public void Restart_ResetsColorToRed()
        {
            GameSession session = CreatePlayingSession();
            session.TryToggleColor();

            session.Restart();

            Assert.That(session.CurrentColor, Is.EqualTo(RunnerColor.Red));
        }

        [Test]
        public void Restart_ResetsScore()
        {
            GameSession session = CreatePlayingSession();
            ResolveMatchingGates(session, 3);

            session.Restart();

            Assert.That(session.CurrentScore, Is.Zero);
        }

        [Test]
        public void Restart_ResetsSpeed()
        {
            GameSession session = CreatePlayingSession();
            ResolveMatchingGates(session, 10);

            session.Restart();

            Assert.That(session.CurrentSpeed, Is.EqualTo(GameRules.InitialSpeed));
        }

        [Test]
        public void Restart_ResetsElapsedTime()
        {
            GameSession session = CreatePlayingSession();
            session.Advance(5f);

            session.Restart();

            Assert.That(session.ElapsedPlayingSeconds, Is.Zero);
        }

        [Test]
        public void Restart_ReplaysSequence()
        {
            const int sequenceLength = 64;
            GameSession session = CreateSession();
            RunnerColor[] firstSequence = ReadSequence(session, sequenceLength);

            session.Restart();
            RunnerColor[] restartedSequence = ReadSequence(session, sequenceLength);

            Assert.That(restartedSequence, Is.EqualTo(firstSequence));
        }

        [Test]
        public void Restart_ReplaysColorAndSpacingSequence()
        {
            const int sequenceLength = 32;
            GameSession session = CreateSession();
            RunnerColor[] firstColors = new RunnerColor[sequenceLength];
            float[] firstSpacings = new float[sequenceLength];

            for (int index = 0; index < sequenceLength; index++)
            {
                firstColors[index] = session.GetNextGateColor();
                firstSpacings[index] = session.GetNextGateSpacing();
            }

            session.Restart();

            for (int index = 0; index < sequenceLength; index++)
            {
                Assert.That(session.GetNextGateColor(), Is.EqualTo(firstColors[index]));
                Assert.That(session.GetNextGateSpacing(), Is.EqualTo(firstSpacings[index]));
            }
        }

        private static GameSession CreateSession()
        {
            return new GameSession(TestSeed);
        }

        private static GameSession CreatePlayingSession()
        {
            GameSession session = CreateSession();
            session.StartRun();
            return session;
        }

        private static GameSession CreateDeadSession()
        {
            GameSession session = CreatePlayingSession();
            session.ResolveGate(OppositeOf(session.CurrentColor));
            return session;
        }

        private static void ResolveMatchingGates(GameSession session, int count)
        {
            for (int index = 0; index < count; index++)
            {
                session.ResolveGate(session.CurrentColor);
            }
        }

        private static RunnerColor[] ReadSequence(GameSession session, int count)
        {
            RunnerColor[] colors = new RunnerColor[count];
            for (int index = 0; index < count; index++)
            {
                colors[index] = session.GetNextGateColor();
            }

            return colors;
        }

        private static RunnerColor OppositeOf(RunnerColor color)
        {
            return color == RunnerColor.Red
                ? RunnerColor.Blue
                : RunnerColor.Red;
        }
    }
}

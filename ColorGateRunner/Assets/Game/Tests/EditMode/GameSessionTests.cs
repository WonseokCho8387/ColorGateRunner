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

        [TestCase(1, 6.12f)]
        [TestCase(5, 6.6f)]
        [TestCase(10, 7.2f)]
        [TestCase(25, 9f)]
        public void Speed_IncreasesWithScore(int score, float expectedSpeed)
        {
            GameSession session = CreatePlayingSession();

            ResolveMatchingGates(session, score);

            Assert.That(session.CurrentSpeed, Is.EqualTo(expectedSpeed).Within(0.0001f));
        }

        [Test]
        public void Speed_NeverExceedsTwelve()
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

            Assert.That(session.CurrentSpeed, Is.EqualTo(6.8f).Within(0.0001f));
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

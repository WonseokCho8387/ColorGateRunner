using System.Collections.Generic;
using ColorGateRunner.Core;
using NUnit.Framework;

namespace ColorGateRunner.Tests.EditMode
{
    public sealed class CampaignHiddenFlickerStageTests
    {
        [Test]
        public void Catalog_ContainsApprovedHiddenAndFlickerStages()
        {
            StageDefinition hidden = StageCatalog.GetByDisplayNumber(12);
            Assert.That(hidden.StageId, Is.EqualTo("stage-12"));
            Assert.That(hidden.Title, Is.EqualTo("HIDDEN MEMORY"));
            Assert.That(hidden.TargetGateCount, Is.EqualTo(50));
            Assert.That(hidden.AllowedColorCount, Is.EqualTo(3));
            Assert.That(hidden.StartingSpeed, Is.EqualTo(46f));
            Assert.That(hidden.MaximumSpeed, Is.EqualTo(68f));
            Assert.That(hidden.CadenceStart, Is.EqualTo(1.05f));
            Assert.That(hidden.CadenceEnd, Is.EqualTo(0.78f));
            Assert.That(hidden.PrimaryMechanic,
                Is.EqualTo(StagePrimaryMechanic.Hidden));
            Assert.That(hidden.HiddenSettings.Enabled, Is.True);
            Assert.That(hidden.HiddenSettings.HideLeadTimeSeconds,
                Is.EqualTo(0.85f));
            Assert.That(hidden.FlickerSettings.Enabled, Is.False);
            Assert.That(hidden.ShieldAllowed, Is.True);
            Assert.That(hidden.BoosterAllowed, Is.True);

            StageDefinition flicker = StageCatalog.GetByDisplayNumber(13);
            Assert.That(flicker.StageId, Is.EqualTo("stage-13"));
            Assert.That(flicker.Title, Is.EqualTo("FLICKER FLOW"));
            Assert.That(flicker.TargetGateCount, Is.EqualTo(52));
            Assert.That(flicker.AllowedColorCount, Is.EqualTo(3));
            Assert.That(flicker.StartingSpeed, Is.EqualTo(46f));
            Assert.That(flicker.MaximumSpeed, Is.EqualTo(68f));
            Assert.That(flicker.CadenceStart, Is.EqualTo(1.08f));
            Assert.That(flicker.CadenceEnd, Is.EqualTo(0.80f));
            Assert.That(flicker.PrimaryMechanic,
                Is.EqualTo(StagePrimaryMechanic.Flicker));
            Assert.That(flicker.HiddenSettings.Enabled, Is.False);
            Assert.That(flicker.FlickerSettings.Enabled, Is.True);
            Assert.That(flicker.FlickerSettings.SwitchIntervalSeconds,
                Is.EqualTo(0.50f));
            Assert.That(flicker.ShieldAllowed, Is.True);
            Assert.That(flicker.BoosterAllowed, Is.True);
        }

        [Test]
        public void Plans_AreDeterministicIsolatedAndDoNotChangeGateCount()
        {
            AssertDeterministicModifierStage(
                StageCatalog.GetByDisplayNumber(12),
                GateModifierType.Hidden);
            AssertDeterministicModifierStage(
                StageCatalog.GetByDisplayNumber(13),
                GateModifierType.Flicker);
        }

        [Test]
        public void FlickerPlans_UsePlayerOrderAndMeetBoosterExposure()
        {
            StageDefinition stage = StageCatalog.GetByDisplayNumber(13);
            DeterministicStageGateSequence sequence =
                new DeterministicStageGateSequence(stage);
            List<GatePlan> plans =
                new List<GatePlan>(stage.TargetGateCount);

            for (int gate = 0; gate < stage.TargetGateCount; gate++)
            {
                plans.Add(sequence.GetPlan(gate));
            }

            int flickerCount = 0;
            foreach (GatePlan plan in plans)
            {
                if (!plan.Modifier.IsFlicker)
                {
                    continue;
                }

                flickerCount++;
                Assert.That(plan.FlickerPlan.CycleColorCount, Is.EqualTo(3));
                Assert.That(
                    plan.FlickerPlan.GetCycleColor(0),
                    Is.EqualTo(plan.Color));
                for (int index = 1;
                    index < plan.FlickerPlan.CycleColorCount;
                    index++)
                {
                    Assert.That(
                        plan.FlickerPlan.GetCycleColor(index),
                        Is.EqualTo(stage.GetNextActiveColor(
                            plan.GateId,
                            plan.FlickerPlan.GetCycleColor(index - 1))));
                }
                Assert.That(
                    stage.GetNextActiveColor(
                        plan.GateId,
                        plan.FlickerPlan.GetCycleColor(2)),
                    Is.EqualTo(plan.FlickerPlan.GetCycleColor(0)));

                int firstVisible = System.Math.Max(0, plan.GateId - 5);
                float visibleDistance = 0f;
                for (int index = firstVisible;
                    index <= plan.GateId;
                    index++)
                {
                    visibleDistance += plans[index].Spacing;
                }
                float visibleAtBooster =
                    visibleDistance / stage.BoosterSpeed;
                Assert.That(
                    visibleAtBooster,
                    Is.GreaterThanOrEqualTo(
                        stage.FlickerSettings.SwitchIntervalSeconds *
                        stage.FlickerSettings.MinimumCyclesVisible));
            }

            Assert.That(
                flickerCount,
                Is.EqualTo(stage.FlickerSettings.MaxOccurrences));
        }

        [Test]
        public void FlickerJudgment_UsesExactCampaignGameplayTimeBoundary()
        {
            StageSession session = CreatePlaying(13, default);
            GatePlan plan = FindModifierPlan(
                session,
                GateModifierType.Flicker);
            float boundary =
                plan.FlickerPlan.SwitchIntervalSeconds -
                plan.FlickerPlan.PhaseOffsetSeconds;
            if (boundary <= 0f)
            {
                boundary = plan.FlickerPlan.SwitchIntervalSeconds;
            }
            RunnerColor expected = plan.FlickerPlan.GetCycleColor(1);
            MatchCurrentColor(session, expected);

            Assert.That(
                plan.GetJudgmentColor(boundary),
                Is.EqualTo(expected));
            Assert.That(
                session.ResolveGate(plan, boundary),
                Is.EqualTo(GateOutcome.Matched));
        }

        [Test]
        public void FlickerStage_BoosterUsesExistingAutoPassAndPreservesShield()
        {
            StageSession session = CreatePlaying(
                13,
                new StartItemSelection(true, true));
            GatePlan plan = FindModifierPlan(
                session,
                GateModifierType.Flicker);
            RunnerColor target = plan.GetJudgmentColor(
                session.ElapsedPlayingSeconds);
            while (session.CurrentColor == target)
            {
                session.TryToggleColor();
            }

            Assert.That(
                session.ResolveGate(
                    plan,
                    session.ElapsedPlayingSeconds),
                Is.EqualTo(GateOutcome.Boosted));
            Assert.That(session.ShieldActive, Is.True);
        }

        [Test]
        public void Retry_ReproducesCampaignTargetsCyclesAndPhases()
        {
            StageSession session = CreatePlaying(13, default);
            List<GatePlan> first = CollectPlans(session);

            session.RetryToSelection();
            session.BeginCountdown();
            session.CompleteCountdown();
            List<GatePlan> replay = CollectPlans(session);

            Assert.That(replay.Count, Is.EqualTo(first.Count));
            for (int index = 0; index < first.Count; index++)
            {
                Assert.That(
                    replay[index].Modifier.Types,
                    Is.EqualTo(first[index].Modifier.Types));
                Assert.That(replay[index].Color, Is.EqualTo(first[index].Color));
                Assert.That(
                    replay[index].FlickerPlan.PhaseOffsetSeconds,
                    Is.EqualTo(first[index].FlickerPlan.PhaseOffsetSeconds));
                CollectionAssert.AreEqual(
                    first[index].FlickerPlan.CopyCycleColors(),
                    replay[index].FlickerPlan.CopyCycleColors());
            }
        }

        private static void AssertDeterministicModifierStage(
            StageDefinition stage,
            GateModifierType expected)
        {
            DeterministicStageGateSequence first =
                new DeterministicStageGateSequence(stage);
            DeterministicStageGateSequence second =
                new DeterministicStageGateSequence(stage);
            int occurrences = 0;

            for (int gate = 0; gate < stage.TargetGateCount; gate++)
            {
                GatePlan a = first.GetPlan(gate);
                GatePlan b = second.GetPlan(gate);
                Assert.That(a.GateId, Is.EqualTo(gate));
                Assert.That(a.Modifier.Types, Is.EqualTo(b.Modifier.Types));
                Assert.That(a.Color, Is.EqualTo(b.Color));
                Assert.That(
                    a.Modifier.Types & ~expected,
                    Is.EqualTo(GateModifierType.None));
                if (a.Modifier.Has(expected))
                {
                    occurrences++;
                }
            }

            Assert.That(occurrences, Is.GreaterThan(0));
        }

        private static StageSession CreatePlaying(
            int stageNumber,
            StartItemSelection items)
        {
            StageSession session = new StageSession(
                StageCatalog.GetByDisplayNumber(stageNumber));
            session.SelectItems(items);
            session.BeginCountdown();
            session.CompleteCountdown();
            return session;
        }

        private static GatePlan FindModifierPlan(
            StageSession session,
            GateModifierType modifier)
        {
            for (int gate = 0;
                gate < session.Stage.TargetGateCount;
                gate++)
            {
                GatePlan plan = session.GetGatePlan(gate);
                if (plan.Modifier.Has(modifier))
                {
                    return plan;
                }
            }
            Assert.Fail($"Modifier {modifier} was not found.");
            return default;
        }

        private static List<GatePlan> CollectPlans(StageSession session)
        {
            List<GatePlan> result =
                new List<GatePlan>(session.Stage.TargetGateCount);
            for (int gate = 0;
                gate < session.Stage.TargetGateCount;
                gate++)
            {
                result.Add(session.GetGatePlan(gate));
            }
            return result;
        }

        private static void MatchCurrentColor(
            StageSession session,
            RunnerColor color)
        {
            int guard = 0;
            while (session.CurrentColor != color && guard < 4)
            {
                session.TryToggleColor();
                guard++;
            }
            Assert.That(session.CurrentColor, Is.EqualTo(color));
        }
    }
}

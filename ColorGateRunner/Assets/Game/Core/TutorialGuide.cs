using System;

namespace ColorGateRunner.Core
{
    public enum TutorialGuideStep
    {
        Inactive,
        TapToChange,
        HoldNeedsTap,
        HoldSameColor,
        HoldMatched,
        Running,
        Grades,
        Goal,
        Done
    }

    // The first-run Stage 1 coach. The presentation freezes the run just
    // before each of the first guided gates until the runner color matches,
    // then explains grades and the Goal. It never judges a gate; frozen time
    // is not gameplay time, so records, grades and simulations are unchanged.
    public sealed class TutorialGuide
    {
        public const int GuidedGateCount = 3;
        // Inside the PERFECT window, so a correct tap while frozen is PERFECT.
        // Leaves a frame of slack before the 0.25 s PERFECT window ends.
        public const float HoldLeadSeconds = 0.15f;
        public const float SameColorHoldSeconds = 1.6f;
        public const float MatchedHoldSeconds = 0.45f;
        public const float GradesSeconds = 4f;
        public const float GoalSeconds = 3f;

        private float _remaining;
        private int _heldGateIndex = -1;
        private TutorialGuideStep _step;

        public TutorialGuideStep Step
        {
            get => _step;
            private set
            {
                _step = value;
                Message = BuildMessage();
            }
        }

        // Rebuilt only when the step changes, never per frame.
        public string Message { get; private set; } = string.Empty;
        public RunnerColor GateColor { get; private set; }
        public bool Frozen =>
            Step == TutorialGuideStep.HoldNeedsTap ||
            Step == TutorialGuideStep.HoldSameColor ||
            Step == TutorialGuideStep.HoldMatched;
        // While frozen only a tap that can still fix the color is accepted.
        public bool AcceptsTap =>
            !Frozen || Step == TutorialGuideStep.HoldNeedsTap;

        private string BuildMessage() => _step switch
        {
            TutorialGuideStep.TapToChange =>
                "TAP ANYWHERE TO CHANGE COLOR\nMATCH EACH GATE YOU RUN THROUGH",
            TutorialGuideStep.HoldNeedsTap =>
                $"GATE IS {GateColor.ToString().ToUpperInvariant()} - TAP TO MATCH\n" +
                "YOUR COLOR IS CHECKED AS YOU PASS THROUGH",
            TutorialGuideStep.HoldSameColor =>
                "SAME COLOR - NO TAP NEEDED\n" +
                "YOUR COLOR IS CHECKED AS YOU PASS THROUGH",
            TutorialGuideStep.HoldMatched => "MATCHED! KEEP IT THROUGH THE GATE",
            TutorialGuideStep.Grades =>
                "SWITCH JUST BEFORE A GATE FOR PERFECT\n" +
                "PERFECT AND GREAT BUILD YOUR COMBO",
            TutorialGuideStep.Goal => "MATCH EVERY GATE TO REACH THE GOAL",
            _ => string.Empty
        };

        public void Begin()
        {
            Step = TutorialGuideStep.TapToChange;
            _heldGateIndex = -1;
            _remaining = 0f;
        }

        public void Reset()
        {
            Step = TutorialGuideStep.Inactive;
            _heldGateIndex = -1;
            _remaining = 0f;
        }

        // Distance before a gate at which the run must stop, or false when
        // this gate is not (or no longer) guided.
        public bool TryGetHoldDistance(
            int gateIndex,
            float speed,
            out float leadDistance)
        {
            leadDistance = Math.Max(0f, speed) * HoldLeadSeconds;
            return (Step == TutorialGuideStep.TapToChange ||
                    Step == TutorialGuideStep.Running) &&
                gateIndex >= 0 &&
                gateIndex < GuidedGateCount &&
                gateIndex > _heldGateIndex;
        }

        public void Hold(int gateIndex, RunnerColor gateColor, RunnerColor current)
        {
            _heldGateIndex = gateIndex;
            // Set before Step so the cached message names this gate.
            GateColor = gateColor;
            if (gateColor == current)
            {
                Step = TutorialGuideStep.HoldSameColor;
                _remaining = SameColorHoldSeconds;
            }
            else
            {
                Step = TutorialGuideStep.HoldNeedsTap;
                _remaining = 0f;
            }
        }

        public void OnColorChanged(RunnerColor current)
        {
            if (Step == TutorialGuideStep.HoldNeedsTap && current == GateColor)
            {
                Step = TutorialGuideStep.HoldMatched;
                _remaining = MatchedHoldSeconds;
            }
        }

        // Called for every resolved guided gate, including a mismatch that a
        // Continue then clears.
        public void OnGateResolved(int gateIndex)
        {
            if (gateIndex == GuidedGateCount - 1 &&
                Step == TutorialGuideStep.Running)
            {
                Step = TutorialGuideStep.Grades;
                _remaining = GradesSeconds;
            }
        }

        // Advances real (unscaled presentation) time.
        public void Tick(float deltaTime)
        {
            if (_remaining <= 0f ||
                Step == TutorialGuideStep.HoldNeedsTap)
            {
                return;
            }
            _remaining -= deltaTime;
            if (_remaining > 0f)
            {
                return;
            }
            _remaining = 0f;
            Step = Step switch
            {
                TutorialGuideStep.HoldSameColor => TutorialGuideStep.Running,
                TutorialGuideStep.HoldMatched => TutorialGuideStep.Running,
                TutorialGuideStep.Grades => TutorialGuideStep.Goal,
                TutorialGuideStep.Goal => TutorialGuideStep.Done,
                _ => Step
            };
            if (Step == TutorialGuideStep.Goal)
            {
                _remaining = GoalSeconds;
            }
        }
    }
}

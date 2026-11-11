namespace ColorGateRunner.Core
{
    public enum TutorialGuideStep
    {
        Inactive,
        Running,
        HoldNeedsTap,
        HoldSameColor,
        HoldMatched,
        HoldTimed,
        // Moving again after a match; taps wait until the held gate passes.
        Passing
    }

    // Runs one Stage's TutorialScript during a guide run. The presentation
    // freezes the run at each hold; frozen time is not gameplay time, so
    // judgment, grades, records and simulations are unchanged. Banners are
    // short lines shown on events without stopping the run.
    public sealed class TutorialGuide
    {
        // Inside the 0.25 s PERFECT window with a frame of slack.
        public const float HoldLeadSeconds = 0.15f;
        public const float SameColorHoldSeconds = 1.6f;
        public const float MatchedHoldSeconds = 0.45f;

        private TutorialScript _script;
        private RunnerColor _thirdColor;
        private int _holdsDone;
        private int _heldGateIndex = -1;
        private float _holdRemaining;
        private string _bannerMessage = string.Empty;
        private float _bannerRemaining;
        private bool _bannerUntilHold;
        private int _afterIndex = -1;
        private int _shownEvents;
        private string _holdMessage = string.Empty;

        public TutorialGuideStep Step { get; private set; }
        public TutorialScript Script => _script;
        public RunnerColor GateColor { get; private set; }
        public int HoldsDone => _holdsDone;
        public int HeldGateIndex => _heldGateIndex;
        // Rebuilt only when the state changes, never per frame.
        public string Message { get; private set; } = string.Empty;

        public bool Active => Step != TutorialGuideStep.Inactive;
        public bool Frozen =>
            Step == TutorialGuideStep.HoldNeedsTap ||
            Step == TutorialGuideStep.HoldSameColor ||
            Step == TutorialGuideStep.HoldMatched ||
            Step == TutorialGuideStep.HoldTimed;
        // While frozen only a tap that can still fix the color is accepted;
        // after a match no tap can undo it before the gate.
        public bool AcceptsTap =>
            Step == TutorialGuideStep.HoldNeedsTap ||
            (!Frozen && Step != TutorialGuideStep.Passing);
        // A BeforeHide hold may stop with an earlier gate still ahead.
        public bool HoldNeedsNextGate =>
            _script == null || _script.HoldKind != TutorialHoldKind.BeforeHide;

        public void Begin(TutorialScript script, RunnerColor thirdColor)
        {
            Reset();
            if (script == null) return;
            _script = script;
            _thirdColor = thirdColor;
            Step = TutorialGuideStep.Running;
        }

        public void Reset()
        {
            _script = null;
            _holdsDone = 0;
            _heldGateIndex = -1;
            _holdRemaining = 0f;
            _bannerMessage = string.Empty;
            _bannerRemaining = 0f;
            _bannerUntilHold = false;
            _afterIndex = -1;
            _shownEvents = 0;
            _holdMessage = string.Empty;
            Step = TutorialGuideStep.Inactive;
            Refresh();
        }

        // True when the run should stop before this gate.
        public bool TryGetHold(int gateIndex, GatePlan plan)
        {
            return Step == TutorialGuideStep.Running &&
                _holdsDone < _script.HoldCount &&
                gateIndex > _heldGateIndex &&
                _script.Matches(plan, _thirdColor);
        }

        public void Hold(int gateIndex, RunnerColor gateColor, RunnerColor current)
        {
            _heldGateIndex = gateIndex;
            _holdsDone++;
            GateColor = gateColor;
            if (_bannerUntilHold) ClearBanner();
            string color = ColorName(gateColor);
            if (!_script.WaitForMatch)
            {
                Step = TutorialGuideStep.HoldTimed;
                _holdRemaining = _script.TimedHoldSeconds;
                _holdMessage = string.Format(_script.NeedsTapMessage, color);
            }
            else if (gateColor == current)
            {
                Step = TutorialGuideStep.HoldSameColor;
                _holdRemaining = _script.SameColorHoldSeconds;
                _holdMessage = string.Format(_script.SameColorMessage, color);
            }
            else
            {
                Step = TutorialGuideStep.HoldNeedsTap;
                _holdRemaining = 0f;
                _holdMessage = string.Format(_script.NeedsTapMessage, color);
            }
            Refresh();
        }

        public void OnColorChanged(RunnerColor current)
        {
            if (Step == TutorialGuideStep.HoldNeedsTap && current == GateColor)
            {
                Step = TutorialGuideStep.HoldMatched;
                _holdRemaining = MatchedHoldSeconds;
                _holdMessage = _script.MatchedMessage;
                Refresh();
            }
        }

        // Called for every resolved gate, including a mismatch that a
        // Continue then clears.
        public void OnGateResolved(int gateIndex)
        {
            if (Step == TutorialGuideStep.Passing && gateIndex == _heldGateIndex)
            {
                Step = TutorialGuideStep.Running;
                Refresh();
            }
            if (!Active || gateIndex != _heldGateIndex ||
                _holdsDone < _script.HoldCount || _afterIndex >= 0 ||
                _script.AfterHolds.Length == 0)
            {
                return;
            }
            _afterIndex = 0;
            ShowBanner(_script.AfterHolds[0].Message, _script.AfterHolds[0].Seconds);
        }

        public void OnEvent(TutorialEvent trigger, RunnerColor color = default)
        {
            if (!Active) return;
            int bit = 1 << (int)trigger;
            if ((_shownEvents & bit) != 0) return;
            TutorialBanner[] banners = _script.Banners;
            for (int index = 0; index < banners.Length; index++)
            {
                if (banners[index].Trigger != trigger) continue;
                _shownEvents |= bit;
                ShowBanner(
                    string.Format(banners[index].Message, ColorName(color)),
                    banners[index].Seconds);
                return;
            }
        }

        // Advances presentation time; banners wait while the run is frozen.
        public void Tick(float deltaTime)
        {
            if (!Active) return;
            if (Frozen)
            {
                if (Step == TutorialGuideStep.HoldNeedsTap) return;
                _holdRemaining -= deltaTime;
                if (_holdRemaining > 0f) return;
                _holdRemaining = 0f;
                // A matched color is carried through the gate; a timed
                // explanation hands control back.
                Step = Step == TutorialGuideStep.HoldTimed
                    ? TutorialGuideStep.Running
                    : TutorialGuideStep.Passing;
                Refresh();
                return;
            }
            if (_bannerUntilHold || _bannerRemaining <= 0f) return;
            _bannerRemaining -= deltaTime;
            if (_bannerRemaining > 0f) return;
            ClearBanner();
            if (_afterIndex >= 0 && _afterIndex < _script.AfterHolds.Length - 1)
            {
                _afterIndex++;
                TutorialBanner next = _script.AfterHolds[_afterIndex];
                ShowBanner(next.Message, next.Seconds);
            }
        }

        private void ShowBanner(string message, float seconds)
        {
            _bannerMessage = message;
            _bannerUntilHold = seconds <= 0f;
            _bannerRemaining = seconds;
            Refresh();
        }

        private void ClearBanner()
        {
            _bannerMessage = string.Empty;
            _bannerRemaining = 0f;
            _bannerUntilHold = false;
            Refresh();
        }

        private void Refresh()
        {
            Message = Frozen || Step == TutorialGuideStep.Passing
                ? _holdMessage
                : _bannerMessage;
        }

        private static string ColorName(RunnerColor color) =>
            color.ToString().ToUpperInvariant();
    }
}

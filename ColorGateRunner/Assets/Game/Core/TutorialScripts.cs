using System;

namespace ColorGateRunner.Core
{
    // Which gates a guide holds before.
    public enum TutorialGateTarget
    {
        AnyGate,
        ThirdColor,
        Camouflage,
        Ice,
        EchoProvider,
        Hidden,
        Flicker
    }

    // Where the run stops relative to the target gate.
    public enum TutorialHoldKind
    {
        // Just before the gate, inside the PERFECT window.
        Standard,
        // Halfway from the previous gate, so the mechanic is seen ahead.
        Midway,
        // While a Hidden gate still shows its color; the gate need not be next.
        BeforeHide
    }

    public enum TutorialEvent
    {
        Go,
        ShieldAbsorbed,
        BoosterEnded,
        FogStarted,
        EchoStored,
        EchoUsed
    }

    public readonly struct TutorialBanner
    {
        public TutorialBanner(TutorialEvent trigger, string message, float seconds)
        {
            Trigger = trigger;
            Message = message;
            Seconds = seconds;
        }

        public TutorialEvent Trigger { get; }
        // {0} is the event color, when the event has one.
        public string Message { get; }
        // Zero keeps the banner until the first hold.
        public float Seconds { get; }
    }

    // What one Stage's guide run teaches. Messages take the gate color as {0}.
    public sealed class TutorialScript
    {
        internal TutorialScript(
            TutorialTopic topic,
            TutorialGateTarget target,
            TutorialHoldKind holdKind,
            int holdCount,
            bool waitForMatch,
            string needsTapMessage,
            string sameColorMessage,
            string matchedMessage,
            float timedHoldSeconds,
            TutorialBanner[] banners,
            TutorialBanner[] afterHolds)
        {
            Topic = topic;
            Target = target;
            HoldKind = holdKind;
            HoldCount = holdCount;
            WaitForMatch = waitForMatch;
            NeedsTapMessage = needsTapMessage;
            SameColorMessage = sameColorMessage;
            MatchedMessage = matchedMessage;
            TimedHoldSeconds = timedHoldSeconds;
            Banners = banners ?? Array.Empty<TutorialBanner>();
            AfterHolds = afterHolds ?? Array.Empty<TutorialBanner>();
        }

        public TutorialTopic Topic { get; }
        public TutorialGateTarget Target { get; }
        public TutorialHoldKind HoldKind { get; }
        public int HoldCount { get; }
        // Waits for a matching tap; otherwise the hold is timed and taps are
        // blocked so an earlier unresolved gate keeps the runner's color.
        public bool WaitForMatch { get; }
        public string NeedsTapMessage { get; }
        public string SameColorMessage { get; }
        public string MatchedMessage { get; }
        public float TimedHoldSeconds { get; }
        public TutorialBanner[] Banners { get; }
        // Shown one after another once the last guided gate resolves.
        public TutorialBanner[] AfterHolds { get; }

        public bool Matches(GatePlan plan, RunnerColor thirdColor) => Target switch
        {
            TutorialGateTarget.AnyGate => true,
            TutorialGateTarget.ThirdColor => plan.Color == thirdColor,
            TutorialGateTarget.Camouflage => plan.Modifier.IsCamouflage,
            TutorialGateTarget.Ice => plan.Modifier.IsIce,
            TutorialGateTarget.EchoProvider => plan.Modifier.IsEchoProvider,
            TutorialGateTarget.Hidden => plan.Modifier.IsHidden,
            TutorialGateTarget.Flicker => plan.Modifier.IsFlicker,
            _ => false
        };

        // Runner distance before the gate at which this hold starts;
        // gateSpacing is the distance from the previous gate.
        public float HoldLeadDistance(
            float gateSpeed,
            float gateSpacing,
            HiddenSettings hidden)
        {
            float speed = Math.Max(0f, gateSpeed);
            float standard = speed * TutorialGuide.HoldLeadSeconds;
            return HoldKind switch
            {
                TutorialHoldKind.Midway => Math.Max(standard, gateSpacing * 0.5f),
                TutorialHoldKind.BeforeHide => speed *
                    (hidden.HideLeadTimeSeconds + TutorialScripts.HideMarginSeconds),
                _ => standard
            };
        }
    }

    public static class TutorialScripts
    {
        internal const float HideMarginSeconds = 0.25f;
        // Mechanic explanations stop the run with taps blocked, then resume.
        public const float ExplainSeconds = 3f;
        private const string CheckedLine = "YOUR COLOR IS CHECKED AS YOU PASS THROUGH";

        public static TutorialScript For(TutorialTopic topic) => topic switch
        {
            TutorialTopic.Basics => new TutorialScript(
                topic, TutorialGateTarget.AnyGate, TutorialHoldKind.Standard, 3, true,
                "GATE IS {0} - TAP TO MATCH\n" + CheckedLine,
                "SAME COLOR - NO TAP NEEDED\n" + CheckedLine,
                "MATCHED! KEEP IT THROUGH THE GATE",
                0f,
                new[]
                {
                    new TutorialBanner(TutorialEvent.Go,
                        "TAP ANYWHERE TO CHANGE COLOR\nMATCH EACH GATE YOU RUN THROUGH", 0f)
                },
                new[]
                {
                    new TutorialBanner(TutorialEvent.Go,
                        "SWITCH JUST BEFORE A GATE FOR PERFECT\n" +
                        "PERFECT AND GREAT BUILD YOUR COMBO", 4f),
                    new TutorialBanner(TutorialEvent.Go,
                        "MATCH EVERY GATE TO REACH THE GOAL", 3f)
                }),
            TutorialTopic.ThirdColor => new TutorialScript(
                topic, TutorialGateTarget.ThirdColor, TutorialHoldKind.Standard, 1, true,
                "NEW COLOR: {0} - TAP TO MATCH\nTAPS GO RED > BLUE > GREEN",
                "{0} ALREADY - NO TAP NEEDED",
                "MATCHED! KEEP IT THROUGH THE GATE",
                0f, null, null),
            TutorialTopic.Shield => Banners(topic,
                new TutorialBanner(TutorialEvent.Go,
                    "FREE SHIELD: ONE WRONG GATE IS FORGIVEN", 4f),
                new TutorialBanner(TutorialEvent.ShieldAbsorbed,
                    "SHIELD USED - THE NEXT MISS FAILS", 3f)),
            TutorialTopic.Booster => Banners(topic,
                new TutorialBanner(TutorialEvent.Go,
                    "FREE BOOSTER: YOU DASH THROUGH THE GATES", 3f),
                new TutorialBanner(TutorialEvent.BoosterEnded,
                    "BOOSTER ENDS - MATCH THE GATES AGAIN", 3f)),
            TutorialTopic.Camouflage => Explain(topic, TutorialGateTarget.Camouflage,
                "CAMOUFLAGE: GRAY GATES HIDE THEIR COLOR\nIT SHOWS ({0} HERE) AS YOU COME CLOSE"),
            TutorialTopic.Fog => Banners(topic,
                new TutorialBanner(TutorialEvent.FogStarted,
                    "FOG! READ EACH GATE AS IT COMES OUT", 3.5f)),
            TutorialTopic.Ice => Explain(topic, TutorialGateTarget.Ice,
                "ICE AHEAD: YOUR SPEED DOUBLES\nEVERY ICE GATE NEEDS A NEW COLOR"),
            TutorialTopic.Echo => new TutorialScript(
                topic, TutorialGateTarget.EchoProvider, TutorialHoldKind.Midway, 1, false,
                "ECHO GATE: MATCH {0} TO STORE IT\nA STORED ECHO SAVES YOU ONCE",
                string.Empty,
                string.Empty,
                ExplainSeconds,
                new[]
                {
                    new TutorialBanner(TutorialEvent.EchoStored,
                        "ECHO STORED: {0}\nIT SAVES YOU ONCE AT A {0} GATE", 4f),
                    new TutorialBanner(TutorialEvent.EchoUsed,
                        "ECHO USED - IT LET YOU THROUGH", 3f)
                },
                null),
            TutorialTopic.Hidden => new TutorialScript(
                topic, TutorialGateTarget.Hidden, TutorialHoldKind.BeforeHide, 1, false,
                "REMEMBER THIS GATE: {0}\nIT GOES DARK BEFORE YOU ARRIVE",
                "REMEMBER THIS GATE: {0}\nIT GOES DARK BEFORE YOU ARRIVE",
                string.Empty,
                ExplainSeconds, null, null),
            TutorialTopic.Flicker => Explain(topic, TutorialGateTarget.Flicker,
                "FLICKER: THIS GATE KEEPS CHANGING COLOR\nMATCH THE COLOR IT LOCKS ON"),
            _ => null
        };

        private static TutorialScript Explain(
            TutorialTopic topic,
            TutorialGateTarget target,
            string message) =>
            new TutorialScript(
                topic, target, TutorialHoldKind.Midway, 1, false,
                message, string.Empty, string.Empty, ExplainSeconds, null, null);

        private static TutorialScript Banners(TutorialTopic topic, params TutorialBanner[] banners) =>
            new TutorialScript(
                topic, TutorialGateTarget.AnyGate, TutorialHoldKind.Standard, 0, true,
                string.Empty, string.Empty, string.Empty, 0f, banners, null);
    }
}

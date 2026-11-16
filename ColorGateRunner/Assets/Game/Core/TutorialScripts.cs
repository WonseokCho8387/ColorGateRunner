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

    // Where the run stops, or a notice starts, relative to the target gate.
    public enum TutorialHoldKind
    {
        // Just before the gate, inside the PERFECT window.
        Standard,
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
            bool notice,
            string needsTapMessage,
            string sameColorMessage,
            string matchedMessage,
            TutorialBanner[] banners,
            TutorialBanner[] afterHolds)
        {
            Topic = topic;
            Target = target;
            HoldKind = holdKind;
            HoldCount = holdCount;
            Notice = notice;
            NeedsTapMessage = needsTapMessage;
            SameColorMessage = sameColorMessage;
            MatchedMessage = matchedMessage;
            Banners = banners ?? Array.Empty<TutorialBanner>();
            AfterHolds = afterHolds ?? Array.Empty<TutorialBanner>();
        }

        public TutorialTopic Topic { get; }
        public TutorialGateTarget Target { get; }
        public TutorialHoldKind HoldKind { get; }
        public int HoldCount { get; }
        // A notice shows NeedsTapMessage as a line without stopping the run;
        // otherwise the run stops until the color matches.
        public bool Notice { get; }
        public string NeedsTapMessage { get; }
        public string SameColorMessage { get; }
        public string MatchedMessage { get; }
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

        // Runner distance before the gate at which this hold or notice
        // starts; gateSpacing is the distance from the previous gate. A notice
        // starts no later than the previous gate, so it can be read ahead.
        public float HoldLeadDistance(
            float gateSpeed,
            float gateSpacing,
            HiddenSettings hidden)
        {
            float speed = Math.Max(0f, gateSpeed);
            float standard = speed * TutorialGuide.HoldLeadSeconds;
            float lead = HoldKind == TutorialHoldKind.BeforeHide
                ? speed * (hidden.HideLeadTimeSeconds + TutorialScripts.HideMarginSeconds)
                : standard;
            return Notice ? Math.Max(lead, gateSpacing) : lead;
        }
    }

    public static class TutorialScripts
    {
        internal const float HideMarginSeconds = 0.25f;
        // Mechanic explanations are lines shown without stopping the run.
        public const float NoticeSeconds = 3.5f;
        private const string CheckedLine = "YOUR COLOR IS CHECKED AS YOU PASS THROUGH";

        public static TutorialScript For(TutorialTopic topic) => topic switch
        {
            TutorialTopic.Basics => new TutorialScript(
                topic, TutorialGateTarget.AnyGate, TutorialHoldKind.Standard, 3, false,
                "GATE IS {0} - TAP TO MATCH\n" + CheckedLine,
                "SAME COLOR - NO TAP NEEDED\n" + CheckedLine,
                "MATCHED! KEEP IT THROUGH THE GATE",
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
                topic, TutorialGateTarget.ThirdColor, TutorialHoldKind.Standard, 1, false,
                "NEW COLOR: {0} - TAP TO MATCH\nTAPS GO RED > BLUE > GREEN",
                "{0} ALREADY - NO TAP NEEDED",
                "MATCHED! KEEP IT THROUGH THE GATE",
                null, null),
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
            // The color is not shown yet when the line starts.
            TutorialTopic.Camouflage => Notice(topic, TutorialGateTarget.Camouflage,
                TutorialHoldKind.Standard,
                "CAMOUFLAGE: GRAY GATES HIDE THEIR COLOR\nIT SHOWS AS YOU COME CLOSE"),
            TutorialTopic.Fog => Banners(topic,
                new TutorialBanner(TutorialEvent.FogStarted,
                    "FOG! READ EACH GATE AS IT COMES OUT", 3.5f)),
            TutorialTopic.Ice => Notice(topic, TutorialGateTarget.Ice,
                TutorialHoldKind.Standard,
                "ICE: YOUR SPEED DOUBLES ON ICE\nEVERY ICE GATE NEEDS A NEW COLOR"),
            TutorialTopic.Echo => Notice(topic, TutorialGateTarget.EchoProvider,
                TutorialHoldKind.Standard,
                "ECHO GATE: MATCH {0} TO STORE IT\nA STORED ECHO SAVES YOU ONCE",
                new TutorialBanner(TutorialEvent.EchoStored,
                    "ECHO STORED: {0}\nIT SAVES YOU ONCE AT A {0} GATE", 4f),
                new TutorialBanner(TutorialEvent.EchoUsed,
                    "ECHO USED - IT LET YOU THROUGH", 3f)),
            TutorialTopic.Hidden => Notice(topic, TutorialGateTarget.Hidden,
                TutorialHoldKind.BeforeHide,
                "REMEMBER THIS GATE: {0}\nIT GOES DARK BEFORE YOU ARRIVE"),
            TutorialTopic.Flicker => Notice(topic, TutorialGateTarget.Flicker,
                TutorialHoldKind.Standard,
                "FLICKER: THIS GATE KEEPS CHANGING COLOR\nMATCH THE COLOR IT LOCKS ON"),
            _ => null
        };

        private static TutorialScript Notice(
            TutorialTopic topic,
            TutorialGateTarget target,
            TutorialHoldKind kind,
            string message,
            params TutorialBanner[] banners) =>
            new TutorialScript(
                topic, target, kind, 1, true,
                message, string.Empty, string.Empty, banners, null);

        private static TutorialScript Banners(TutorialTopic topic, params TutorialBanner[] banners) =>
            new TutorialScript(
                topic, TutorialGateTarget.AnyGate, TutorialHoldKind.Standard, 0, false,
                string.Empty, string.Empty, string.Empty, banners, null);
    }
}

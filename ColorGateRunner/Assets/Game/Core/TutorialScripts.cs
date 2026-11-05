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
        // Just after a Camouflage gate has revealed its color.
        AfterReveal,
        // While a Hidden gate still shows its color; the gate need not be next.
        BeforeHide,
        // Just after a Flicker gate has locked its color.
        AfterLock
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

        // Runner distance before the gate at which this hold starts.
        public float HoldLeadDistance(
            float gateSpeed,
            CamouflageSettings camouflage,
            HiddenSettings hidden)
        {
            float speed = Math.Max(0f, gateSpeed);
            return HoldKind switch
            {
                TutorialHoldKind.AfterReveal => speed * Math.Max(
                    TutorialGuide.HoldLeadSeconds,
                    camouflage.RevealLeadTimeSeconds -
                    camouflage.RevealTransitionSeconds -
                    TutorialScripts.RevealSettleSeconds),
                TutorialHoldKind.BeforeHide => speed *
                    (hidden.HideLeadTimeSeconds + TutorialScripts.HideMarginSeconds),
                TutorialHoldKind.AfterLock =>
                    FlickerGateRuntimeState.LockDistance - TutorialScripts.LockMarginDistance,
                _ => speed * TutorialGuide.HoldLeadSeconds
            };
        }
    }

    public static class TutorialScripts
    {
        internal const float RevealSettleSeconds = 0.1f;
        internal const float HideMarginSeconds = 0.25f;
        internal const float LockMarginDistance = 2f;
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
            TutorialTopic.Camouflage => new TutorialScript(
                topic, TutorialGateTarget.Camouflage, TutorialHoldKind.AfterReveal, 1, true,
                "COLOR REVEALED: {0} - TAP TO MATCH\nGRAY GATES SHOW THEIR COLOR UP CLOSE",
                "COLOR REVEALED: {0} - NO TAP NEEDED\nGRAY GATES SHOW THEIR COLOR UP CLOSE",
                "MATCHED! KEEP IT THROUGH THE GATE",
                0f, null, null),
            TutorialTopic.Fog => Banners(topic,
                new TutorialBanner(TutorialEvent.FogStarted,
                    "FOG! READ EACH GATE AS IT COMES OUT", 3.5f)),
            TutorialTopic.Ice => new TutorialScript(
                topic, TutorialGateTarget.Ice, TutorialHoldKind.Standard, 1, true,
                "ICE: SPEED X2 - TAP TO {0}\nEVERY ICE GATE NEEDS A NEW COLOR",
                "ICE: SPEED X2 - KEEP {0}",
                "MATCHED! KEEP THE RHYTHM ON ICE",
                0f, null, null),
            TutorialTopic.Echo => new TutorialScript(
                topic, TutorialGateTarget.EchoProvider, TutorialHoldKind.Standard, 1, true,
                "ECHO GATE: MATCH {0} TO STORE IT",
                "ECHO GATE: KEEP {0} TO STORE IT",
                "MATCHED! PASS TO STORE THE ECHO",
                0f,
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
                2.2f, null, null),
            TutorialTopic.Flicker => new TutorialScript(
                topic, TutorialGateTarget.Flicker, TutorialHoldKind.AfterLock, 1, true,
                "LOCKED ON {0} - TAP TO MATCH\nFLICKER GATES STOP JUST BEFORE YOU",
                "LOCKED ON {0} - NO TAP NEEDED\nFLICKER GATES STOP JUST BEFORE YOU",
                "MATCHED! KEEP IT THROUGH THE GATE",
                0f, null, null),
            _ => null
        };

        private static TutorialScript Banners(TutorialTopic topic, params TutorialBanner[] banners) =>
            new TutorialScript(
                topic, TutorialGateTarget.AnyGate, TutorialHoldKind.Standard, 0, true,
                string.Empty, string.Empty, string.Empty, 0f, banners, null);
    }
}

using System;

namespace ColorGateRunner.Core
{
    public enum TutorialTopic
    {
        None,
        Basics,
        ThirdColor,
        Shield,
        Booster,
        StartItems,
        Camouflage,
        Fog,
        Ice,
        Echo,
        Hidden,
        Flicker
    }

    public readonly struct TutorialCard
    {
        public TutorialCard(string title, string body)
        {
            Title = title;
            Body = body;
        }

        public string Title { get; }
        public string Body { get; }
    }

    // Player-facing rule cards: the HOW TO PLAY pages, the crown card shown
    // after the first Stage 1 clear, and the one-time intro of each new
    // mechanic on the first Stage that uses it.
    public static class TutorialCards
    {
        public const string TutorialStageId = "stage-01";

        public static TutorialCard[] HowToPlay(int comboTarget) => new[]
        {
            new TutorialCard(
                "CHANGE COLOR",
                "YOUR RUNNER MOVES BY ITSELF.\n\n" +
                "TAP ANYWHERE TO CHANGE ITS COLOR.\n" +
                "EACH TAP STEPS TO THE NEXT COLOR\nOF THE STAGE."),
            new TutorialCard(
                "MATCH THE GATE",
                "YOUR COLOR IS CHECKED THE MOMENT\nYOU PASS THROUGH A GATE.\n\n" +
                "SAME COLOR: THE GATE BREAKS.\n" +
                "WRONG COLOR: THE RUN FAILS.\n\n" +
                "CLEAR EVERY GATE TO REACH THE GOAL."),
            new TutorialCard(
                "TIMING",
                "SWITCH WITHIN 0.25s OF A GATE,\nOR KEEP THE RIGHT COLOR: PERFECT\n" +
                "WITHIN 0.5s: GREAT\nEARLIER: GOOD\n\n" +
                "PERFECT AND GREAT BUILD YOUR COMBO.\nGOOD BREAKS IT."),
            Crowns(comboTarget)
        };

        public static TutorialCard Crowns(int comboTarget) => new TutorialCard(
            "CROWNS",
            "EVERY STAGE HAS THREE CROWNS,\nEARNED BY CLEARING IT.\n\n" +
            "CLEAN: NO CONTINUE, NO SHIELD HIT\n" +
            $"{comboTarget} COMBO: REACH A {comboTarget} COMBO\n" +
            "FULL COMBO: CLEAN WITH NO GOOD\n\n" +
            "HARDER STAGES NEED A LONGER COMBO.");

        // The intro of the mechanic this Stage introduces, if it is the first
        // catalog Stage that uses it.
        // What this Stage is the first to introduce: the tutorial Stage
        // teaches the basics, later Stages the first use of a mechanic.
        public static TutorialTopic GetStageTopic(
            IStageCatalog catalog,
            StageDefinition stage)
        {
            if (catalog == null) throw new ArgumentNullException(nameof(catalog));
            if (stage == null) throw new ArgumentNullException(nameof(stage));
            if (stage.StageId == TutorialStageId) return TutorialTopic.Basics;
            bool firstThreeColors = stage.AllowedColorCount >= 3;
            bool firstItems = stage.ShieldAllowed || stage.BoosterAllowed;
            bool firstMechanic = stage.PrimaryMechanic != StagePrimaryMechanic.None;
            for (int index = 0; index < catalog.Count; index++)
            {
                StageDefinition earlier = catalog.GetByIndex(index);
                if (earlier.DisplayNumber >= stage.DisplayNumber) continue;
                firstThreeColors &= earlier.AllowedColorCount < 3;
                firstItems &= !earlier.ShieldAllowed && !earlier.BoosterAllowed;
                firstMechanic &= earlier.PrimaryMechanic != stage.PrimaryMechanic;
            }
            if (firstMechanic)
            {
                return stage.PrimaryMechanic switch
                {
                    StagePrimaryMechanic.Shield => TutorialTopic.Shield,
                    StagePrimaryMechanic.Booster => TutorialTopic.Booster,
                    StagePrimaryMechanic.Camouflage => TutorialTopic.Camouflage,
                    StagePrimaryMechanic.Fog => TutorialTopic.Fog,
                    StagePrimaryMechanic.Ice => TutorialTopic.Ice,
                    StagePrimaryMechanic.Echo => TutorialTopic.Echo,
                    StagePrimaryMechanic.Hidden => TutorialTopic.Hidden,
                    StagePrimaryMechanic.Flicker => TutorialTopic.Flicker,
                    _ => TutorialTopic.None
                };
            }
            if (firstItems) return TutorialTopic.StartItems;
            return firstThreeColors ? TutorialTopic.ThirdColor : TutorialTopic.None;
        }

        // The intro of the mechanic this Stage introduces, if it is the first
        // catalog Stage that uses it.
        public static bool TryGetStageIntro(
            IStageCatalog catalog,
            StageDefinition stage,
            out TutorialCard card)
        {
            TutorialTopic topic = GetStageTopic(catalog, stage);
            card = default;
            if (topic == TutorialTopic.None || topic == TutorialTopic.Basics)
            {
                return false;
            }
            if (topic != TutorialTopic.StartItems &&
                topic != TutorialTopic.ThirdColor)
            {
                card = ForMechanic(stage.PrimaryMechanic);
                return true;
            }
            if (topic == TutorialTopic.StartItems)
            {
                card = new TutorialCard(
                    "START ITEMS",
                    "BEFORE START YOU MAY SWITCH ON\nITEMS YOU OWN.\n\n" +
                    "SHIELD: SURVIVES ONE WRONG GATE\n" +
                    "BOOSTER: DASHES THROUGH THE\nFIRST STRETCH\n\n" +
                    "EACH START USES ONE OF EACH\nSELECTED ITEM.");
                return true;
            }
            card = new TutorialCard(
                "GREEN",
                "A THIRD COLOR JOINS: GREEN.\n\n" +
                "TAPS NOW CYCLE\nRED > BLUE > GREEN > RED.\n\n" +
                "PLAN AHEAD: GOING BACK A COLOR\nTAKES TWO TAPS.");
            return true;
        }

        private static TutorialCard ForMechanic(StagePrimaryMechanic mechanic) =>
            mechanic switch
            {
                StagePrimaryMechanic.Shield => new TutorialCard(
                    "SHIELD",
                    "THIS STAGE GIVES YOU A FREE SHIELD.\n\n" +
                    "IT ABSORBS ONE WRONG GATE,\nTHEN BREAKS.\n\n" +
                    "A SHIELD HIT LOSES THE CLEAN CROWN."),
                StagePrimaryMechanic.Booster => new TutorialCard(
                    "BOOSTER",
                    "THIS STAGE GIVES YOU A FREE BOOSTER.\n\n" +
                    "AFTER GO YOU DASH AT HIGH SPEED\nAND BREAK EVERY GATE ON THE WAY.\n\n" +
                    "GET READY TO MATCH WHEN IT ENDS."),
                StagePrimaryMechanic.Camouflage => new TutorialCard(
                    "CAMOUFLAGE",
                    "SOME GATES STAY GRAY AND\nSHOW NO COLOR AT FIRST.\n\n" +
                    "THEIR COLOR IS REVEALED\nAS YOU GET CLOSE.\n\n" +
                    "WAIT FOR THE REVEAL, THEN MATCH."),
                StagePrimaryMechanic.Fog => new TutorialCard(
                    "FOG",
                    "A FOG BANK ROLLS IN AND HIDES\nTHE GATES AHEAD FOR A FEW SECONDS.\n\n" +
                    "READ EACH GATE AS IT\nCOMES OUT OF THE FOG.\n\n" +
                    "THE FOG CLEARS AGAIN SOON."),
                StagePrimaryMechanic.Ice => new TutorialCard(
                    "ICE",
                    "ICE DOUBLES YOUR SPEED.\n\n" +
                    "GATES ON ICE ARRIVE FAST,\nAND EACH ONE NEEDS A NEW COLOR.\n\n" +
                    "KEEP A STEADY TAP RHYTHM."),
                StagePrimaryMechanic.Echo => new TutorialCard(
                    "ECHO",
                    "MATCH AN ECHO GATE TO STORE\nITS COLOR AS A SHELL.\n\n" +
                    "LATER, A GATE OF THAT COLOR\nLETS YOU THROUGH EVEN WITH\nTHE WRONG COLOR, ONCE.\n\n" +
                    "YOU HOLD ONE ECHO AT A TIME."),
                StagePrimaryMechanic.Hidden => new TutorialCard(
                    "HIDDEN",
                    "SOME GATES SHOW THEIR COLOR,\nTHEN GO DARK BEFORE YOU ARRIVE.\n\n" +
                    "REMEMBER THE COLOR\nWHILE IT IS SHOWN."),
                StagePrimaryMechanic.Flicker => new TutorialCard(
                    "FLICKER",
                    "FLICKER GATES KEEP CHANGING COLOR\nAS YOU APPROACH.\n\n" +
                    "THEY LOCK JUST BEFORE YOU ARRIVE.\n" +
                    "MATCH THE COLOR THEY LOCK ON."),
                _ => new TutorialCard(string.Empty, string.Empty)
            };
    }
}

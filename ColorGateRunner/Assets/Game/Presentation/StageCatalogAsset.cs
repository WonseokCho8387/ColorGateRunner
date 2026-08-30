using System;
using ColorGateRunner.Core;
using UnityEngine;

namespace ColorGateRunner.Presentation
{
    [CreateAssetMenu(
        fileName = "StageCatalog",
        menuName = "Color Gate Runner/Stage Catalog")]
    public sealed class StageCatalogAsset : ScriptableObject
    {
        private const int CurvedProfileSampleCount = 101;
        private const int CurrentCatalogRevision = 14;

        [Serializable]
        private sealed class StageEntry
        {
            [SerializeField] private string stageId;
            [SerializeField] private int displayNumber;
            [SerializeField] private string title;
            [SerializeField, TextArea] private string description;
            [SerializeField] private StageDifficulty difficulty;
            [SerializeField, Min(1)] private int targetGateCount;
            [SerializeField] private RunnerColor[] allowedColors;
            [SerializeField, Range(2, 3)]
            [Tooltip("Number of colors used by this stage. Uses the first colors in Allowed Colors.")]
            private int activeColorCount;

            [Header("Speed")]
            [SerializeField, Min(0.01f)] private float startingSpeed;
            [SerializeField, Min(0.01f)] private float maximumSpeed;
            [SerializeField] private AnimationCurve speedCurve =
                AnimationCurve.Linear(0f, 0f, 1f, 1f);

            [Header("Cadence and Patterns")]
            [SerializeField, Min(0.01f)] private float cadenceStart;
            [SerializeField, Min(0.01f)] private float cadenceEnd;
            [SerializeField] private GatePatternType[] allowedPatterns;
            [SerializeField, Min(0)] private int introGateCount;
            [SerializeField, Min(1)] private int finalPressureGateCount;

            [Header("Items")]
            [SerializeField, Min(0.01f)] private float boosterDistance;
            [SerializeField, Min(0.01f)] private float boosterSpeed;
            [SerializeField] private bool shieldAllowed = true;
            [SerializeField] private bool boosterAllowed = true;

            [Header("Determinism")]
            [SerializeField] private uint seed;
            [SerializeField] private bool activeColorsFromStart = true;
            [SerializeField] private int[] firstGateIndicesByColor;

            [Header("Stage Mechanic")]
            [SerializeField]
            [Tooltip("The mechanic taught and emphasized by this stage.")]
            private StagePrimaryMechanic primaryMechanic;
            [SerializeField]
            [Tooltip("Gate modifiers eligible for deterministic placement in this stage.")]
            private GateModifierType gateModifiers;

            [Header("Echo")]
            [SerializeField]
            [Tooltip("Allows deterministic Echo offers on eligible ordinary gates.")]
            private bool echoEnabled;
            [SerializeField, Range(0f, 1f)] private float echoEligibleStart = 0.1f;
            [SerializeField, Range(0f, 1f)] private float echoEligibleEnd = 0.5f;
            [SerializeField] private bool firstEchoGuaranteed = true;
            [SerializeField, Range(0f, 1f)] private float firstEchoMin = 0.1f;
            [SerializeField, Range(0f, 1f)] private float firstEchoMax = 0.2f;
            [SerializeField, Range(0f, 1f)] private float additionalEchoChance = 0.3f;
            [SerializeField, Min(0)] private int echoCooldownGateCount = 2;
            [SerializeField, Min(0)] private int echoMaxAcquisitions = 3;

            [Header("Camouflage")]
            [SerializeField, Min(0.01f)]
            [Tooltip("Time-to-arrival at which a hidden gate begins revealing.")]
            private float camouflageRevealLeadTime = 1.25f;
            [SerializeField, Min(0f)]
            [Tooltip("Seconds used to transition from neutral to the target color.")]
            private float camouflageRevealTransition = 0.18f;

            [Header("Fog Curtain")]
            [SerializeField, Min(0.01f)] private float fogFadeInSeconds = 0.5f;
            [SerializeField, Min(0.01f)]
            private float fogFullOpacitySeconds = 5f;
            [SerializeField, Min(0.01f)] private float fogFadeOutSeconds = 0.5f;

            [Header("Ice Runway")]
            [SerializeField, Min(0.01f)]
            private float iceSpeedMultiplier =
                GateModifierRules.IceSpeedMultiplier;

            [Header("Hidden")]
            [SerializeField]
            [Tooltip("Enables deterministic Hidden placement for this stage.")]
            private bool hiddenEnabled;
            [SerializeField, Range(0f, 1f)]
            private float hiddenEligibleStart = 0.15f;
            [SerializeField, Range(0f, 1f)]
            private float hiddenEligibleEnd = 0.85f;
            [SerializeField, Range(0f, 1f)]
            private float hiddenOccurrenceChance = 0.35f;
            [SerializeField, Min(0)] private int hiddenMinimumGateCooldown = 2;
            [SerializeField, Min(0f)] private float hiddenReadableSeconds = 1f;
            [SerializeField, Min(0f)] private float hiddenLeadSeconds = 0.85f;
            [SerializeField, Min(0f)] private float hiddenTransitionSeconds = 0.12f;
            [SerializeField, Min(0)] private int hiddenMaxOccurrences = 4;
            [SerializeField] private bool hiddenFirstGuaranteed = true;

            [Header("Flicker")]
            [SerializeField]
            [Tooltip("Enables deterministic color-cycling Flicker placement for this stage.")]
            private bool flickerEnabled;
            [SerializeField, Range(0f, 1f)]
            private float flickerEligibleStart = 0.15f;
            [SerializeField, Range(0f, 1f)]
            private float flickerEligibleEnd = 0.85f;
            [SerializeField, Range(0f, 1f)]
            private float flickerOccurrenceChance = 0.30f;
            [SerializeField, Min(0)] private int flickerMinimumGateCooldown = 2;
            [SerializeField, Min(0)] private int flickerMaxOccurrences = 4;
            [SerializeField] private bool flickerFirstGuaranteed = true;
            [SerializeField, Min(0.01f)] private float flickerSwitchSeconds = 0.50f;
            [SerializeField, Min(0f)] private float flickerPulseSeconds = 0.10f;
            [SerializeField, Min(1)] private int flickerMinimumCyclesVisible = 3;
            [SerializeField] private bool flickerRandomizePhase = true;

            [Header("Stage-local Grant")]
            [SerializeField]
            [Tooltip("Provides a mechanic inside this stage without changing inventory.")]
            private bool mechanicGrantEnabled;
            [SerializeField] private StageMechanicGrantMechanic grantedMechanic;
            [SerializeField] private StageMechanicActivationMode grantActivationMode;
            [SerializeField, Range(0f, 1f)] private float grantActivationProgress;
            [SerializeField, Min(0)] private int grantChargeCount;

            public StageDefinition ToCore()
            {
                int colorCount = GetActiveColorCount();
                return new StageDefinition(
                    stageId,
                    displayNumber,
                    title,
                    description,
                    targetGateCount,
                    CopyFirst(allowedColors, colorCount),
                    CreateSpeedProfile(),
                    cadenceStart,
                    cadenceEnd,
                    allowedPatterns,
                    introGateCount,
                    finalPressureGateCount,
                    boosterDistance,
                    boosterSpeed,
                    shieldAllowed,
                    boosterAllowed,
                    seed,
                    activeColorsFromStart,
                    CopyFirst(firstGateIndicesByColor, colorCount),
                    primaryMechanic,
                    gateModifiers,
                    CreateEchoSettings(),
                    new CamouflageSettings(
                        camouflageRevealLeadTime,
                        camouflageRevealTransition),
                    new StageMechanicGrantSettings(
                        mechanicGrantEnabled,
                        grantedMechanic,
                        grantActivationMode,
                        grantActivationProgress,
                        grantChargeCount,
                        true),
                    CreateHiddenSettings(),
                    CreateFlickerSettings(),
                    difficulty,
                    CreateFogCurtainSettings(),
                    new IceRunwaySettings(iceSpeedMultiplier));
            }

            public static StageEntry Create(
                string id,
                int number,
                string stageTitle,
                string stageDescription,
                int gateCount,
                RunnerColor[] colors,
                float startSpeed,
                float maxSpeed,
                float startCadence,
                float endCadence,
                GatePatternType[] patterns,
                int introCount,
                int finalCount,
                float itemBoosterDistance,
                float itemBoosterSpeed,
                uint stageSeed,
                bool colorsActiveFromStart = true,
                int[] colorFirstGateIndices = null,
                StagePrimaryMechanic mechanic = StagePrimaryMechanic.None,
                GateModifierType modifiers = GateModifierType.None,
                bool enableEcho = false,
                bool enableGrant = false,
                StageMechanicGrantMechanic grantMechanic =
                    StageMechanicGrantMechanic.None,
                StageMechanicActivationMode activationMode =
                    StageMechanicActivationMode.ActiveAtStageStart,
                float activationProgress = 0f)
            {
                return new StageEntry
                {
                    stageId = id,
                    displayNumber = number,
                    title = stageTitle,
                    description = stageDescription,
                    targetGateCount = gateCount,
                    allowedColors = colors,
                    activeColorCount = colors.Length,
                    startingSpeed = startSpeed,
                    maximumSpeed = maxSpeed,
                    speedCurve = AnimationCurve.Linear(0f, 0f, 1f, 1f),
                    cadenceStart = startCadence,
                    cadenceEnd = endCadence,
                    allowedPatterns = patterns,
                    introGateCount = introCount,
                    finalPressureGateCount = finalCount,
                    boosterDistance = itemBoosterDistance,
                    boosterSpeed = itemBoosterSpeed,
                    shieldAllowed = true,
                    boosterAllowed = true,
                    seed = stageSeed,
                    activeColorsFromStart = colorsActiveFromStart,
                    firstGateIndicesByColor = colorFirstGateIndices ??
                        new int[colors.Length],
                    primaryMechanic = mechanic,
                    gateModifiers = modifiers,
                    echoEnabled = enableEcho,
                    firstEchoGuaranteed = true,
                    echoEligibleStart = 0.1f,
                    echoEligibleEnd = 0.5f,
                    firstEchoMin = 0.1f,
                    firstEchoMax = 0.2f,
                    additionalEchoChance = 0.3f,
                    echoCooldownGateCount = 2,
                    echoMaxAcquisitions = enableEcho ? 3 : 0,
                    camouflageRevealLeadTime = 1.25f,
                    camouflageRevealTransition = 0.18f,
                    fogFadeInSeconds = 0.5f,
                    fogFullOpacitySeconds = 5f,
                    fogFadeOutSeconds = 0.5f,
                    iceSpeedMultiplier = GateModifierRules.IceSpeedMultiplier,
                    hiddenEnabled = false,
                    hiddenEligibleStart = 0.15f,
                    hiddenEligibleEnd = 0.85f,
                    hiddenOccurrenceChance = 0.35f,
                    hiddenMinimumGateCooldown = 2,
                    hiddenReadableSeconds = 1f,
                    hiddenLeadSeconds = 0.85f,
                    hiddenTransitionSeconds = 0.12f,
                    hiddenMaxOccurrences = 4,
                    hiddenFirstGuaranteed = true,
                    flickerEnabled = false,
                    flickerEligibleStart = 0.15f,
                    flickerEligibleEnd = 0.85f,
                    flickerOccurrenceChance = 0.30f,
                    flickerMinimumGateCooldown = 2,
                    flickerMaxOccurrences = 4,
                    flickerFirstGuaranteed = true,
                    flickerSwitchSeconds = 0.50f,
                    flickerPulseSeconds = 0.10f,
                    flickerMinimumCyclesVisible = 3,
                    flickerRandomizePhase = true,
                    mechanicGrantEnabled = enableGrant,
                    grantedMechanic = grantMechanic,
                    grantActivationMode = activationMode,
                    grantActivationProgress = activationProgress,
                    grantChargeCount = enableGrant ? 1 : 0
                };
            }

            public StageEntry WithSpeedCurve(params Keyframe[] keys)
            {
                speedCurve = new AnimationCurve(keys);
                return this;
            }

            public StageEntry WithActiveColorCount(int count)
            {
                activeColorCount = count;
                return this;
            }

            public StageEntry WithItemAvailability(
                bool allowShield,
                bool allowBooster)
            {
                shieldAllowed = allowShield;
                boosterAllowed = allowBooster;
                return this;
            }

            public StageEntry WithDifficulty(StageDifficulty value)
            {
                difficulty = value;
                return this;
            }

            public StageEntry WithFogCurtain(float fullOpacitySeconds)
            {
                fogFadeInSeconds = 0.5f;
                fogFullOpacitySeconds = fullOpacitySeconds;
                fogFadeOutSeconds = 0.5f;
                return this;
            }

            public StageEntry WithIceRunway(float speedMultiplier)
            {
                iceSpeedMultiplier = speedMultiplier;
                return this;
            }

            public StageEntry WithHidden(
                float eligibleStart,
                float eligibleEnd,
                float occurrenceChance,
                int minimumGateCooldown,
                float hideLeadSeconds,
                float transitionSeconds,
                int maxOccurrences)
            {
                hiddenEnabled = true;
                hiddenEligibleStart = eligibleStart;
                hiddenEligibleEnd = eligibleEnd;
                hiddenOccurrenceChance = occurrenceChance;
                hiddenMinimumGateCooldown = minimumGateCooldown;
                hiddenLeadSeconds = hideLeadSeconds;
                hiddenTransitionSeconds = transitionSeconds;
                hiddenMaxOccurrences = maxOccurrences;
                hiddenFirstGuaranteed = true;
                return this;
            }

            public StageEntry WithFlicker(
                float eligibleStart,
                float eligibleEnd,
                float occurrenceChance,
                int minimumGateCooldown,
                float switchSeconds,
                float transitionSeconds,
                int maxOccurrences)
            {
                flickerEnabled = true;
                flickerEligibleStart = eligibleStart;
                flickerEligibleEnd = eligibleEnd;
                flickerOccurrenceChance = occurrenceChance;
                flickerMinimumGateCooldown = minimumGateCooldown;
                flickerSwitchSeconds = switchSeconds;
                flickerPulseSeconds = transitionSeconds;
                flickerMaxOccurrences = maxOccurrences;
                flickerFirstGuaranteed = true;
                return this;
            }

            private StageSpeedProfile CreateSpeedProfile()
            {
                if (speedCurve == null || IsLinearCurve(speedCurve))
                {
                    return StageSpeedProfile.Linear(
                        startingSpeed,
                        maximumSpeed);
                }

                float[] samples = new float[CurvedProfileSampleCount];
                for (int index = 0; index < samples.Length; index++)
                {
                    float progress = index / (float)(samples.Length - 1);
                    samples[index] = speedCurve.Evaluate(progress);
                }

                return new StageSpeedProfile(
                    startingSpeed,
                    maximumSpeed,
                    samples);
            }

            private EchoSettings CreateEchoSettings()
            {
                return new EchoSettings(
                    echoEnabled,
                    echoEligibleStart,
                    echoEligibleEnd,
                    firstEchoGuaranteed,
                    firstEchoMin,
                    firstEchoMax,
                    additionalEchoChance,
                    echoCooldownGateCount,
                    echoMaxAcquisitions);
            }

            private FogCurtainSettings CreateFogCurtainSettings()
            {
                return new FogCurtainSettings(
                    fogFadeInSeconds,
                    fogFullOpacitySeconds,
                    fogFadeOutSeconds);
            }

            private HiddenSettings CreateHiddenSettings()
            {
                return new HiddenSettings(
                    hiddenEnabled,
                    hiddenEligibleStart,
                    hiddenEligibleEnd,
                    hiddenOccurrenceChance,
                    hiddenMinimumGateCooldown,
                    hiddenReadableSeconds,
                    hiddenLeadSeconds,
                    hiddenTransitionSeconds,
                    hiddenMaxOccurrences,
                    hiddenFirstGuaranteed);
            }

            private FlickerSettings CreateFlickerSettings()
            {
                return new FlickerSettings(
                    flickerEnabled,
                    flickerEligibleStart,
                    flickerEligibleEnd,
                    flickerOccurrenceChance,
                    flickerMinimumGateCooldown,
                    flickerMaxOccurrences,
                    flickerFirstGuaranteed,
                    flickerSwitchSeconds,
                    flickerPulseSeconds,
                    flickerMinimumCyclesVisible,
                    flickerRandomizePhase);
            }

            private int GetActiveColorCount()
            {
                if (allowedColors == null || allowedColors.Length < 2)
                {
                    return allowedColors == null ? 0 : allowedColors.Length;
                }

                int configured = activeColorCount <= 0
                    ? allowedColors.Length
                    : activeColorCount;
                return Mathf.Clamp(configured, 2, allowedColors.Length);
            }

            private static T[] CopyFirst<T>(T[] source, int count)
            {
                if (source == null)
                {
                    return null;
                }
                if (source.Length == count)
                {
                    return (T[])source.Clone();
                }

                T[] result = new T[count];
                Array.Copy(source, result, Math.Min(source.Length, count));
                return result;
            }

            private static bool IsLinearCurve(AnimationCurve curve)
            {
                Keyframe[] keys = curve.keys;
                return keys.Length == 2 &&
                    Mathf.Approximately(keys[0].time, 0f) &&
                    Mathf.Approximately(keys[0].value, 0f) &&
                    Mathf.Approximately(keys[0].outTangent, 1f) &&
                    Mathf.Approximately(keys[1].time, 1f) &&
                    Mathf.Approximately(keys[1].value, 1f) &&
                    Mathf.Approximately(keys[1].inTangent, 1f);
            }
        }

        [SerializeField, HideInInspector] private int catalogRevision;
        [SerializeField] private StageEntry[] stages = Array.Empty<StageEntry>();

        public int Count => stages == null ? 0 : stages.Length;
        public bool RequiresUpgrade =>
            catalogRevision < CurrentCatalogRevision;

        public IStageCatalog BuildCatalog()
        {
            if (stages == null || stages.Length == 0)
            {
                throw new InvalidOperationException(
                    "StageCatalogAsset contains no stages.");
            }

            StageDefinition[] definitions =
                new StageDefinition[stages.Length];
            for (int index = 0; index < stages.Length; index++)
            {
                definitions[index] = stages[index].ToCore();
                if (!definitions[index].IsValid())
                {
                    throw new InvalidOperationException(
                        $"Stage {index + 1} is not valid.");
                }
            }

            return new InMemoryStageCatalog(definitions);
        }

        public void InitializeLegacyStages()
        {
            catalogRevision = CurrentCatalogRevision;
            stages = new[]
            {
                StageEntry.Create(
                    "stage-01", 1, "TWO-COLOR BASICS",
                    "Learn the red and blue match.",
                    24, new[] { RunnerColor.Red, RunnerColor.Blue },
                    28f, 40f, 1.55f, 1.25f,
                    new[] { GatePatternType.Steady, GatePatternType.Release },
                    6, 4, 320f, 88f, 10101u)
                    .WithItemAvailability(false, false),
                StageEntry.Create(
                    "stage-02", 2, "RHYTHM CONTRAST",
                    "Read changing gate cadence.",
                    28, new[] { RunnerColor.Red, RunnerColor.Blue },
                    32f, 44f, 1.45f, 1.10f,
                    new[]
                    {
                        GatePatternType.Steady,
                        GatePatternType.Compression,
                        GatePatternType.Release,
                        GatePatternType.Syncopation
                    },
                    5, 5, 360f, 92f, 20202u)
                    .WithItemAvailability(false, false),
                StageEntry.Create(
                    "stage-03", 3, "EXCEPTION COLOR",
                    "Watch for deliberate color exceptions.",
                    30, new[] { RunnerColor.Red, RunnerColor.Blue },
                    34f, 46f, 1.40f, 1.00f,
                    new[]
                    {
                        GatePatternType.Steady,
                        GatePatternType.SameColorBait,
                        GatePatternType.SingleColorBreak,
                        GatePatternType.Burst
                    },
                    5, 6, 400f, 96f, 30303u)
                    .WithItemAvailability(false, false),
                StageEntry.Create(
                    "stage-04", 4, "GREEN INTRODUCTION",
                    "Meet green safely, then use all three colors.",
                    32, new[]
                    {
                        RunnerColor.Red,
                        RunnerColor.Blue,
                        RunnerColor.Green
                    },
                    32f, 48f, 1.45f, 1.00f,
                    new[]
                    {
                        GatePatternType.Steady,
                        GatePatternType.ThirdColorTutorial,
                        GatePatternType.Compression,
                        GatePatternType.Release
                    },
                    8, 6, 420f, 100f, 40404u,
                    true, new[] { 0, 0, 8 })
                    .WithItemAvailability(false, false),
                StageEntry.Create(
                    "stage-05", 5, "THREE-COLOR CHALLENGE",
                    "Master three colors under pressure.",
                    36, new[]
                    {
                        RunnerColor.Red,
                        RunnerColor.Blue,
                        RunnerColor.Green
                    },
                    36f, 54f, 1.25f, 0.85f,
                    new[]
                    {
                        GatePatternType.ThreeColorFlow,
                        GatePatternType.Compression,
                        GatePatternType.Syncopation,
                        GatePatternType.SameColorBait,
                        GatePatternType.SingleColorBreak
                    },
                    4, 8, 460f, 104f, 50505u)
                    .WithItemAvailability(false, false),
                StageEntry.Create(
                    "stage-06", 6, "SHIELD TRAINING",
                    "Learn one stage-local Shield charge.",
                    38, new[]
                    {
                        RunnerColor.Red,
                        RunnerColor.Blue,
                        RunnerColor.Green
                    },
                    38f, 56f, 1.22f, 0.82f,
                    new[]
                    {
                        GatePatternType.ThreeColorFlow,
                        GatePatternType.Compression,
                        GatePatternType.Release
                    },
                    5, 8, 480f, 108f, 60606u,
                    true, null,
                    StagePrimaryMechanic.Shield,
                    GateModifierType.None,
                    false, true,
                    StageMechanicGrantMechanic.Shield,
                    StageMechanicActivationMode.ActiveAtStageStart)
                    .WithSpeedCurve(
                        new Keyframe(0f, 0f),
                        new Keyframe(0.35f, 0.22f),
                        new Keyframe(0.70f, 0.62f),
                        new Keyframe(1f, 1f))
                    .WithActiveColorCount(2)
                    .WithItemAvailability(false, false),
                StageEntry.Create(
                    "stage-07", 7, "BOOSTER TIMING",
                    "Receive a stage-local Booster from the start.",
                    40, new[]
                    {
                        RunnerColor.Red,
                        RunnerColor.Blue,
                        RunnerColor.Green
                    },
                    40f, 58f, 1.18f, 0.80f,
                    new[]
                    {
                        GatePatternType.ThreeColorFlow,
                        GatePatternType.Syncopation,
                        GatePatternType.Release
                    },
                    5, 8, 500f, 110f, 70707u,
                    true, null,
                    StagePrimaryMechanic.Booster,
                    GateModifierType.None,
                    false, true,
                    StageMechanicGrantMechanic.Booster,
                    StageMechanicActivationMode.ActiveAtStageStart)
                    .WithSpeedCurve(
                        new Keyframe(0f, 0f),
                        new Keyframe(0.30f, 0.18f),
                        new Keyframe(0.65f, 0.55f),
                        new Keyframe(1f, 1f))
                    .WithActiveColorCount(2)
                    .WithItemAvailability(false, false),
                StageEntry.Create(
                    "stage-08", 8, "ITEM APPLICATION",
                    "Apply an available item in a clean three-color run.",
                    40, new[]
                    {
                        RunnerColor.Red,
                        RunnerColor.Blue,
                        RunnerColor.Green
                    },
                    38f, 56f, 1.22f, 0.84f,
                    new[]
                    {
                        GatePatternType.ThreeColorFlow,
                        GatePatternType.Steady,
                        GatePatternType.Release
                    },
                    6, 8, 520f, 112f, 80808u)
                    .WithSpeedCurve(
                        new Keyframe(0f, 0f),
                        new Keyframe(0.45f, 0.30f),
                        new Keyframe(0.78f, 0.70f),
                        new Keyframe(1f, 1f)),
                StageEntry.Create(
                    "stage-09", 9, "CAMOUFLAGE INTRO",
                    "Learn the reveal timing with two colors.",
                    38, new[]
                    {
                        RunnerColor.Red,
                        RunnerColor.Blue,
                        RunnerColor.Green
                    },
                    38f, 54f, 1.24f, 0.90f,
                    new[]
                    {
                        GatePatternType.Steady,
                        GatePatternType.Release
                    },
                    10, 6, 540f, 114f, 90909u,
                    true, null,
                    StagePrimaryMechanic.Camouflage,
                    GateModifierType.Camouflage)
                    .WithSpeedCurve(
                        new Keyframe(0f, 0f),
                        new Keyframe(0.45f, 0.28f),
                        new Keyframe(0.78f, 0.70f),
                        new Keyframe(1f, 1f))
                    .WithActiveColorCount(2),
                StageEntry.Create(
                    "stage-10", 10, "CAMOUFLAGE PRACTICE",
                    "Practice concealed gates through a longer two-color run.",
                    42, new[]
                    {
                        RunnerColor.Red,
                        RunnerColor.Blue,
                        RunnerColor.Green
                    },
                    40f, 58f, 1.18f, 0.84f,
                    new[]
                    {
                        GatePatternType.ThreeColorFlow,
                        GatePatternType.Compression,
                        GatePatternType.Release
                    },
                    7, 8, 560f, 116f, 100010u,
                    true, null,
                    StagePrimaryMechanic.Camouflage,
                    GateModifierType.Camouflage)
                    .WithSpeedCurve(
                        new Keyframe(0f, 0f),
                        new Keyframe(0.42f, 0.27f),
                        new Keyframe(0.75f, 0.66f),
                        new Keyframe(1f, 1f))
                    .WithActiveColorCount(2),
                StageEntry.Create(
                    "stage-11", 11, "CAMOUFLAGE MASTERY",
                    "Master concealed gates after restoring three colors.",
                    46, new[]
                    {
                        RunnerColor.Red,
                        RunnerColor.Blue,
                        RunnerColor.Green
                    },
                    42f, 62f, 1.12f, 0.80f,
                    new[]
                    {
                        GatePatternType.ThreeColorFlow,
                        GatePatternType.SameColorBait,
                        GatePatternType.Release
                    },
                    6, 10, 580f, 118f, 110011u,
                    true, null,
                    StagePrimaryMechanic.Camouflage,
                    GateModifierType.Camouflage)
                    .WithSpeedCurve(
                        new Keyframe(0f, 0f),
                        new Keyframe(0.38f, 0.24f),
                        new Keyframe(0.72f, 0.64f),
                        new Keyframe(1f, 1f))
                    .WithDifficulty(StageDifficulty.Hard),
                StageEntry.Create(
                    "stage-12", 12, "FOG INTRO",
                    "Learn to read the nearest gates through Fog.",
                    40, new[]
                    {
                        RunnerColor.Red,
                        RunnerColor.Blue,
                        RunnerColor.Green
                    },
                    40f, 56f, 1.22f, 0.88f,
                    new[]
                    {
                        GatePatternType.Steady,
                        GatePatternType.Release
                    },
                    8, 6, 600f, 120f, 120012u,
                    true, null,
                    StagePrimaryMechanic.Fog,
                    GateModifierType.Fog)
                    .WithSpeedCurve(
                        new Keyframe(0f, 0f),
                        new Keyframe(0.45f, 0.28f),
                        new Keyframe(0.78f, 0.70f),
                        new Keyframe(1f, 1f))
                    .WithActiveColorCount(2)
                    .WithFogCurtain(5f),
                StageEntry.Create(
                    "stage-13", 13, "FOG PRACTICE",
                    "Practice planning a two-color path through Fog.",
                    44, new[]
                    {
                        RunnerColor.Red,
                        RunnerColor.Blue,
                        RunnerColor.Green
                    },
                    42f, 60f, 1.16f, 0.82f,
                    new[]
                    {
                        GatePatternType.ThreeColorFlow,
                        GatePatternType.Syncopation,
                        GatePatternType.Release
                    },
                    7, 8, 620f, 122f, 130013u,
                    true, null,
                    StagePrimaryMechanic.Fog,
                    GateModifierType.Fog)
                    .WithSpeedCurve(
                        new Keyframe(0f, 0f),
                        new Keyframe(0.42f, 0.27f),
                        new Keyframe(0.75f, 0.66f),
                        new Keyframe(1f, 1f))
                    .WithActiveColorCount(2)
                    .WithFogCurtain(6f),
                StageEntry.Create(
                    "stage-14", 14, "FOG MASTERY",
                    "Master Fog after restoring three colors.",
                    48, new[]
                    {
                        RunnerColor.Red,
                        RunnerColor.Blue,
                        RunnerColor.Green
                    },
                    44f, 64f, 1.10f, 0.80f,
                    new[]
                    {
                        GatePatternType.ThreeColorFlow,
                        GatePatternType.SingleColorBreak,
                        GatePatternType.Release
                    },
                    6, 10, 640f, 124f, 140014u,
                    true, null,
                    StagePrimaryMechanic.Fog,
                    GateModifierType.Fog)
                    .WithSpeedCurve(
                        new Keyframe(0f, 0f),
                        new Keyframe(0.38f, 0.24f),
                        new Keyframe(0.72f, 0.64f),
                        new Keyframe(1f, 1f))
                    .WithDifficulty(StageDifficulty.Hard)
                    .WithFogCurtain(7f),
                StageEntry.Create(
                    "stage-15", 15, "ICE INTRO",
                    "Learn Ice momentum with two colors.",
                    42, new[]
                    {
                        RunnerColor.Red,
                        RunnerColor.Blue,
                        RunnerColor.Green
                    },
                    40f, 56f, 1.20f, 0.88f,
                    new[]
                    {
                        GatePatternType.Steady,
                        GatePatternType.Release
                    },
                    8, 6, 660f, 126f, 150015u,
                    true, null,
                    StagePrimaryMechanic.Ice,
                    GateModifierType.Ice)
                    .WithSpeedCurve(
                        new Keyframe(0f, 0f),
                        new Keyframe(0.45f, 0.28f),
                        new Keyframe(0.78f, 0.70f),
                        new Keyframe(1f, 1f))
                    .WithActiveColorCount(2)
                    .WithIceRunway(2f),
                StageEntry.Create(
                    "stage-16", 16, "ICE PRACTICE",
                    "Practice controlling Ice through a longer run.",
                    46, new[]
                    {
                        RunnerColor.Red,
                        RunnerColor.Blue,
                        RunnerColor.Green
                    },
                    42f, 60f, 1.14f, 0.82f,
                    new[]
                    {
                        GatePatternType.ThreeColorFlow,
                        GatePatternType.Compression,
                        GatePatternType.Release
                    },
                    7, 8, 680f, 128f, 160016u,
                    true, null,
                    StagePrimaryMechanic.Ice,
                    GateModifierType.Ice)
                    .WithSpeedCurve(
                        new Keyframe(0f, 0f),
                        new Keyframe(0.42f, 0.27f),
                        new Keyframe(0.75f, 0.66f),
                        new Keyframe(1f, 1f))
                    .WithActiveColorCount(2)
                    .WithIceRunway(2f),
                StageEntry.Create(
                    "stage-17", 17, "ICE MASTERY",
                    "Master Ice with three-color pressure.",
                    50, new[]
                    {
                        RunnerColor.Red,
                        RunnerColor.Blue,
                        RunnerColor.Green
                    },
                    44f, 64f, 1.08f, 0.78f,
                    new[]
                    {
                        GatePatternType.ThreeColorFlow,
                        GatePatternType.Syncopation,
                        GatePatternType.Burst
                    },
                    6, 10, 700f, 130f, 170017u,
                    true, null,
                    StagePrimaryMechanic.Ice,
                    GateModifierType.Ice)
                    .WithSpeedCurve(
                        new Keyframe(0f, 0f),
                        new Keyframe(0.38f, 0.24f),
                        new Keyframe(0.72f, 0.64f),
                        new Keyframe(1f, 1f))
                    .WithDifficulty(StageDifficulty.Hard)
                    .WithIceRunway(2f),
                StageEntry.Create(
                    "stage-18", 18, "ECHO INTRO",
                    "Learn to acquire and spend Echo with two colors.",
                    42, new[]
                    {
                        RunnerColor.Red,
                        RunnerColor.Blue,
                        RunnerColor.Green
                    },
                    42f, 58f, 1.20f, 0.86f,
                    new[]
                    {
                        GatePatternType.Steady,
                        GatePatternType.Release
                    },
                    8, 6, 720f, 132f, 180018u,
                    true, null,
                    StagePrimaryMechanic.Echo,
                    GateModifierType.EchoProvider,
                    true)
                    .WithSpeedCurve(
                        new Keyframe(0f, 0f),
                        new Keyframe(0.45f, 0.28f),
                        new Keyframe(0.78f, 0.70f),
                        new Keyframe(1f, 1f))
                    .WithActiveColorCount(2),
                StageEntry.Create(
                    "stage-19", 19, "ECHO PRACTICE",
                    "Practice Echo timing through a longer two-color run.",
                    46, new[]
                    {
                        RunnerColor.Red,
                        RunnerColor.Blue,
                        RunnerColor.Green
                    },
                    44f, 62f, 1.14f, 0.80f,
                    new[]
                    {
                        GatePatternType.ThreeColorFlow,
                        GatePatternType.SameColorBait,
                        GatePatternType.Release
                    },
                    7, 8, 740f, 134f, 190019u,
                    true, null,
                    StagePrimaryMechanic.Echo,
                    GateModifierType.EchoProvider,
                    true)
                    .WithSpeedCurve(
                        new Keyframe(0f, 0f),
                        new Keyframe(0.42f, 0.27f),
                        new Keyframe(0.75f, 0.66f),
                        new Keyframe(1f, 1f))
                    .WithActiveColorCount(2),
                StageEntry.Create(
                    "stage-20", 20, "ECHO MASTERY",
                    "Master Echo decisions with three-color pressure.",
                    50, new[]
                    {
                        RunnerColor.Red,
                        RunnerColor.Blue,
                        RunnerColor.Green
                    },
                    46f, 66f, 1.08f, 0.78f,
                    new[]
                    {
                        GatePatternType.ThreeColorFlow,
                        GatePatternType.Syncopation,
                        GatePatternType.SingleColorBreak
                    },
                    6, 10, 760f, 136f, 200020u,
                    true, null,
                    StagePrimaryMechanic.Echo,
                    GateModifierType.EchoProvider,
                    true)
                    .WithSpeedCurve(
                        new Keyframe(0f, 0f),
                        new Keyframe(0.38f, 0.24f),
                        new Keyframe(0.72f, 0.64f),
                        new Keyframe(1f, 1f))
                    .WithDifficulty(StageDifficulty.VeryHard),
                StageEntry.Create(
                    "stage-21", 21, "HIDDEN INTRO",
                    "Learn to remember a gate before its color disappears.",
                    42, new[]
                    {
                        RunnerColor.Red,
                        RunnerColor.Blue,
                        RunnerColor.Green
                    },
                    42f, 58f, 1.20f, 0.86f,
                    new[]
                    {
                        GatePatternType.Steady,
                        GatePatternType.Release
                    },
                    8, 6, 780f, 138f, 210021u,
                    true, null,
                    StagePrimaryMechanic.Hidden,
                    GateModifierType.Hidden)
                    .WithSpeedCurve(
                        new Keyframe(0f, 0f),
                        new Keyframe(0.45f, 0.28f),
                        new Keyframe(0.78f, 0.70f),
                        new Keyframe(1f, 1f))
                    .WithActiveColorCount(2)
                    .WithHidden(
                        0.10f, 0.92f, 0.40f, 2,
                        1.30f, 0.18f, 7),
                StageEntry.Create(
                    "stage-22", 22, "HIDDEN PRACTICE",
                    "Practice recalling hidden gates through a longer run.",
                    46, new[]
                    {
                        RunnerColor.Red,
                        RunnerColor.Blue,
                        RunnerColor.Green
                    },
                    44f, 62f, 1.14f, 0.80f,
                    new[]
                    {
                        GatePatternType.ThreeColorFlow,
                        GatePatternType.Compression,
                        GatePatternType.Release
                    },
                    7, 8, 800f, 140f, 220022u,
                    true, null,
                    StagePrimaryMechanic.Hidden,
                    GateModifierType.Hidden)
                    .WithSpeedCurve(
                        new Keyframe(0f, 0f),
                        new Keyframe(0.42f, 0.27f),
                        new Keyframe(0.75f, 0.66f),
                        new Keyframe(1f, 1f))
                    .WithActiveColorCount(2)
                    .WithHidden(
                        0.10f, 0.94f, 0.40f, 2,
                        1.15f, 0.18f, 9),
                StageEntry.Create(
                    "stage-23", 23, "HIDDEN MASTERY",
                    "Master three-color recall while gates hide their targets.",
                    50, new[]
                    {
                        RunnerColor.Red,
                        RunnerColor.Blue,
                        RunnerColor.Green
                    },
                    46f, 66f, 1.08f, 0.78f,
                    new[]
                    {
                        GatePatternType.ThreeColorFlow,
                        GatePatternType.Syncopation,
                        GatePatternType.Release
                    },
                    6, 10, 820f, 142f, 230023u,
                    true, null,
                    StagePrimaryMechanic.Hidden,
                    GateModifierType.Hidden)
                    .WithSpeedCurve(
                        new Keyframe(0f, 0f),
                        new Keyframe(0.38f, 0.24f),
                        new Keyframe(0.72f, 0.64f),
                        new Keyframe(1f, 1f))
                    .WithDifficulty(StageDifficulty.Hard)
                    .WithHidden(
                        0.10f, 0.96f, 0.45f, 2,
                        1.00f, 0.18f, 11),
                StageEntry.Create(
                    "stage-24", 24, "FLICKER INTRO",
                    "Read a gate whose neon color changes as you approach.",
                    44, new[]
                    {
                        RunnerColor.Red,
                        RunnerColor.Blue,
                        RunnerColor.Green
                    },
                    44f, 60f, 1.18f, 0.84f,
                    new[]
                    {
                        GatePatternType.Steady,
                        GatePatternType.Compression,
                        GatePatternType.Release
                    },
                    8, 7, 840f, 144f, 240024u,
                    true, null,
                    StagePrimaryMechanic.Flicker,
                    GateModifierType.Flicker)
                    .WithSpeedCurve(
                        new Keyframe(0f, 0f),
                        new Keyframe(0.44f, 0.28f),
                        new Keyframe(0.77f, 0.69f),
                        new Keyframe(1f, 1f))
                    .WithActiveColorCount(2)
                    .WithItemAvailability(true, false)
                    .WithFlicker(
                        0.10f, 0.92f, 0.22f, 2,
                        0.90f, 0.24f, 6),
                StageEntry.Create(
                    "stage-25", 25, "FLICKER PRACTICE",
                    "Track faster color changes through a longer approach.",
                    48, new[]
                    {
                        RunnerColor.Red,
                        RunnerColor.Blue,
                        RunnerColor.Green
                    },
                    46f, 64f, 1.12f, 0.80f,
                    new[]
                    {
                        GatePatternType.ThreeColorFlow,
                        GatePatternType.Compression,
                        GatePatternType.Release
                    },
                    7, 8, 860f, 146f, 250025u,
                    true, null,
                    StagePrimaryMechanic.Flicker,
                    GateModifierType.Flicker)
                    .WithSpeedCurve(
                        new Keyframe(0f, 0f),
                        new Keyframe(0.42f, 0.26f),
                        new Keyframe(0.75f, 0.66f),
                        new Keyframe(1f, 1f))
                    .WithActiveColorCount(2)
                    .WithItemAvailability(true, false)
                    .WithFlicker(
                        0.10f, 0.94f, 0.30f, 2,
                        0.78f, 0.24f, 8),
                StageEntry.Create(
                    "stage-26", 26, "FLICKER MASTERY",
                    "Master three-color gates with compressed neon changes.",
                    52, new[]
                    {
                        RunnerColor.Red,
                        RunnerColor.Blue,
                        RunnerColor.Green
                    },
                    48f, 68f, 1.06f, 0.76f,
                    new[]
                    {
                        GatePatternType.ThreeColorFlow,
                        GatePatternType.Syncopation,
                        GatePatternType.Release
                    },
                    6, 10, 880f, 148f, 260026u,
                    true, null,
                    StagePrimaryMechanic.Flicker,
                    GateModifierType.Flicker)
                    .WithSpeedCurve(
                        new Keyframe(0f, 0f),
                        new Keyframe(0.38f, 0.24f),
                        new Keyframe(0.72f, 0.64f),
                        new Keyframe(1f, 1f))
                    .WithDifficulty(StageDifficulty.Hard)
                    .WithItemAvailability(true, false)
                    .WithFlicker(
                        0.10f, 0.96f, 0.46f, 2,
                        0.66f, 0.24f, 10)
            };
        }
    }

    public static class StageCatalogProvider
    {
        public const string ResourceName = "StageCatalog";

        public static void Configure(StageCatalogAsset asset)
        {
            if (asset == null)
            {
                throw new ArgumentNullException(nameof(asset));
            }

            StageCatalog.Configure(asset.BuildCatalog());
        }

        public static void EnsureConfigured()
        {
            if (StageCatalog.IsConfigured)
            {
                return;
            }

            StageCatalogAsset asset =
                Resources.Load<StageCatalogAsset>(ResourceName);
            if (asset == null)
            {
                throw new InvalidOperationException(
                    $"Resources/{ResourceName}.asset is missing.");
            }

            Configure(asset);
        }
    }
}

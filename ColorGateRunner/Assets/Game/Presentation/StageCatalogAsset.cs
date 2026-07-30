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
        private const int CurrentCatalogRevision = 4;

        [Serializable]
        private sealed class StageEntry
        {
            [SerializeField] private string stageId;
            [SerializeField] private int displayNumber;
            [SerializeField] private string title;
            [SerializeField, TextArea] private string description;
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
                        true));
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
                    .WithActiveColorCount(2),
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
                    .WithActiveColorCount(2),
                StageEntry.Create(
                    "stage-08", 8, "CAMOUFLAGE READ",
                    "Hidden gates reveal from time-to-arrival.",
                    42, new[]
                    {
                        RunnerColor.Red,
                        RunnerColor.Blue,
                        RunnerColor.Green
                    },
                    40f, 60f, 1.16f, 0.78f,
                    new[]
                    {
                        GatePatternType.ThreeColorFlow,
                        GatePatternType.Compression,
                        GatePatternType.Release
                    },
                    6, 9, 500f, 112f, 80808u,
                    true, null,
                    StagePrimaryMechanic.Camouflage,
                    GateModifierType.Camouflage)
                    .WithSpeedCurve(
                        new Keyframe(0f, 0f),
                        new Keyframe(0.40f, 0.25f),
                        new Keyframe(0.75f, 0.70f),
                        new Keyframe(1f, 1f))
                    .WithActiveColorCount(2),
                StageEntry.Create(
                    "stage-09", 9, "FOG SIGHTLINE",
                    "Read only the nearest two gates through Fog.",
                    44, new[]
                    {
                        RunnerColor.Red,
                        RunnerColor.Blue,
                        RunnerColor.Green
                    },
                    42f, 62f, 1.12f, 0.76f,
                    new[]
                    {
                        GatePatternType.ThreeColorFlow,
                        GatePatternType.Syncopation,
                        GatePatternType.SameColorBait
                    },
                    6, 9, 520f, 114f, 90909u,
                    true, null,
                    StagePrimaryMechanic.Fog,
                    GateModifierType.Fog)
                    .WithActiveColorCount(2),
                StageEntry.Create(
                    "stage-10", 10, "ICE MOMENTUM",
                    "Ice raises movement speed while preserving reaction time.",
                    46, new[]
                    {
                        RunnerColor.Red,
                        RunnerColor.Blue,
                        RunnerColor.Green
                    },
                    44f, 64f, 1.08f, 0.76f,
                    new[]
                    {
                        GatePatternType.ThreeColorFlow,
                        GatePatternType.Compression,
                        GatePatternType.Release
                    },
                    6, 10, 540f, 116f, 100010u,
                    true, null,
                    StagePrimaryMechanic.Ice,
                    GateModifierType.Ice)
                    .WithSpeedCurve(
                        new Keyframe(0f, 0f),
                        new Keyframe(0.50f, 0.35f),
                        new Keyframe(0.80f, 0.75f),
                        new Keyframe(1f, 1f))
                    .WithActiveColorCount(2),
                StageEntry.Create(
                    "stage-11", 11, "ECHO MASTERY",
                    "Acquire and spend one-color Echo protection.",
                    48, new[]
                    {
                        RunnerColor.Red,
                        RunnerColor.Blue,
                        RunnerColor.Green
                    },
                    44f, 66f, 1.05f, 0.76f,
                    new[]
                    {
                        GatePatternType.ThreeColorFlow,
                        GatePatternType.Syncopation,
                        GatePatternType.SingleColorBreak
                    },
                    6, 10, 560f, 118f, 110011u,
                    true, null,
                    StagePrimaryMechanic.Echo,
                    GateModifierType.EchoProvider,
                    true)
                    .WithSpeedCurve(
                        new Keyframe(0f, 0f),
                        new Keyframe(0.45f, 0.30f),
                        new Keyframe(0.75f, 0.68f),
                        new Keyframe(1f, 1f))
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

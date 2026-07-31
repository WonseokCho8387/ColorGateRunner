using System;

namespace ColorGateRunner.Core
{
    public static class GateModifierRules
    {
        public const float IceSpeedMultiplier = 1.45f;
        public const float IceSpacingMultiplier = 1.30f;

        public static GateModifier CreateForStageGate(
            StageDefinition stage,
            int gateIndex)
        {
            if (stage == null)
            {
                throw new ArgumentNullException(nameof(stage));
            }
            if (gateIndex < 0)
            {
                throw new ArgumentOutOfRangeException(nameof(gateIndex));
            }

            GateModifierType types = GateModifierType.None;
            float progress = stage.TargetGateCount <= 1
                ? 1f
                : Math.Min(
                    1f,
                    gateIndex / (float)(stage.TargetGateCount - 1));

            if ((stage.GateModifiers & GateModifierType.Camouflage) != 0 &&
                gateIndex >= stage.IntroGateCount &&
                gateIndex % 3 == 2)
            {
                types |= GateModifierType.Camouflage;
            }
            if ((stage.GateModifiers & GateModifierType.Fog) != 0 &&
                progress >= 0.25f &&
                progress <= 0.65f)
            {
                types |= GateModifierType.Fog;
            }
            if ((stage.GateModifiers & GateModifierType.Ice) != 0 &&
                progress >= 0.30f &&
                progress <= 0.60f)
            {
                types |= GateModifierType.Ice;
            }

            return new GateModifier(types);
        }
    }

    [Flags]
    public enum GateModifierType
    {
        None = 0,
        Camouflage = 1 << 0,
        Fog = 1 << 1,
        Ice = 1 << 2,
        EchoProvider = 1 << 3,
        Flicker = 1 << 4
    }

    public readonly struct GateModifier
    {
        public GateModifier(GateModifierType types)
        {
            Types = types;
        }

        public GateModifierType Types { get; }
        public bool IsNone => Types == GateModifierType.None;
        public bool IsCamouflage => Has(GateModifierType.Camouflage);
        public bool IsFog => Has(GateModifierType.Fog);
        public bool IsIce => Has(GateModifierType.Ice);
        public bool IsEchoProvider => Has(GateModifierType.EchoProvider);
        public bool IsFlicker => Has(GateModifierType.Flicker);

        public static GateModifier None =>
            new GateModifier(GateModifierType.None);

        public bool Has(GateModifierType type)
        {
            return (Types & type) == type;
        }

        public GateModifier With(GateModifierType type)
        {
            return new GateModifier(Types | type);
        }
    }

    public sealed class EchoSettings
    {
        public EchoSettings(
            bool enabled,
            float eligibleStartProgress,
            float eligibleEndProgress,
            bool firstEchoGuaranteed,
            float firstEchoMinProgress,
            float firstEchoMaxProgress,
            float additionalEchoChance,
            int cooldownGateCount,
            int maxAcquisitions)
        {
            Enabled = enabled;
            EligibleStartProgress = eligibleStartProgress;
            EligibleEndProgress = eligibleEndProgress;
            FirstEchoGuaranteed = firstEchoGuaranteed;
            FirstEchoMinProgress = firstEchoMinProgress;
            FirstEchoMaxProgress = firstEchoMaxProgress;
            AdditionalEchoChance = additionalEchoChance;
            CooldownGateCount = cooldownGateCount;
            MaxAcquisitions = maxAcquisitions;
            Validate();
        }

        public bool Enabled { get; }
        public float EligibleStartProgress { get; }
        public float EligibleEndProgress { get; }
        public bool FirstEchoGuaranteed { get; }
        public float FirstEchoMinProgress { get; }
        public float FirstEchoMaxProgress { get; }
        public float AdditionalEchoChance { get; }
        public int CooldownGateCount { get; }
        public int MaxAcquisitions { get; }

        public static EchoSettings Disabled()
        {
            return new EchoSettings(
                false, 0.1f, 0.5f, false, 0.1f, 0.2f, 0f, 0, 0);
        }

        public static EchoSettings CreateDefault()
        {
            return new EchoSettings(
                true, 0.1f, 0.5f, true, 0.1f, 0.2f, 0.3f, 2, 3);
        }

        private void Validate()
        {
            ValidateProgress(
                EligibleStartProgress,
                nameof(EligibleStartProgress));
            ValidateProgress(
                EligibleEndProgress,
                nameof(EligibleEndProgress));
            ValidateProgress(
                FirstEchoMinProgress,
                nameof(FirstEchoMinProgress));
            ValidateProgress(
                FirstEchoMaxProgress,
                nameof(FirstEchoMaxProgress));
            if (EligibleStartProgress >= EligibleEndProgress)
            {
                throw new ArgumentException(
                    "Echo eligible start must be before eligible end.");
            }
            if (FirstEchoMinProgress > FirstEchoMaxProgress ||
                FirstEchoMinProgress < EligibleStartProgress ||
                FirstEchoMaxProgress > EligibleEndProgress)
            {
                throw new ArgumentException(
                    "First Echo range must be ordered inside the eligible range.");
            }
            if (AdditionalEchoChance < 0f || AdditionalEchoChance > 1f)
            {
                throw new ArgumentOutOfRangeException(
                    nameof(AdditionalEchoChance));
            }
            if (CooldownGateCount < 0)
            {
                throw new ArgumentOutOfRangeException(
                    nameof(CooldownGateCount));
            }
            if (MaxAcquisitions < 0)
            {
                throw new ArgumentOutOfRangeException(
                    nameof(MaxAcquisitions));
            }
        }

        private static void ValidateProgress(float value, string name)
        {
            if (value < 0f || value > 1f)
            {
                throw new ArgumentOutOfRangeException(name);
            }
        }
    }

    public sealed class CamouflageSettings
    {
        public CamouflageSettings(
            float revealLeadTimeSeconds,
            float revealTransitionSeconds)
        {
            if (revealLeadTimeSeconds <= 0f)
            {
                throw new ArgumentOutOfRangeException(
                    nameof(revealLeadTimeSeconds));
            }
            if (revealTransitionSeconds < 0f)
            {
                throw new ArgumentOutOfRangeException(
                    nameof(revealTransitionSeconds));
            }

            RevealLeadTimeSeconds = revealLeadTimeSeconds;
            RevealTransitionSeconds = revealTransitionSeconds;
        }

        public float RevealLeadTimeSeconds { get; }
        public float RevealTransitionSeconds { get; }

        public static CamouflageSettings CreateDefault()
        {
            return new CamouflageSettings(1.25f, 0.18f);
        }
    }

    public sealed class FlickerSettings
    {
        public FlickerSettings(
            bool enabled,
            float eligibleStartProgress,
            float eligibleEndProgress,
            float occurrenceChance,
            int minimumGateCooldown,
            float revealDurationSeconds,
            float hideLeadTimeSeconds,
            float transitionSeconds,
            int maxOccurrences,
            bool firstOccurrenceGuaranteed)
        {
            ValidateProgress(
                eligibleStartProgress,
                nameof(eligibleStartProgress));
            ValidateProgress(
                eligibleEndProgress,
                nameof(eligibleEndProgress));
            if (eligibleStartProgress >= eligibleEndProgress)
            {
                throw new ArgumentException(
                    "Flicker eligible start must be before eligible end.");
            }
            ValidateProgress(occurrenceChance, nameof(occurrenceChance));
            if (minimumGateCooldown < 0)
            {
                throw new ArgumentOutOfRangeException(
                    nameof(minimumGateCooldown));
            }
            if (revealDurationSeconds < 0f)
            {
                throw new ArgumentOutOfRangeException(
                    nameof(revealDurationSeconds));
            }
            if (hideLeadTimeSeconds < 0f)
            {
                throw new ArgumentOutOfRangeException(
                    nameof(hideLeadTimeSeconds));
            }
            if (transitionSeconds < 0f)
            {
                throw new ArgumentOutOfRangeException(
                    nameof(transitionSeconds));
            }
            if (maxOccurrences < 0)
            {
                throw new ArgumentOutOfRangeException(
                    nameof(maxOccurrences));
            }

            Enabled = enabled;
            EligibleStartProgress = eligibleStartProgress;
            EligibleEndProgress = eligibleEndProgress;
            OccurrenceChance = occurrenceChance;
            MinimumGateCooldown = minimumGateCooldown;
            RevealDurationSeconds = revealDurationSeconds;
            HideLeadTimeSeconds = hideLeadTimeSeconds;
            TransitionSeconds = transitionSeconds;
            MaxOccurrences = maxOccurrences;
            FirstOccurrenceGuaranteed = firstOccurrenceGuaranteed;
        }

        public bool Enabled { get; }
        public float EligibleStartProgress { get; }
        public float EligibleEndProgress { get; }
        public float OccurrenceChance { get; }
        public int MinimumGateCooldown { get; }
        public float RevealDurationSeconds { get; }
        public float HideLeadTimeSeconds { get; }
        public float TransitionSeconds { get; }
        public int MaxOccurrences { get; }
        public bool FirstOccurrenceGuaranteed { get; }

        public static FlickerSettings Disabled()
        {
            return new FlickerSettings(
                false,
                0.15f,
                0.85f,
                0f,
                0,
                0f,
                0f,
                0f,
                0,
                false);
        }

        public static FlickerSettings CreateDefault()
        {
            return new FlickerSettings(
                true,
                0.15f,
                0.85f,
                0.35f,
                2,
                1f,
                0.65f,
                0.12f,
                4,
                true);
        }

        private static void ValidateProgress(float value, string name)
        {
            if (value < 0f || value > 1f)
            {
                throw new ArgumentOutOfRangeException(name);
            }
        }
    }

    public sealed class FlickerVisibilityState
    {
        public float VisibleElapsed { get; private set; }
        public bool HideStarted { get; private set; }
        public float TransitionProgress { get; private set; }
        public float TargetAlpha => 1f - TransitionProgress;
        public int HideStartCount { get; private set; }

        public void Advance(
            float deltaSeconds,
            float estimatedArrivalSeconds,
            FlickerSettings settings)
        {
            if (deltaSeconds < 0f)
            {
                throw new ArgumentOutOfRangeException(nameof(deltaSeconds));
            }
            if (settings == null)
            {
                throw new ArgumentNullException(nameof(settings));
            }

            VisibleElapsed += deltaSeconds;
            if (!HideStarted &&
                VisibleElapsed >= settings.RevealDurationSeconds &&
                estimatedArrivalSeconds <= settings.HideLeadTimeSeconds)
            {
                HideStarted = true;
                HideStartCount++;
                TransitionProgress =
                    settings.TransitionSeconds <= 0f ? 1f : 0f;
            }
            if (HideStarted && TransitionProgress < 1f)
            {
                TransitionProgress = Math.Min(
                    1f,
                    TransitionProgress +
                    (deltaSeconds / settings.TransitionSeconds));
            }
        }

        public void Reset()
        {
            VisibleElapsed = 0f;
            HideStarted = false;
            TransitionProgress = 0f;
            HideStartCount = 0;
        }
    }

    public enum StagePrimaryMechanic
    {
        None,
        Shield,
        Booster,
        Camouflage,
        Fog,
        Ice,
        Echo
    }

    public enum StageMechanicGrantMechanic
    {
        None,
        Shield,
        Booster
    }

    public enum StageMechanicActivationMode
    {
        ActiveAtStageStart,
        AutoActivateAtProgress
    }

    public sealed class StageMechanicGrantSettings
    {
        public StageMechanicGrantSettings(
            bool enabled,
            StageMechanicGrantMechanic mechanic,
            StageMechanicActivationMode activationMode,
            float activationProgress,
            int chargeCount,
            bool stageLocal)
        {
            if (activationProgress < 0f || activationProgress > 1f)
            {
                throw new ArgumentOutOfRangeException(
                    nameof(activationProgress));
            }
            if (chargeCount < 0)
            {
                throw new ArgumentOutOfRangeException(nameof(chargeCount));
            }
            if (enabled &&
                (mechanic == StageMechanicGrantMechanic.None ||
                 chargeCount < 1 ||
                 !stageLocal))
            {
                throw new ArgumentException(
                    "Enabled mechanic grants require a mechanic, a charge, " +
                    "and stage-local scope.");
            }

            Enabled = enabled;
            Mechanic = mechanic;
            ActivationMode = activationMode;
            ActivationProgress = activationProgress;
            ChargeCount = chargeCount;
            StageLocal = stageLocal;
        }

        public bool Enabled { get; }
        public StageMechanicGrantMechanic Mechanic { get; }
        public StageMechanicActivationMode ActivationMode { get; }
        public float ActivationProgress { get; }
        public int ChargeCount { get; }
        public bool StageLocal { get; }

        public static StageMechanicGrantSettings Disabled()
        {
            return new StageMechanicGrantSettings(
                false,
                StageMechanicGrantMechanic.None,
                StageMechanicActivationMode.ActiveAtStageStart,
                0f,
                0,
                true);
        }
    }

    public sealed class EchoOfferCoordinator
    {
        private readonly EchoSettings _settings;
        private readonly uint _seed;
        private readonly int _gateCount;
        private int _firstOfferGateId;
        private int _lastAcquiredGateId;

        public EchoOfferCoordinator(
            EchoSettings settings,
            uint seed,
            int gateCount)
        {
            _settings = settings ??
                throw new ArgumentNullException(nameof(settings));
            if (gateCount < 1)
            {
                throw new ArgumentOutOfRangeException(nameof(gateCount));
            }

            _seed = DeterministicGateSequence.NormalizeSeed(seed);
            _gateCount = gateCount;
            Restart();
        }

        public bool EchoActive { get; private set; }
        public RunnerColor EchoColor { get; private set; }
        public int EchoAcquisitionCount { get; private set; }
        public int EchoCooldownRemaining { get; private set; }
        public bool EchoOfferPending { get; private set; }
        public int ActiveEchoOfferGateId { get; private set; }
        public int FirstOfferGateId => _firstOfferGateId;

        public GateModifier RegisterGate(
            int gateId,
            float progress,
            bool isGoal,
            GateModifier existingModifier)
        {
            if (gateId < 0)
            {
                throw new ArgumentOutOfRangeException(nameof(gateId));
            }
            if (progress < 0f || progress > 1f)
            {
                throw new ArgumentOutOfRangeException(nameof(progress));
            }
            if (!_settings.Enabled ||
                isGoal ||
                EchoActive ||
                EchoCooldownRemaining > 0 ||
                EchoAcquisitionCount >= _settings.MaxAcquisitions ||
                progress < _settings.EligibleStartProgress ||
                progress > _settings.EligibleEndProgress)
            {
                return existingModifier;
            }
            if (EchoOfferPending)
            {
                return ActiveEchoOfferGateId == gateId
                    ? existingModifier.With(GateModifierType.EchoProvider)
                    : existingModifier;
            }

            bool firstOffer =
                _settings.FirstEchoGuaranteed &&
                gateId == _firstOfferGateId;
            if (_settings.FirstEchoGuaranteed && gateId < _firstOfferGateId)
            {
                return existingModifier;
            }
            if (!firstOffer &&
                RollForGate(gateId) >= _settings.AdditionalEchoChance)
            {
                return existingModifier;
            }

            EchoOfferPending = true;
            ActiveEchoOfferGateId = gateId;
            return existingModifier.With(GateModifierType.EchoProvider);
        }

        public bool TryAcquire(
            int gateId,
            RunnerColor effectiveColor,
            bool passedByPlayerColor)
        {
            if (!EchoOfferPending ||
                ActiveEchoOfferGateId != gateId ||
                !passedByPlayerColor ||
                EchoActive)
            {
                return false;
            }

            EchoActive = true;
            EchoColor = effectiveColor;
            EchoAcquisitionCount++;
            ClearPendingOffer();
            EchoCooldownRemaining = _settings.CooldownGateCount;
            _lastAcquiredGateId = gateId;
            return true;
        }

        public bool TryConsume(RunnerColor effectiveColor)
        {
            if (!EchoActive || EchoColor != effectiveColor)
            {
                return false;
            }

            EchoActive = false;
            return true;
        }

        public void OnGateResolved(int gateId)
        {
            if (_lastAcquiredGateId == gateId)
            {
                _lastAcquiredGateId = -1;
                return;
            }
            if (EchoOfferPending && ActiveEchoOfferGateId == gateId)
            {
                ClearPendingOffer();
                EchoCooldownRemaining = _settings.CooldownGateCount;
                return;
            }

            if (EchoCooldownRemaining > 0)
            {
                EchoCooldownRemaining--;
            }
        }

        public void OnGateReleased(int gateId)
        {
            if (EchoOfferPending && ActiveEchoOfferGateId == gateId)
            {
                ClearPendingOffer();
                EchoCooldownRemaining = _settings.CooldownGateCount;
            }
        }

        public void Restart()
        {
            EchoActive = false;
            EchoColor = RunnerColor.Red;
            EchoAcquisitionCount = 0;
            EchoCooldownRemaining = 0;
            EchoOfferPending = false;
            ActiveEchoOfferGateId = -1;
            _lastAcquiredGateId = -1;
            _firstOfferGateId = ChooseFirstOfferGateId();
        }

        private int ChooseFirstOfferGateId()
        {
            int lastGateId = Math.Max(0, _gateCount - 1);
            int minimum = (int)Math.Ceiling(
                _settings.FirstEchoMinProgress * lastGateId);
            int maximum = (int)Math.Floor(
                _settings.FirstEchoMaxProgress * lastGateId);
            if (maximum < minimum)
            {
                maximum = minimum;
            }

            uint state = DeterministicGateSequence.AdvanceXorshift32(_seed);
            int count = maximum - minimum + 1;
            return minimum + (int)(state % (uint)Math.Max(1, count));
        }

        private float RollForGate(int gateId)
        {
            uint state = _seed ^ ((uint)(gateId + 1) * 0x9E3779B9u);
            state = DeterministicGateSequence.AdvanceXorshift32(state);
            return (state & 0x00FFFFFFu) / 16777216f;
        }

        private void ClearPendingOffer()
        {
            EchoOfferPending = false;
            ActiveEchoOfferGateId = -1;
        }
    }
}

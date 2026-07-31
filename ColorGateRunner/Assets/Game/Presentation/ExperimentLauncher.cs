using ColorGateRunner.Core;
using UnityEngine;
using UnityEngine.Serialization;
using UnityEngine.UI;

namespace ColorGateRunner.Presentation
{
    public sealed class ExperimentLauncher : MonoBehaviour
    {
        [SerializeField] private Text label;
        [SerializeField] private StageSceneController sceneController;
        [SerializeField] private Button previousColorCountButton;
        [SerializeField] private Button nextColorCountButton;
        [SerializeField] private Button nextMechanicButton;
        [SerializeField] private Button decreaseSeedButton;
        [SerializeField] private Button increaseSeedButton;
        [SerializeField] private Button toggleShieldButton;
        [SerializeField] private Button toggleBoosterButton;
        [SerializeField] private Button startButton;
        [SerializeField] private Button leaveButton;
        [FormerlySerializedAs("flickerEnabled")]
        [SerializeField, Tooltip("Enables Hidden gate selection.")]
        private bool hiddenEnabled = true;
        [FormerlySerializedAs("flickerEligibleStartProgress")]
        [SerializeField, Tooltip("First normalized progress eligible for Hidden.")]
        private float hiddenEligibleStartProgress = 0.15f;
        [FormerlySerializedAs("flickerEligibleEndProgress")]
        [SerializeField, Tooltip("Last normalized progress eligible for Hidden.")]
        private float hiddenEligibleEndProgress = 0.85f;
        [FormerlySerializedAs("flickerOccurrenceChance")]
        [SerializeField, Tooltip("Deterministic Hidden chance after the guaranteed first occurrence.")]
        private float hiddenOccurrenceChance = 0.35f;
        [FormerlySerializedAs("flickerMinimumGateCooldown")]
        [SerializeField, Tooltip("Minimum ordinary gates between Hidden occurrences.")]
        private int hiddenMinimumGateCooldown = 2;
        [FormerlySerializedAs("flickerRevealDurationSeconds")]
        [SerializeField, Tooltip("Seconds Hidden target information remains visible.")]
        private float hiddenRevealDurationSeconds = 1f;
        [FormerlySerializedAs("flickerHideLeadTimeSeconds")]
        [SerializeField, Tooltip("Effective-speed ETA threshold for Hidden.")]
        private float hiddenHideLeadTimeSeconds = 0.65f;
        [FormerlySerializedAs("flickerTransitionSeconds")]
        [SerializeField, Tooltip("Seconds used to hide target information.")]
        private float hiddenTransitionSeconds = 0.12f;
        [FormerlySerializedAs("flickerMaxOccurrences")]
        [SerializeField, Tooltip("Maximum Hidden gates in one experiment.")]
        private int hiddenMaxOccurrences = 4;
        [FormerlySerializedAs("flickerFirstOccurrenceGuaranteed")]
        [SerializeField, Tooltip("Guarantees the first eligible Hidden occurrence.")]
        private bool hiddenFirstOccurrenceGuaranteed = true;
        [SerializeField, Tooltip("Enables color-cycling Flicker gate selection.")]
        private bool colorCycleFlickerEnabled = true;
        [SerializeField, Tooltip("First normalized progress eligible for Flicker.")]
        private float colorCycleFlickerEligibleStartProgress = 0.15f;
        [SerializeField, Tooltip("Last normalized progress eligible for Flicker.")]
        private float colorCycleFlickerEligibleEndProgress = 0.85f;
        [SerializeField, Tooltip("Deterministic Flicker chance after the guaranteed first occurrence.")]
        private float colorCycleFlickerOccurrenceChance = 0.30f;
        [SerializeField, Tooltip("Minimum ordinary gates between Flicker occurrences.")]
        private int colorCycleFlickerMinimumGateCooldown = 2;
        [SerializeField, Tooltip("Maximum Flicker gates in one experiment.")]
        private int colorCycleFlickerMaxOccurrences = 4;
        [SerializeField, Tooltip("Guarantees the first eligible Flicker occurrence.")]
        private bool colorCycleFlickerFirstOccurrenceGuaranteed = true;
        [SerializeField, Tooltip("Distinct colors in each Flicker cycle; must be 2 or 3.")]
        private int colorCycleFlickerCycleColorCount = 2;
        [SerializeField, Tooltip("Seconds between authoritative Flicker color switches.")]
        private float colorCycleFlickerSwitchIntervalSeconds = 0.50f;
        [SerializeField, Tooltip("Seconds of visual pulse after a logical color switch.")]
        private float colorCycleFlickerTransitionPulseSeconds = 0.10f;
        [SerializeField, Tooltip("Minimum switch intervals visible before crossing.")]
        private int colorCycleFlickerMinimumCyclesVisible = 3;
        [SerializeField, Tooltip("Uses a deterministic seed-and-gate phase offset.")]
        private bool colorCycleFlickerRandomizePhaseOffset = true;

        private int _colorCount = 3;
        private MechanicExperimentType _mechanic;
        private uint _seed = ExperimentCatalog.DefaultSeed;
        private bool _shield;
        private bool _booster;
        private ExperimentSession _session;

        internal int ColorCount => _colorCount;
        internal MechanicExperimentType Mechanic => _mechanic;
        internal uint Seed => _seed;
        internal bool Shield => _shield;
        internal bool Booster =>
            _mechanic != MechanicExperimentType.Hidden &&
            _mechanic != MechanicExperimentType.Flicker &&
            _booster;
        internal ExperimentSession Session => _session;
        internal string Label => label.text;
        internal ExperimentDefinition SelectedDefinition =>
            ExperimentCatalog.Get(
                _colorCount,
                _mechanic,
                _seed,
                _mechanic == MechanicExperimentType.Hidden
                    ? CreateHiddenSettings()
                    : null,
                _mechanic == MechanicExperimentType.Flicker
                    ? CreateFlickerSettings()
                    : null);

        private void Awake()
        {
            AddListeners();
            RefreshLabel();
#if !UNITY_EDITOR && !DEVELOPMENT_BUILD
            gameObject.SetActive(false);
#endif
        }

        private void OnEnable()
        {
            if (sceneController != null &&
                !sceneController.ExperimentActive)
            {
                _session = null;
                RefreshLabel();
            }
        }

        private void OnDestroy()
        {
            RemoveListeners();
        }

        internal void Configure(
            StageSceneController controller,
            Text experimentLabel,
            Button previousColors,
            Button nextColors,
            Button mechanicButton,
            Button seedDown,
            Button seedUp,
            Button shieldButton,
            Button boosterButton,
            Button launch,
            Button leave)
        {
            sceneController = controller;
            label = experimentLabel;
            previousColorCountButton = previousColors;
            nextColorCountButton = nextColors;
            nextMechanicButton = mechanicButton;
            decreaseSeedButton = seedDown;
            increaseSeedButton = seedUp;
            toggleShieldButton = shieldButton;
            toggleBoosterButton = boosterButton;
            startButton = launch;
            leaveButton = leave;
        }

        internal bool HasRequiredReferences()
        {
            return sceneController != null &&
                label != null &&
                previousColorCountButton != null &&
                nextColorCountButton != null &&
                nextMechanicButton != null &&
                decreaseSeedButton != null &&
                increaseSeedButton != null &&
                toggleShieldButton != null &&
                toggleBoosterButton != null &&
                startButton != null &&
                leaveButton != null;
        }

        internal void PreviousColorCount()
        {
            _colorCount = _colorCount <= 3 ? 6 : _colorCount - 1;
            _session = null;
            RefreshLabel();
        }

        internal void NextColorCount()
        {
            _colorCount = _colorCount >= 6 ? 3 : _colorCount + 1;
            _session = null;
            RefreshLabel();
        }

        internal void NextMechanic()
        {
            _mechanic = (MechanicExperimentType)(
                ((int)_mechanic + 1) %
                ((int)MechanicExperimentType.Flicker + 1));
            if (_mechanic == MechanicExperimentType.Hidden ||
                _mechanic == MechanicExperimentType.Flicker)
            {
                _booster = false;
            }
            _session = null;
            RefreshLabel();
        }

        internal void DecreaseSeed()
        {
            _seed = _seed == 0u ? uint.MaxValue : _seed - 1u;
            _session = null;
            RefreshLabel();
        }

        internal void IncreaseSeed()
        {
            _seed++;
            _session = null;
            RefreshLabel();
        }

        internal void ToggleShield()
        {
            _shield = !_shield;
            RefreshLabel();
        }

        internal void ToggleBooster()
        {
            if (_mechanic == MechanicExperimentType.Hidden ||
                _mechanic == MechanicExperimentType.Flicker)
            {
                _booster = false;
                RefreshLabel();
                return;
            }
            _booster = !_booster;
            RefreshLabel();
        }

        internal void StartExperiment()
        {
            ExperimentDefinition definition = SelectedDefinition;
            StartItemSelection items =
                new StartItemSelection(_shield, Booster);
            sceneController.StartDevelopmentExperiment(
                definition,
                items);
            _session = sceneController.ExperimentSession;
            RefreshLabel();
        }

        internal void LeaveExperiment()
        {
            _session = null;
            sceneController.ShowLobby();
            RefreshLabel();
        }

        private void AddListeners()
        {
            previousColorCountButton.onClick.AddListener(PreviousColorCount);
            nextColorCountButton.onClick.AddListener(NextColorCount);
            nextMechanicButton.onClick.AddListener(NextMechanic);
            decreaseSeedButton.onClick.AddListener(DecreaseSeed);
            increaseSeedButton.onClick.AddListener(IncreaseSeed);
            toggleShieldButton.onClick.AddListener(ToggleShield);
            toggleBoosterButton.onClick.AddListener(ToggleBooster);
            startButton.onClick.AddListener(StartExperiment);
            leaveButton.onClick.AddListener(LeaveExperiment);
        }

        private void RemoveListeners()
        {
            if (!HasRequiredReferences())
            {
                return;
            }
            previousColorCountButton.onClick.RemoveListener(PreviousColorCount);
            nextColorCountButton.onClick.RemoveListener(NextColorCount);
            nextMechanicButton.onClick.RemoveListener(NextMechanic);
            decreaseSeedButton.onClick.RemoveListener(DecreaseSeed);
            increaseSeedButton.onClick.RemoveListener(IncreaseSeed);
            toggleShieldButton.onClick.RemoveListener(ToggleShield);
            toggleBoosterButton.onClick.RemoveListener(ToggleBooster);
            startButton.onClick.RemoveListener(StartExperiment);
            leaveButton.onClick.RemoveListener(LeaveExperiment);
        }

        private void RefreshLabel()
        {
            string items =
                _mechanic == MechanicExperimentType.Hidden ||
                _mechanic == MechanicExperimentType.Flicker
                ? _shield
                    ? "SHIELD / BOOSTER DISABLED"
                    : "BOOSTER DISABLED"
                : _shield && _booster
                    ? "SHIELD+BOOSTER"
                    : _shield
                        ? "SHIELD"
                        : _booster ? "BOOSTER" : "NO ITEMS";
            label.text =
                $"{_colorCount} COLORS · {_mechanic.ToString().ToUpperInvariant()} · SEED {_seed}\n" +
                $"{items}{(_session == null ? string.Empty : " · READY")}";
        }

        private HiddenSettings CreateHiddenSettings()
        {
            return new HiddenSettings(
                hiddenEnabled,
                hiddenEligibleStartProgress,
                hiddenEligibleEndProgress,
                hiddenOccurrenceChance,
                hiddenMinimumGateCooldown,
                hiddenRevealDurationSeconds,
                hiddenHideLeadTimeSeconds,
                hiddenTransitionSeconds,
                hiddenMaxOccurrences,
                hiddenFirstOccurrenceGuaranteed);
        }

        private FlickerSettings CreateFlickerSettings()
        {
            return new FlickerSettings(
                colorCycleFlickerEnabled,
                colorCycleFlickerEligibleStartProgress,
                colorCycleFlickerEligibleEndProgress,
                colorCycleFlickerOccurrenceChance,
                colorCycleFlickerMinimumGateCooldown,
                colorCycleFlickerMaxOccurrences,
                colorCycleFlickerFirstOccurrenceGuaranteed,
                colorCycleFlickerCycleColorCount,
                colorCycleFlickerSwitchIntervalSeconds,
                colorCycleFlickerTransitionPulseSeconds,
                colorCycleFlickerMinimumCyclesVisible,
                colorCycleFlickerRandomizePhaseOffset);
        }
    }
}

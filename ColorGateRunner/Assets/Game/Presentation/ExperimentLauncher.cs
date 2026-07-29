using ColorGateRunner.Core;
using UnityEngine;
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
        internal bool Booster => _booster;
        internal ExperimentSession Session => _session;
        internal string Label => label.text;

        private void Awake()
        {
            AddListeners();
            RefreshLabel();
#if !UNITY_EDITOR && !DEVELOPMENT_BUILD
            gameObject.SetActive(false);
#endif
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
                ((int)_mechanic + 1) % 4);
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
            _booster = !_booster;
            RefreshLabel();
        }

        internal void StartExperiment()
        {
            ExperimentDefinition definition =
                ExperimentCatalog.Get(_colorCount, _mechanic, _seed);
            StartItemSelection items = new StartItemSelection(_shield, _booster);
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
            string items = _shield && _booster
                ? "SHIELD+BOOSTER"
                : _shield ? "SHIELD" : _booster ? "BOOSTER" : "NO ITEMS";
            label.text =
                $"{_colorCount} COLORS · {_mechanic.ToString().ToUpperInvariant()} · SEED {_seed}\n" +
                $"{items}{(_session == null ? string.Empty : " · READY")}";
        }
    }
}

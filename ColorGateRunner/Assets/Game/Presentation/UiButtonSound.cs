using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

namespace ColorGateRunner.Presentation
{
    // Plays its Builder-chosen cue when the control is clicked; a control
    // that could not act when pressed plays the denied cue instead.
    public sealed class UiButtonSound :
        MonoBehaviour,
        IPointerDownHandler,
        IPointerClickHandler
    {
        [SerializeField] private SoundCue cue;
        [SerializeField] private SoundEffectPlayer player;

        private bool _pressed;
        private bool _interactableAtPress;

        internal SoundCue Cue => cue;
        internal SoundEffectPlayer Player => player;

        internal void Configure(SoundCue value, SoundEffectPlayer effects)
        {
            cue = value;
            player = effects;
        }

        // Interactability is read at press time: a click handler may switch
        // its own control off (a purchase in flight, a scene leaving), and
        // that successful click must not sound refused.
        public void OnPointerDown(PointerEventData eventData)
        {
            Selectable selectable = GetComponent<Selectable>();
            _interactableAtPress =
                selectable == null || selectable.IsInteractable();
            _pressed = true;
        }

        public void OnPointerClick(PointerEventData eventData)
        {
            if (eventData != null &&
                eventData.button != PointerEventData.InputButton.Left)
            {
                return;
            }
            bool interactable = _pressed
                ? _interactableAtPress
                : IsInteractableNow();
            _pressed = false;
            if (player != null)
            {
                player.Play(ResolveCue(interactable));
            }
        }

        internal SoundCue ResolveCue()
        {
            return ResolveCue(IsInteractableNow());
        }

        // Runs after the control's own click handler (added after it), so a
        // Toggle already shows its new state.
        internal SoundCue ResolveCue(bool interactable)
        {
            if (!interactable)
            {
                return SoundCue.UiDenied;
            }
            if (GetComponent<Selectable>() is Toggle toggle)
            {
                return toggle.isOn ? SoundCue.UiToggleOn : SoundCue.UiToggleOff;
            }
            return cue;
        }

        private bool IsInteractableNow()
        {
            Selectable selectable = GetComponent<Selectable>();
            return selectable == null || selectable.IsInteractable();
        }
    }
}

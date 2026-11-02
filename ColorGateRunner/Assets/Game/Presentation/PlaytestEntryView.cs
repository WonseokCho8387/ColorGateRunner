using System;
using ColorGateRunner.Product;
using UnityEngine;
using UnityEngine.UI;

namespace ColorGateRunner.Presentation
{
    public sealed class PlaytestEntryView : MonoBehaviour
    {
        [SerializeField] private Text title;
        [SerializeField] private Text description;
        [SerializeField] private Text actionLabel;
        [SerializeField] private Button playButton;
        private Action _play;

        internal void Configure(Text heading, Text details, Button play, Text label)
        {
            title = heading;
            description = details;
            playButton = play;
            actionLabel = label;
        }

        internal void Bind(PlaytestSession session, Action play)
        {
            _play = play;
            playButton.onClick.RemoveListener(Play);
            playButton.onClick.AddListener(Play);
            title.text = session.Completed ? "THANK YOU FOR PLAYING" : "COLOR GATE RUNNER";
            description.text = session.Completed
                ? "PLAYTEST v0.1\n8 / 8 STAGES COMPLETE\n\n" +
                  "Was the rule clear?\nDid failures make sense?\nWould you play again?\n\n" +
                  "Please share feedback with the person who invited you."
                : "PLAYTEST v0.1\n8 SHORT STAGES\n\n" +
                  "Click or tap to change your color.\nMatch each gate before crossing.\n\n" +
                  "Unlimited retries. No purchases.\nRefreshing starts a new playtest.\n\n" +
                  "Analytics is not connected.\nNo gameplay events are sent.";
            actionLabel.text = session.Completed ? "PLAY AGAIN" :
                session.AttemptStarted || session.Position > 0 ? "CONTINUE PLAYTEST" : "PLAY";
            playButton.interactable = true;
        }

        private void Play() => _play?.Invoke();
        private void OnDestroy() { if (playButton != null) playButton.onClick.RemoveListener(Play); }
    }
}

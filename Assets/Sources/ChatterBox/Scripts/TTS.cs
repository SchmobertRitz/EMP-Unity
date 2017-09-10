//
// MIT License
// Copyright (c) EMP - https://github.com/SchmobertRitz/EMP-Unity
//
using System;
using System.Collections;
using UnityEngine;

namespace EMP.ChatterBox
{
    public class TTS
    {
        public ITextToSpeech TextToSpeech
        { get; set; }

        public AudioSource AudioSource
        { get; set; }

        public TTSDisplay TextDisplay
        { get; set; }

        private Coroutine waitForEndOfAudioClipCoroutine;

        public WaitWhileSaying YieldSaying(string text, float speakingPauseBefore = 1)
        {
            return new WaitWhileSaying {
                text = text,
                tts = this,
                speakingPauseBefore = speakingPauseBefore
            };
        }

        public void Prepare(string text, Action<Action<Action>> ttsStartAction, Action onTssEndedAction = null, ChatterBox.ECachingMode cachingMode = ChatterBox.ECachingMode.CacheInFileSystem) {
            ChatterBox.Instance.Prepare(
                new TTSText(text),
                this,
                ttsStartAction,
                cachingMode
            );
        }

        public void Say(string text, Action onTssEndedAction = null, ChatterBox.ECachingMode cachingMode = ChatterBox.ECachingMode.CacheInFileSystem) {
            ChatterBox.Instance.Prepare(
                new TTSText(text),
                this,
                startSayingAction => startSayingAction(onTssEndedAction),
                cachingMode
            );
        }
        
        internal void PlayAudioClip(AudioClip audioClip, TTSText ttsText, Action onTssEndedAction) {
            if (waitForEndOfAudioClipCoroutine != null)
            {
                ChatterBox.Instance.StopCoroutine(waitForEndOfAudioClipCoroutine);
                Debug.LogWarning("Started an audio clip before previous audio clip ended.");
                waitForEndOfAudioClipCoroutine = null;
            }

            if (TextDisplay != null)
            {
                TextDisplay.SetText(ttsText.DisplayText);
            }

            if (AudioSource != null)
            {
                AudioSource.PlayOneShot(audioClip);
            }
            else
            {
                Debug.LogWarning("Unable to play tts. No AudioSource given.");
            }

            waitForEndOfAudioClipCoroutine = ChatterBox.Instance.StartCoroutine(WaitForEndOfAudioClip(onTssEndedAction));
        }

        private Action lastOnTssEndedAction;
        private IEnumerator WaitForEndOfAudioClip(Action onTssEndedAction)
        {
            if (lastOnTssEndedAction != null) {
                lastOnTssEndedAction();
            }
            lastOnTssEndedAction = onTssEndedAction;

            if (AudioSource != null)
            {
                yield return new WaitWhile(() => AudioSource.isPlaying);
            } else
            {
                yield return new WaitForSeconds(1);
            }

            if (TextDisplay != null)
            {
                TextDisplay.SetText("");
            }

            waitForEndOfAudioClipCoroutine = null;

            if (lastOnTssEndedAction != null)
            {
                lastOnTssEndedAction();
                lastOnTssEndedAction = null;
            }
        }
    }
}

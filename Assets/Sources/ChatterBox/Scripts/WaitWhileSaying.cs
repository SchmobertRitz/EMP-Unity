//
// MIT License
// Copyright (c) EMP - https://github.com/SchmobertRitz/EMP-Unity
//
using System;
using UnityEngine;

namespace EMP.ChatterBox
{
    public class WaitWhileSaying : CustomYieldInstruction
    {
        public string text;
        public TTS tts;
        public float speakingPauseBefore;
        private bool initialized;
        private float timeToStartTts;
        private Action<Action> startTtsAction;
        private bool ttsStarted;
        private bool isSpeaking;

        public override bool keepWaiting
        {
            get
            {
                if (!initialized)
                {
                    initialized = true;
                    timeToStartTts = Time.time + speakingPauseBefore;
                    tts.Prepare(
                        text,
                        action => startTtsAction = action
                    );
                    return true;
                }
                else if (startTtsAction != null && Time.time > timeToStartTts && !ttsStarted)
                {
                    ttsStarted = true;
                    isSpeaking = true;
                    startTtsAction(() => isSpeaking = false);
                    return true;
                }
                else if (ttsStarted)
                {
                    return isSpeaking;
                }
                return true;
            }
        }
    }
}

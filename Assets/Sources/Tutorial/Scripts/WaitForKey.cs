//
// MIT License
// Copyright (c) EMP - https://github.com/SchmobertRitz/EMP-Unity
//
using UnityEngine;

namespace EMP.Tutorial
{
    public class WaitForKey : CustomYieldInstruction
    {
        private readonly KeyCode keyCode;
        private readonly TutorialResult wasTimeout;
        private readonly float timeUntilTimeout;

        public WaitForKey(KeyCode keyCode, TutorialResult result, float timeout = 10)
        {
            this.keyCode = keyCode;
            this.wasTimeout = result;
            this.timeUntilTimeout = Time.time + timeout;
        }

        public override bool keepWaiting
        {
            get
            {
                if (Input.GetKeyDown(keyCode))
                {
                    wasTimeout.SetPassed();
                    return false;
                }
                else if (Time.time > timeUntilTimeout)
                {
                    wasTimeout.SetFailed();
                    return false;
                }
                return true;
            }
        }
    }
}

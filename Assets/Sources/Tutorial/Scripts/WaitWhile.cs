//
// MIT License
// Copyright (c) EMP - https://github.com/SchmobertRitz/EMP-Unity
//
using System;
using UnityEngine;

namespace EMP.Tutorial
{
    public class WaitWhile : CustomYieldInstruction
    {
        private readonly Func<bool> condition;
        private readonly TutorialResult result;
        private readonly float timeUntilTimeout;

        public WaitWhile(Func<bool> condition, TutorialResult result, float timeout = 10)
        {
            this.condition = condition;
            this.result = result;
            this.timeUntilTimeout = Time.time + timeout;
        }

        public override bool keepWaiting
        {
            get
            {
                if (!condition())
                {
                    result.SetPassed();
                    return false;
                }
                else if (Time.time > timeUntilTimeout)
                {
                    result.SetFailed();
                    return false;
                }
                return true;
            }
        }
    }
}

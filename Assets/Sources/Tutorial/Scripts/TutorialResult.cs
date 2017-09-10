//
// MIT License
// Copyright (c) EMP - https://github.com/SchmobertRitz/EMP-Unity
//
namespace EMP.Tutorial
{
    public class TutorialResult
    {
        private bool passed;
        public bool Passed
        { get { return passed; } }

        public bool Failed
        { get { return !passed; } }

        public void SetPassed()
        { passed = true; }

        public void SetFailed()
        { passed = false; }
    }
}

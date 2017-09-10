//
// MIT License
// Copyright (c) EMP - https://github.com/SchmobertRitz/EMP-Unity
//
using System.Collections;
using System.Collections.Generic;
using System.Text.RegularExpressions;
using UnityEngine;

namespace EMP.ChatterBox
{
    public class TTSText
    {
        private static Regex regex = new Regex(@"\(([^|]*)\|([^)]*)\)");

        public string DisplayText
        { get; private set; }

        public string SpokenText
        { get; private set; }

        public TTSText(string text)
        {
            DisplayText = regex.Replace(text, "$1");
            SpokenText = regex.Replace(text, "$2");
        }
    }
}

//
// MIT License
// Copyright (c) EMP - https://github.com/SchmobertRitz/EMP-Unity
//
using UnityEngine;

namespace EMP.Forms
{
    public class Headline : Label
    {
        public Headline(string text) : base(text)
        {
            style.fontSize = 15;
            style.fontStyle = FontStyle.Bold;
        }
    }
}

//
// MIT License
// Copyright (c) EMP - https://github.com/SchmobertRitz/EMP-Unity
//
using UnityEngine;

namespace EMP.Forms
{
    public class Label : View
    {
        public string Text { get; set; }

        public Label(string text) : base()
        {
            Height = LineHeight;
            Text = text;
            style = new GUIStyle(GUI.skin.label);
            style.alignment = TextAnchor.MiddleLeft;
            style.wordWrap = true;
        }

        public override void Draw()
        {
            GUI.SetNextControlName(Name);
            GUI.Label(Rect, Text, style);
        }
    }
}

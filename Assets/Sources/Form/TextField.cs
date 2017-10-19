//
// MIT License
// Copyright (c) EMP - https://github.com/SchmobertRitz/EMP-Unity
//
using UnityEngine;

namespace EMP.Forms
{
    public class TextField : View
    {
        public string Text { get; set; }

        public TextField(string text = "", float? width = null) : base()
        {
            this.Width = width;
            Height = LineHeight;
            Text = text == null ? "" : text;
            style = new GUIStyle(GUI.skin.textField);
            style.alignment = TextAnchor.MiddleLeft;
        }

        public override void Draw()
        {
            GUI.SetNextControlName(Name);
            Text = GUI.TextField(Rect, Text, style);
        }
    }
}

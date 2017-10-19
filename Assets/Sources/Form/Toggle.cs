//
// MIT License
// Copyright (c) EMP - https://github.com/SchmobertRitz/EMP-Unity
//
using UnityEngine;

namespace EMP.Forms
{
    public class Toggle : View
    {
        public bool Checked { get; set; }
        public string Text { get; set; }

        public Toggle(bool isChecked, string text = "") : base()
        {
            Height = LineHeight;
            Checked = isChecked;
            Text = text;
            style = new GUIStyle(GUI.skin.toggle);
        }

        public override void Draw()
        {
            GUI.SetNextControlName(Name);
            Checked = GUI.Toggle(Rect, Checked, Text, style);
        }
    }
}

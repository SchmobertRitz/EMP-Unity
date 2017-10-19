//
// MIT License
// Copyright (c) EMP - https://github.com/SchmobertRitz/EMP-Unity
//
using System;
using UnityEngine;

namespace EMP.Forms
{
    public class Button : View
    {
        public string Text { get; set; }
        public Action<Button> Action;

        public Button(string text, Action<Button> action = null)
        {
            Height = LineHeight;
            Text = text;
            Action = action;
            style = new GUIStyle(GUI.skin.button);
        }

        public override void Draw()
        {
            if (GUI.Button(Rect, Text, style) && Action != null)
            {
                Action(this);
            }
        }
    }

}

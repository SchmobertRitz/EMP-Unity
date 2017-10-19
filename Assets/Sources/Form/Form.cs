//
// MIT License
// Copyright (c) EMP - https://github.com/SchmobertRitz/EMP-Unity
//
using UnityEngine;

namespace EMP.Forms
{
    public class Form : LinearLayout
    {
        public View RequestFocusForView { get; set; }

        public Form(EOrientation orientation = EOrientation.Vertical) : base(orientation)
        {
        }

        public void OnGUI(Rect rect)
        {
            Layout(rect);
            Draw();
            if (RequestFocusForView != null)
            {
                GUI.FocusControl(RequestFocusForView.Name);
                RequestFocusForView = null;
            }
        }
    }
}

//
// MIT License
// Copyright (c) EMP - https://github.com/SchmobertRitz/EMP-Unity
//
using EMP.Forms;
using UnityEditor;
using UnityEngine;

namespace EMP.Editor {

    public abstract class FormPopup : PopupWindowContent
    {
        private Form form;

        public override void OnGUI(Rect rect)
        {
            if (form == null)
            {
                Vector2 formSize = GetFormSize();
                int border = 100;
                Rect popupRect = new Rect(
                    Mathf.Min(Screen.currentResolution.width - formSize.x - border, Mathf.Max(border, Event.current.mousePosition.x - formSize.x / 2)),
                    Mathf.Min(Screen.currentResolution.height - formSize.y - border, Mathf.Max(border, Event.current.mousePosition.y - formSize.y / 2)),
                    formSize.x, formSize.y);
                editorWindow.position = popupRect;
                editorWindow.minSize = formSize;
                form = new Form();
                OnCreateForm(form);
            }
            else
            {
                //GUI.skin.settings.cursorFlashSpeed = 1;
                form.OnGUI(new Rect(10, 10, rect.width - 20, rect.height - 20));
            }
        }

        public void Show()
        {
            Vector3 size = GetFormSize();
            PopupWindow.Show(new Rect(0, 0, size.x, size.y), this);
        }
        protected abstract void OnCreateForm(Form form);
        protected abstract Vector2 GetFormSize();

        public override Vector2 GetWindowSize()
        {
            return GetFormSize();
        }
    }

}

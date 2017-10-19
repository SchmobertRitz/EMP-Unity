//
// MIT License
// Copyright (c) EMP - https://github.com/SchmobertRitz/EMP-Unity
//
using EMP.Forms;
using System;
using UnityEngine;

namespace EMP.Forms
{
    public class View
    {
        public const float LineHeight = 24;

        public class Inset
        {
            public static Inset Border(float width)
            {
                return new View.Inset()
                {
                    top = width,
                    right = width,
                    bottom = width,
                    left = width
                };
            }
            public float top, right, bottom, left;
        }

        public enum EVisibility
        {
            Visible, Invisible, Hidden
        }

        protected bool Dirty { get; set; }
        public bool Enabled { get; set; }
        public EVisibility Visibility { get; set; }
        public Rect Rect { get; set; }
        public float? Width { get; set; }
        public float? Height { get; set; }
        public GUIStyle style { get; set; }
        public View Parent { get; internal set; }

        public string Name { get; set; }

        public View()
        {
            Width = null;
            Height = null;
            Name = Guid.NewGuid().ToString();
        }

        public virtual void Layout(Rect rect)
        {
            Rect = rect;
        }

        public virtual void Draw()
        {
            // Override
        }

        public T Bind<T>(out T thisPointer)
            where T : View
        {
            thisPointer = (T) this;
            return (T) this;
        }

        public static View operator +(View vLeft, View vRight)
        {
            LinearLayout linLayout = vLeft as LinearLayout;
            if (linLayout != null && linLayout.Orientation == LinearLayout.EOrientation.Horizontal)
            {
                return linLayout.Add(vRight);
            }

            linLayout = vLeft.Parent as LinearLayout;
            if (linLayout != null && linLayout.Orientation == LinearLayout.EOrientation.Horizontal)
            {
                return linLayout.Add(vRight);
            }

            return new LinearLayout(LinearLayout.EOrientation.Horizontal).Add(vLeft).Add(vRight);
        }

        public static View operator /(View vLeft, View vRight)
        {
            LinearLayout linLayout = vLeft as LinearLayout;
            if (linLayout != null && linLayout.Orientation == LinearLayout.EOrientation.Vertical)
            {
                return linLayout.Add(vRight);
            }

            linLayout = vLeft.Parent as LinearLayout;
            if (linLayout != null && linLayout.Orientation == LinearLayout.EOrientation.Vertical)
            {
                return linLayout.Add(vRight);
            }

            return new LinearLayout(LinearLayout.EOrientation.Vertical).Add(vLeft).Add(vRight);
        }

        public static View operator <<(View vLeft, int height)
        {
            vLeft.Height = height;
            return vLeft;
        }

        public static View operator >>(View vLeft, int width)
        {
            vLeft.Width = width;
            return vLeft;
        }
    }
}

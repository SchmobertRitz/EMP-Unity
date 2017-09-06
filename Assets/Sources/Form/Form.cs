//
// MIT License
// Copyright (c) EMP - https://github.com/SchmobertRitz/EMP-Unity
//
using System;
using System.Collections.Generic;
using UnityEngine;

namespace EMP.Forms
{
    public class Form : Linear
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
    }

    public abstract class ViewGroup<T> : View
        where T : ViewGroup<T>
    {
        protected List<View> views = new List<View>();

        public virtual T Add(View view)
        {
            views.Add(view);
            Dirty = true;
            return (T) this;
        }

        public virtual T Remove(View view)
        {
            views.Remove(view);
            Dirty = true;
            return (T) this;
        }

        public override void Draw()
        {
            views.ForEach(view => {
                view.Draw();
            });
        }
    }

    public class Linear : ViewGroup<Linear>
    {
        public static Linear Horizontal(params View[] views)
        {
            Linear result = new Linear(EOrientation.Horizontal);
            Array.ForEach(views, v => result.Add(v));
            return result;
        }

        public static Linear Vertical(params View[] views)
        {
            Linear result = new Linear(EOrientation.Vertical);
            Array.ForEach(views, v => result.Add(v));
            return result;
        }

        public float Spacing { get; set; }

        protected List<float> weights = new List<float>();
        public enum EOrientation
        {
            Horizontal, Vertical
        }

        public EOrientation Orientation;

        public Linear(EOrientation orientation = EOrientation.Vertical)
        {
            Orientation = orientation;
        }

        public override Linear Add(View view)
        {
            return Add(view, 1);
        }

        public override Linear Remove(View view)
        {
            return Remove(view, 1);
        }

        public Linear Add(View view, float weight)
        {
            views.Add(view);
            weights.Add(weight);
            return this;
        }

        public Linear Remove(View view, float weight)
        {
            int index = views.IndexOf(view);
            if (index >= 0)
            {
                views.RemoveAt(index);
                weights.RemoveAt(index);
            }
            return this;
        }

        public override void Layout(Rect rect)
        {
            if (Orientation == EOrientation.Horizontal)
            {
                HelpLayouting(
                    () => rect.width,
                    (v) => v.Width,
                    (offset, width, v) => new Rect(rect.x + offset, rect.y, width, Mathf.Min(v.Height ?? float.MaxValue, rect.height))
                );
            }
            else
            {
                HelpLayouting(
                    () => rect.height,
                    (v) => v.Height,
                    (offset, height, v) => new Rect(rect.x, rect.y + offset, Mathf.Min(v.Width ?? float.MaxValue, rect.width), height)
                );
            }
        }

        private void HelpLayouting(
            Func<float> extendFunction,
            Func<View, float?> fixedExtend,
            Func<float, float, View, Rect> rect
        )
        {
            float weightSum = 0;
            weights.ForEach(w => weightSum += w);

            float reservedSpace = Spacing * (views.Count - 1);

            views.ForEach(v =>
                {
                    float? fixedExtends = fixedExtend(v);
                    if (fixedExtends != null)
                    {
                        weightSum -= weights[views.IndexOf(v)];
                        reservedSpace += fixedExtend(v).Value;
                    }
                });

            float unitExtend = (extendFunction() - reservedSpace) / weightSum;
            float offset = 0;
            for (int i = 0; i < views.Count; i++)
            {
                View v = views[i];
                float w = weights[i];
                float extendOfView = fixedExtend(v) ?? unitExtend * w;
                v.Rect = rect(offset, extendOfView, v);
                v.Layout(v.Rect);
                offset += extendOfView + Spacing;
            }
        }
    }

    public class Grid : ViewGroup<Grid>
    {
        public enum EOrientation
        {
            Horizontal, Vertical
        }

        private readonly int rows;
        private readonly EOrientation orientation;

        public Grid(int rows, EOrientation orientation) : base()
        {
            this.rows = rows;
            this.orientation = orientation;
        }

        public override void Layout(Rect rect)
        {
            if (orientation == EOrientation.Vertical)
            {
                float w = rect.width / rows;
                float h = rect.height / (views.Count / rows);
                for (int i = 0; i < views.Count; i++)
                {
                    float x = i % rows * w + rect.x;
                    float y = i / rows * h + rect.y;
                    View view = views[i];
                    view.Rect = new Rect(x, y, w, h);
                }
            } else
            {
                float w = rect.height / rows;
                float h = rect.width / (views.Count / rows);
                for (int i = 0; i < views.Count; i++)
                {
                    float x = i / rows * w + rect.x;
                    float y = i % rows * h + rect.y; 
                    View view = views[i];
                    view.Rect = new Rect(x, y, w, h);
                }
            }
        }
    }

    public class Label : View
    {
        public string Text { get; set; }

        public Label(string text) : base()
        {
            Height = LineHeight;
            Text = text;
            style = new GUIStyle(GUI.skin.label);
        }

        public override void Draw()
        {
            GUI.SetNextControlName(Name);
            GUI.Label(Rect, Text, style);
        }
    }

    public class Headline : Label
    {
        public Headline(string text) : base(text)
        {
            style.fontSize = 15;
            style.fontStyle = FontStyle.Bold;
        }
    }

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

    public class TextField : View
    {
        public string Text { get; set; }

        public TextField(string text = "") : base()
        {
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

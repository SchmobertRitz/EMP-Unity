using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace EMP.Form
{
    public class Form : Linear
    {
        public Form()
        {
        }

        public void OnGUI(Rect rect)
        {
            Layout(rect);
            Draw();
        }
    }

    public class View
    {
        public struct Inset
        {
            public float top, right, bottom, left;
        }

        public enum EVisibility
        {
            Visible, Invisible, Hidden
        }


        protected bool Dirty { get; set; }
        public Inset Padding { get; set; }
        public Inset Margin { get; set; }
        public bool Enabled { get; set; }
        public EVisibility Visibility { get; set; }
        public Rect Rect { get; set; }
        public float? FixedWidth { get; set; }
        public float? FixedHeight { get; set; }

        public View()
        {
            FixedWidth = null;
            FixedHeight = null;
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

    public abstract class ViewGroup : View
    {
        protected List<View> views = new List<View>();

        public virtual void Add(View view)
        {
            views.Add(view);
            Dirty = true;
        }

        public virtual void Remove(View view)
        {
            views.Remove(view);
            Dirty = true;
        }
    }

    public class Linear : ViewGroup
    {
        protected List<float> weights = new List<float>();
        public enum EOrientation
        {
            Horizontal, Vertical
        }

        public EOrientation Orientation = EOrientation.Horizontal;

        public Linear(EOrientation orientation = EOrientation.Horizontal)
        {
            Orientation = orientation;
        }

        public override void Add(View view)
        {
            Add(view, 1);
        }

        public override void Remove(View view)
        {
            Remove(view, 1);
        }

        public void Add(View view, float weight)
        {
            views.Add(view);
            weights.Add(weight);
        }

        public void Remove(View view, float weight)
        {
            int index = views.IndexOf(view);
            if (index >= 0)
            {
                views.RemoveAt(index);
                weights.RemoveAt(index);
            }
        }

        public override void Layout(Rect rect)
        {
            if (Orientation == EOrientation.Horizontal)
            {
                HelpLayouting(
                    () => rect.width,
                    (v) => v.FixedWidth,
                    (offset, width) => new Rect(rect.x + offset, rect.y, width, rect.height)
                );
            }
            else
            {
                HelpLayouting(
                    () => rect.height,
                    (v) => v.FixedHeight,
                    (offset, height) => new Rect(rect.x, rect.y + offset, rect.width, height)
                );
            }
        }

        private void HelpLayouting(Func<float> extendFunction, Func<View, float?> fixedExtendFunction, Func<float, float, Rect> rectFunction)
        {
            float weightSum = 0;
            weights.ForEach(w => weightSum += w);

            float fixedValues = 0;
            views.FindAll(v => fixedExtendFunction(v) != null)
                .ForEach(v =>
                {
                    weightSum -= weights[views.IndexOf(v)];
                    fixedValues += fixedExtendFunction(v).Value;
                });

            float unitExtend = (extendFunction() - fixedValues) / weightSum;
            float offset = 0;
            for (int i = 0; i < views.Count; i++)
            {
                View v = views[i];
                float w = weights[i];
                float extendOfView = fixedExtendFunction(v) ?? unitExtend * w;
                v.Rect = rectFunction(offset, extendOfView);
                v.Layout(v.Rect);
                offset += extendOfView;
            }
        }

        public override void Draw()
        {
            views.ForEach(view => view.Draw());
        }
    }

    public class Label : View
    {
        public string Text { get; set; }

        public Label(string text)
        {
            Text = text;
        }

        public override void Draw()
        {
            GUI.Label(Rect, Text);
        }
    }

    public class Button : View
    {
        public string Text { get; set; }
        public Action<Button> Action;

        public Button(string text, Action<Button> action = null)
        {
            Text = text;
            Action = action;
        }

        public override void Draw()
        {
            if (GUI.Button(Rect, Text) && Action != null)
            {
                Action(this);
            }
        }
    }

    public class TextField : View
    {
        public string Text { get; set; }
        public Action<Button> Action { get; set; }

        public TextField(string text)
        {
            Text = text;
        }

        public override void Draw()
        {
            Text = GUI.TextField(Rect, Text);
        }
    }
}

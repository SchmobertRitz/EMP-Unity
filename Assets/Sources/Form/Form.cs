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
            float weightSum = 0;
            weights.ForEach(w => weightSum += w);

            if (Orientation == EOrientation.Horizontal)
            {
                float fixedWidths = 0;
                views.FindAll(v => v.FixedWidth != null)
                    .ForEach(v =>
                    {
                        weightSum -= weights[views.IndexOf(v)];
                        fixedWidths += v.FixedWidth.Value;
                    });

                float unitWidth = (rect.width - fixedWidths) / weightSum;
                float offset = 0;
                for (int i = 0; i < views.Count; i++)
                {
                    View v = views[i];
                    float w = weights[i];
                    float width = v.FixedWidth ?? unitWidth * w;
                    v.Rect = new Rect(rect.x + offset, rect.y, width, rect.height);
                    v.Layout(v.Rect);
                    offset += width;
                }
            }
            else
            {
                float fixedHeights = 0;
                views.FindAll(v => v.FixedHeight != null)
                    .ForEach(v =>
                    {
                        weightSum -= weights[views.IndexOf(v)];
                        fixedHeights += v.FixedHeight.Value;
                    });

                float unitHeight = (rect.height - fixedHeights) / weightSum;
                float offset = 0;
                for (int i = 0; i < views.Count; i++)
                {
                    View v = views[i];
                    float w = weights[i];
                    float height = v.FixedHeight ?? unitHeight * w;
                    v.Rect = new Rect(rect.x, rect.y + offset, rect.width, height);
                    v.Layout(v.Rect);
                    offset += height;
                }
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

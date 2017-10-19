//
// MIT License
// Copyright (c) EMP - https://github.com/SchmobertRitz/EMP-Unity
//
using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace EMP.Forms
{
    public class LinearLayout : ViewGroup<LinearLayout>
    {
        public static LinearLayout Horizontal(params View[] views)
        {
            LinearLayout result = new LinearLayout(EOrientation.Horizontal);
            Array.ForEach(views, v => result.Add(v));
            return result;
        }

        public static LinearLayout Vertical(params View[] views)
        {
            LinearLayout result = new LinearLayout(EOrientation.Vertical);
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

        public LinearLayout(EOrientation orientation = EOrientation.Vertical)
        {
            Orientation = orientation;
        }

        public override LinearLayout Add(View view)
        {
            return Add(view, 1);
        }

        public override LinearLayout Remove(View view)
        {
            return Remove(view, 1);
        }

        public LinearLayout Add(View view, float weight)
        {
            views.Add(view);
            weights.Add(weight);
            return this;
        }

        public LinearLayout Remove(View view, float weight)
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
}

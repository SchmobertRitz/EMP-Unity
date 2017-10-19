//
// MIT License
// Copyright (c) EMP - https://github.com/SchmobertRitz/EMP-Unity
//
using UnityEngine;

namespace EMP.Forms
{
    public class GridLayout : ViewGroup<GridLayout>
    {
        public enum EOrientation
        {
            Horizontal, Vertical
        }

        private readonly int rows;
        private readonly EOrientation orientation;

        public GridLayout(int rows, EOrientation orientation) : base()
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
            }
            else
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
}

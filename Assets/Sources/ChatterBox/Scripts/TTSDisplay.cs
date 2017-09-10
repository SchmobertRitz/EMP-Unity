//
// MIT License
// Copyright (c) EMP - https://github.com/SchmobertRitz/EMP-Unity
//
using UnityEngine;

namespace EMP.ChatterBox
{
    public class TTSDisplay : MonoBehaviour
    {
        public virtual void SetText(string text)
        {
            TextMesh textMesh = GetComponent<TextMesh>();
            if (textMesh)
            {
                textMesh.text = text == null ? "" : text;
            }
            else
            {
                Debug.Log("No text mesh found. text: " + text);
            }
        }
    }
}

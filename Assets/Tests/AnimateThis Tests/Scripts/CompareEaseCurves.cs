//
// MIT License
// Copyright (c) EMP - https://github.com/SchmobertRitz/EMP-Unity
//
using EMP.Animations;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
// Logger definition here

namespace EMP
{
    public class CompareEaseCurves : MonoBehaviour
    {
        void Start()
        {
            Transform ball1 = GameObject.Find("Ball1").transform;
            Transform ball2 = GameObject.Find("Ball2").transform;
            Transform ball3 = GameObject.Find("Ball3").transform;

            AnimateThis.With(ball1)
                .Transform()
                .ToPosition(ball1.position + Vector3.down * 3)
                .Duration(2)
                .Start();

            AnimateThis.With(ball2)
                .Transform()
                .ToPosition(ball2.position + Vector3.down * 3)
                .Ease(AnimateThis.EaseInOutSmooth)
                .Duration(2)
                .Start();

            AnimateThis.With(ball3)
                .Transform()
                .ToPosition(ball3.position + Vector3.down * 3)
                .Ease(AnimateThis.EaseOutBounce)
                .Duration(2)
                .Start();
        }
    }
}

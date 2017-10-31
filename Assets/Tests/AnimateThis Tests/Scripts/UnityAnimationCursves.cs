//
// MIT License
// Copyright (c) EMP - https://github.com/SchmobertRitz/EMP-Unity
//
using EMP.Animations;
using UnityEngine;
// Logger definition here

namespace EMP
{
    public class UnityAnimationCursves : MonoBehaviour
    {
        [SerializeField]
        private AnimationCurve Curve;

        void Start()
        {
            Transform ball1 = GameObject.Find("Ball1").transform;

            AnimateThis.With(ball1)
                .Transform()
                .ToPosition(ball1.position + Vector3.down * 3)
                .Ease(Curve.Evaluate)
                .Duration(3)
                .Start();
        }
    }
}

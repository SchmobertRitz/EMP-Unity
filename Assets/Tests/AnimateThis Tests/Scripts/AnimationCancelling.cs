//
// MIT License
// Copyright (c) EMP - https://github.com/SchmobertRitz/EMP-Unity
//
using EMP.Animations;
using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace EMP
{
    public class AnimationCancelling : MonoBehaviour
    {
        private void Start()
        {
            StartCoroutine(StartAnimation());
        }

        private IEnumerator StartAnimation()
        {
            AnimateThis.With(this)
                .Transform()
                .ToScale(3).Duration(10).Start();

            while(true)
            {
                AnimateThis.With(this).CancelByTag("Transformation");

                AnimateThis.With(this)
                    .Transform()
                    .Tag("Transformation")
                    .ToPosition(UnityEngine.Random.insideUnitSphere * 2).Duration(3)
                    .Ease(AnimateThis.EaseSmooth).OnCancel(() => Debug.Log("Cancelled"))
                    .Start();

                AnimateThis.With(this)
                    .Transform()
                    .Tag("Transformation")
                    .ToRotation(UnityEngine.Random.rotationUniform)
                    .Duration(3)
                    .Ease(AnimateThis.EaseSmooth).OnCancel(() => Debug.Log("Cancelled"))
                    .Start();

                yield return new WaitForSeconds(3 * UnityEngine.Random.value);
            }
        }

        private IEnumerator StartAnimation2()
        {
            AnimateThis.Animation a1 = null;

            AnimateThis.With(this)
                .Transform()
                .ToScale(3).Duration(10).Start();

            while (true)
            {
                a1 = AnimateThis.With(this)
                    .Cancel(a1)
                    .Transform()
                    .ToPosition(UnityEngine.Random.insideUnitSphere * 2).Duration(3)
                    .Ease(AnimateThis.EaseSmooth).OnCancel(() => Debug.Log("Cancelled"))
                    .Start();

                yield return new WaitForSeconds(3 * UnityEngine.Random.value);
            }
        }
    }
}

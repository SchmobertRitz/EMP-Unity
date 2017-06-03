using UnityEngine;
using System.Collections.Generic;
using System;

namespace EMP.Animations
{
    public interface IAnimateThis
    {
        AnimateThis.TransformAnimationBuilder Transformate();
        AnimateThis.ValueAnimationBuilder Do(Action<float> handler);
        IAnimateThis CancelAll();
        IAnimateThis Cancel(AnimateThis.Animation animation);
    }

    public class AnimateThis : MonoBehaviour, IAnimateThis
    {

        public static IAnimateThis With(MonoBehaviour obj)
        {
            return CreateIfNull(obj.GetComponent<AnimateThis>(), obj.transform);
        }

        public static IAnimateThis With(Transform obj)
        {
            return CreateIfNull(obj.GetComponent<AnimateThis>(), obj);
        }

        private static IAnimateThis CreateIfNull(AnimateThis instance, Transform t)
        {
            if (instance != null)
            {
                return instance;
            }
            return t.gameObject.AddComponent<AnimateThis>();
        }

        public static float EasePow2(float t)
        {
            return t * t;
        }

        public static float EaseOutElastic(float t)
        {
            float p = 0.3f;
            return Mathf.Pow(2, -10 * t) * Mathf.Sin((t - p / 4) * (2 * Mathf.PI) / p) + 1;
        }

        public static float EaseInOutSinus(float t)
        {
            return (Mathf.Sin(t * Mathf.PI - Mathf.PI / 2) + 1) / 2;
        }

        public static float EaseInOutSmooth(float t)
        {
            float sqt = t * t;
            return sqt / (2.0f * (sqt - t) + 1.0f);
        }

        public static float EaseOutQuintic(float t)
        {
            return (t - 1f) * (t - 1f) * (t - 1f) * (t - 1f) * (t - 1f) + 1f;
        }

        public static float EaseInQuintic(float t)
        {
            return t * t * t * t * t;
        }

        public static float EaseSmooth(float t)
        {
            return EaseInOutSinus(t);
        }
        
        public interface IAnimatable
        {
            void DoAnimFrame(float t);
            void DoEndAnim();
            void DoStartAnim();
        }

        internal class TransformAnimatable : IAnimatable
        {
            public Transform transform;
            public Vector3? posFrom, posTo;
            public Vector3? scaleFrom, scaleTo;
            public Quaternion? rotFrom, rotTo;

            public TransformAnimatable(Transform transform)
            {
                this.transform = transform;
            }

            public void DoAnimFrame(float t)
            {
                if (posFrom != null && posTo != null)
                {
                    transform.localPosition = Vector3.LerpUnclamped(posFrom ?? Vector3.zero, posTo ?? Vector3.zero, t);
                }
                if (scaleFrom != null && scaleTo != null)
                {
                    transform.localScale = Vector3.LerpUnclamped(scaleFrom ?? Vector3.zero, scaleTo ?? Vector3.zero, t);
                }
                if (rotFrom != null && rotTo != null)
                {
                    transform.localRotation = Quaternion.LerpUnclamped(rotFrom ?? Quaternion.identity, rotTo ?? Quaternion.identity, t);
                }
            }

            public void DoEndAnim()
            {
                if (posFrom != null && posTo != null)
                {
                    transform.localPosition = posTo ?? Vector3.zero;
                }
                if (scaleFrom != null && scaleTo != null)
                {
                    transform.localScale = scaleTo ?? Vector3.zero;
                }
                if (rotFrom != null && rotTo != null)
                {
                    transform.localRotation = rotTo ?? Quaternion.identity;
                }
            }

            public void DoStartAnim()
            {
                if (posFrom == null)
                {
                    posFrom = transform.localPosition;
                }
                if (scaleFrom == null)
                {
                    scaleFrom = transform.localScale;
                }
                if (rotFrom == null)
                {
                    rotFrom = transform.localRotation;
                }
                if (posFrom != null && posTo != null)
                {
                    transform.localPosition = posFrom ?? Vector3.zero;
                }
                if (scaleFrom != null && scaleTo != null)
                {
                    transform.localScale = scaleFrom ?? Vector3.zero;
                }
                if (rotFrom != null && rotTo != null)
                {
                    transform.localRotation = rotFrom ?? Quaternion.identity;
                }
            }
        }

        public class Animation
        {
            internal IAnimatable animatable;
            public delegate float EaseFunction(float t);
            internal EaseFunction easeFunction;
            internal float timeStart;
            internal float timeStop;
            internal Action onAnimationStart;
            internal Action onAnimationEnd;
            internal Action onAnimationCancelled;
            internal bool isPlaying;
            internal bool isCanceled;
        }

        public abstract class AnimationBuilder
        {
            protected AnimateThis animator;
            protected IAnimatable animatable;
            internal float startDelay = 0;
            internal float duration = 1;
            internal Animation.EaseFunction easeFunction;
            internal Action onAnimationStartAction;
            internal Action onAnimationEndAction;
            internal Action onAnimationCancelledAction;
            internal AnimationBuilder delegateBuilder;

            public Animation Start()
            {
                if (delegateBuilder != null)
                {
                    return delegateBuilder.Start();
                }
                else
                {
                    return CreateAndStartAnimation();
                }
            }

            internal Animation CreateAndStartAnimation()
            {
                Animation result = new Animation();
                float t = Time.time;
                result.animatable = animatable;
                result.timeStart = t + startDelay;
                result.timeStop = result.timeStart + duration;
                result.easeFunction = easeFunction;
                result.onAnimationStart = onAnimationStartAction;
                result.onAnimationEnd = onAnimationEndAction;
                result.onAnimationCancelled = onAnimationCancelledAction;

                if (result.timeStart == result.timeStop)
                {
                    if (onAnimationStartAction != null)
                    {
                        onAnimationStartAction();
                    }
                    result.animatable.DoEndAnim();
                    if (onAnimationEndAction != null)
                    {
                        onAnimationEndAction();
                    }
                }
                else
                {
                    animator.Add(result);
                }

                return result;
            }

            public IAnimateThis Then()
            {
                IAnimateThis chainedAnimation = new AnimationChainer(animator, this);
                return chainedAnimation;
            }
        }

        public abstract class GenericAnimationBuilder<T> : AnimationBuilder where T : GenericAnimationBuilder<T>
        {
            protected GenericAnimationBuilder(AnimateThis animator, IAnimatable animatable)
            {
                this.animator = animator;
                this.animatable = animatable;
            }

            public T Duration(float duration)
            {
                this.duration = duration;
                return (T)this;
            }

            public T Delay(float delay)
            {
                startDelay = delay;
                return (T)this;
            }

            public T Ease(Animation.EaseFunction easeFunction)
            {
                this.easeFunction = easeFunction;
                return (T)this;
            }

            public T OnStart(Action onStartDelegate)
            {
                onAnimationStartAction = onStartDelegate;
                return (T)this;
            }

            public T OnEnd(Action onEndDelegate)
            {
                onAnimationEndAction = onEndDelegate;
                return (T)this;
            }

        }

        private class AnimationChainer : IAnimateThis
        {
            private readonly IAnimateThis delegateInstance;
            private readonly AnimationBuilder delegateBuilder;

            public AnimationChainer(IAnimateThis delegateInstance, AnimationBuilder delegateBuilder)
            {
                this.delegateInstance = delegateInstance;
                this.delegateBuilder = delegateBuilder;
            }

            public IAnimateThis Cancel(Animation animation)
            {
                delegateInstance.Cancel(animation);
                return this;
            }

            public IAnimateThis CancelAll()
            {
                delegateInstance.CancelAll();
                return this;
            }

            public ValueAnimationBuilder Do(Action<float> handler)
            {
                ValueAnimationBuilder builder = delegateInstance.Do(handler);
                LinkBuilders(builder);
                return builder;
            }

            public TransformAnimationBuilder Transformate()
            {
                TransformAnimationBuilder builder = delegateInstance.Transformate();
                LinkBuilders(builder);
                return builder;
            }

            private void LinkBuilders(AnimationBuilder builder)
            {
                builder.delegateBuilder = delegateBuilder;
                Action startNextAnbimationAction = () => { builder.CreateAndStartAnimation(); };
                if (delegateBuilder.onAnimationEndAction != null)
                {
                    Action oldAction = delegateBuilder.onAnimationEndAction;
                    delegateBuilder.onAnimationEndAction = () => { oldAction(); builder.CreateAndStartAnimation(); };
                }
                else
                {
                    delegateBuilder.onAnimationEndAction = () => { builder.CreateAndStartAnimation(); };
                }
            }
        }

        public class TransformAnimationBuilder : GenericAnimationBuilder<TransformAnimationBuilder>
        {
            TransformAnimatable transformAnimatable;
            internal TransformAnimationBuilder(AnimateThis animator, TransformAnimatable animatable) : base(animator, animatable)
            {
                this.transformAnimatable = animatable;
            }

            public TransformAnimationBuilder ToPosition(Vector3 posTo)
            {
                transformAnimatable.posTo = posTo;
                return this;
            }

            public TransformAnimationBuilder FromPosition(Vector3 posFrom)
            {
                transformAnimatable.posFrom = posFrom;
                return this;
            }

            public TransformAnimationBuilder ToScale(Vector3 scaleTo)
            {
                transformAnimatable.scaleTo = scaleTo;
                return this;
            }

            public TransformAnimationBuilder ToRotation(Quaternion rotTo)
            {
                transformAnimatable.rotTo = rotTo;
                return this;
            }

            public TransformAnimationBuilder FromRotation(Quaternion rotFrom)
            {
                transformAnimatable.rotFrom = rotFrom;
                return this;
            }

            public TransformAnimationBuilder FromScale(float scaleFrom)
            {
                return FromScale(Vector3.one * scaleFrom);
            }

            public TransformAnimationBuilder ToScale(float scaleTo)
            {
                return ToScale(Vector3.one * scaleTo);
            }

            public TransformAnimationBuilder FromScale(Vector3 scaleFrom)
            {
                transformAnimatable.scaleFrom = scaleFrom;
                return this;
            }
        }

        public class ValueAnimatable : IAnimatable
        {
            public float valueStart = 0, valueEnd = 1;
            private Action<float> handler;

            public ValueAnimatable(Action<float> handler)
            {
                this.handler = handler;
            }

            public void DoAnimFrame(float t)
            {
                handler.Invoke(valueStart * (1 - t) + t * valueEnd);
            }

            public void DoEndAnim()
            {
                handler.Invoke(valueEnd);
            }

            public void DoStartAnim()
            {
                handler.Invoke(valueStart);
            }
        }

        public class ValueAnimationBuilder : GenericAnimationBuilder<ValueAnimationBuilder>
        {

            private ValueAnimatable valueAnimatable;
            public ValueAnimationBuilder(AnimateThis animator, ValueAnimatable animatable) : base(animator, animatable)
            {
                valueAnimatable = animatable;
            }

            public ValueAnimationBuilder From(float value)
            {
                valueAnimatable.valueStart = value;
                return this;
            }

            public ValueAnimationBuilder To(float value)
            {
                valueAnimatable.valueEnd = value;
                return this;
            }
        }

        private List<Animation> animations = new List<Animation>();

        public IAnimateThis CancelAll()
        {
            animations.Clear();
            return this;
        }

        public IAnimateThis Cancel(Animation animation)
        {
            animations.Remove(animation);
            return this;
        }

        public TransformAnimationBuilder Transformate()
        {
            return new TransformAnimationBuilder(this, new TransformAnimatable(transform));
        }

        public ValueAnimationBuilder Do(Action<float> handler)
        {
            return new ValueAnimationBuilder(this, new ValueAnimatable(handler));
        }

        private void Add(Animation a)
        {
            animations.Add(a);
        }

        void Update()
        {
            float t = Time.time;
            for (int i = animations.Count - 1; i >= 0; i--)
            {
                Animation a = animations[i];
                if (a.isCanceled)
                {
                    animations.RemoveAt(i);
                    if (a.onAnimationCancelled != null)
                    {
                        a.onAnimationCancelled();
                    }
                }
                else if (t >= a.timeStop)
                {
                    animations.RemoveAt(i);
                    a.animatable.DoEndAnim();
                    if (a.onAnimationEnd != null)
                    {
                        a.onAnimationEnd();
                    }
                }
                else if (t >= a.timeStart)
                {
                    if (!a.isPlaying)
                    {
                        a.animatable.DoStartAnim();
                        if (a.onAnimationStart != null)
                        {
                            a.onAnimationStart();
                        }
                        a.isPlaying = true;
                    }
                    float interpolate = (t - a.timeStart) / (a.timeStop - a.timeStart);
                    if (a.easeFunction == null)
                    {
                        a.animatable.DoAnimFrame(interpolate);
                    }
                    else
                    {
                        a.animatable.DoAnimFrame(a.easeFunction(interpolate));
                    }

                }
            }
            if (animations.Count == 0)
            {
                Destroy(this);
            }
        }
    }
}
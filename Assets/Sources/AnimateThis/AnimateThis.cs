using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using System;

public class AnimateThis : MonoBehaviour {

    public static AnimateThis With(MonoBehaviour obj)
    {
        return CreateIfNull(obj.GetComponent<AnimateThis>(), obj.transform);
    }

    public static AnimateThis With(Transform obj)
    {
        return CreateIfNull(obj.GetComponent<AnimateThis>(), obj);
    }

    private static AnimateThis CreateIfNull(AnimateThis instance, Transform t)
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

	public interface Animatable
	{
		void DoAnimFrame(float t);
	}

	public class TransformAnimatable : Animatable
	{
		public Transform transform;
		public Vector3 posFrom, posTo;
		public Vector3 scaleFrom, scaleTo;
		public Quaternion rotFrom, rotTo;

		public TransformAnimatable(Transform transform)
		{
			this.transform = transform;
			posFrom = posTo = transform.localPosition;
			scaleFrom = scaleTo = transform.localScale;
			rotFrom = rotTo = transform.rotation;
		}

		public void DoAnimFrame(float t)
		{
			if (posFrom != posTo)
			{
				transform.localPosition = Vector3.LerpUnclamped(posFrom, posTo, t);
			}
			if (scaleFrom != scaleTo)
			{
				transform.localScale = Vector3.LerpUnclamped(scaleFrom, scaleTo, t);
			}
			if (rotFrom != rotTo)
			{
				transform.localRotation = Quaternion.LerpUnclamped(rotFrom, rotTo, t);
			}
		}

		public void DoEndAnim(Animation a)
		{
			if (posFrom != posTo)
			{
				transform.localPosition = posTo;
			}
			if (scaleFrom != scaleTo)
			{
				transform.localScale = scaleTo;
			}
			if (rotFrom != rotTo)
			{
				transform.localRotation = rotTo;
			} 
		}

		public void DoInitAnim(Animation a)
		{
		}

		public void DoStartAnim(Animation a)
		{
			if (posFrom != posTo)
			{
				transform.localPosition = posFrom;
			}
			if (scaleFrom != scaleTo)
			{
				transform.localScale = scaleFrom;
			}
			if (rotFrom != rotTo)
			{
				transform.localRotation = rotFrom;
			}
		}
	}

	public class Animation
	{
		public Animatable animatable;
		public delegate float EaseFunction(float t);
		public EaseFunction easeFunction;
		public float timeStart;
		public float timeStop;
		public Action onAnimationStart;
		public Action onAnimationEnd;
		public Action onAnimationCancelled;
		public bool isPlaying;
		public bool isCanceled;
	}

	public abstract class AnimationBuilder<T> where T : AnimationBuilder<T>
	{
		protected AnimateThis animator;
		protected Animatable animatable;
		private float startDelay = 0;
		private float duration = 1;
		private Animation.EaseFunction easeFunction;
		private Action onAnimationStartAction;
		private Action onAnimationEndAction;
		private Action onAnimationCancelledAction;

		protected AnimationBuilder(AnimateThis animator, Animatable animatable)
		{
			this.animator = animator;
			this.animatable = animatable;
		}

		public T Duration(float duration)
		{
			this.duration = duration;
			return (T) this;
		}

		public T Delay(float delay)
		{
			startDelay = delay;
			return (T)this;
		}

		public T Ease(Animation.EaseFunction easeFunction)
		{
			this.easeFunction = easeFunction;
			return (T) this;
		}

        public T OnStart(Action onStartDelegate)
        {
            this.onAnimationStartAction = onStartDelegate;
            return (T)this;
        }

        public T OnEnd(Action onEndDelegate)
        {
            this.onAnimationEndAction = onEndDelegate;
            return (T)this;
        }

        public Animation Start()
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
			animator.Add(result);
			return result;
		}
	}

	public class TransformAnimationBuilder : AnimationBuilder<TransformAnimationBuilder>
	{
		private TransformAnimatable transformAnimatable;
		public TransformAnimationBuilder(AnimateThis animator, TransformAnimatable animatable) : base(animator, animatable)
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

    public class ValueAnimatable : Animatable
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
    }

    public class ValueAnimationBuilder : AnimationBuilder<ValueAnimationBuilder>
    {

        private ValueAnimatable valueAnimatable;
        public ValueAnimationBuilder(AnimateThis animator, ValueAnimatable animatable) : base(animator, animatable)
        {
            this.valueAnimatable = animatable;
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

	public AnimateThis CancelAll()
	{
		animations.Clear();
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

	public void Add(Animation a)
	{
		animations.Add(a);
	}

	void Update () {
		float t = Time.time;

		for(int i = animations.Count - 1; i >= 0; i--)
		{
			Animation a = animations[i];
			if (a.isCanceled)
			{
				animations.RemoveAt(i);
				if (a.onAnimationCancelled != null)
				{
					a.onAnimationCancelled();
				}
			} else if (t >= a.timeStop)
			{
				animations.RemoveAt(i);
				a.animatable.DoAnimFrame(1);
				if (a.onAnimationEnd != null)
				{
					a.onAnimationEnd();
				}
			} else if (t >= a.timeStart)
			{
				if (!a.isPlaying)
				{
					if (a.onAnimationStart != null)
					{
						a.onAnimationStart();
					}
					a.isPlaying = true;
				}
				float interpolate = (t - a.timeStart) / (a.timeStop - a.timeStart);
				if (a.easeFunction == null) {
					a.animatable.DoAnimFrame(interpolate);
				} else
				{
					a.animatable.DoAnimFrame(a.easeFunction(interpolate));
				}

			}
		}
	}
}

# AnimateThis # 


Animate this is a lightweight, simple to use animation framework for interpolation based animations in Unity3D. You can animate the Transform properties like scale, position and rotation and also more specific properties like the volume of a AudioSource component. In one sentence: You can almost animate everything you want to have changed over time with an easy C# API.

## API-Documentation ##

### The Class AnimateThis ###

The class `AnimateThis` is the main API entry point of the framework. It provides a couple of static methods that allow to create animation builders. Animation builders are objects that help you to define your animation parameters in a convenient way. For more information about the Builder Pattern, please have a look at [Wikipedia/BuilderPattern](https://en.wikipedia.org/wiki/Builder_pattern "Wikipedia - Builder Pattern").

The method `AnimateThis.With(...)` allows you to create an animation builder. The animation builder is bound to the object given as parameter to the `With`-method.

The following example shows the usage of the `AnimateThis.With` method.

```
AnimateThis.With(this)
    .Transform()
    .ToScale(0)
    .OnEnd(() => Destroy(this.gameObject))
    .Start();
```

In this code snippet, a tranform animation builder is created and configured to perform a scale animation that shrinks the Transform of the current MonoBehaviour to a scale of (0, 0, 0) and then - after finishing the scaling - destroys the GameObject. This is a common use case for getting a fancy effect while removing objects from your runing game or application.

#### Transform Animations ####

The `Transform()` method determins, that you want to create an transformation based animation. Transformation based anmations allow you to scale, translate and rotate GameObjects. Note: All transformations are done in the local coordinate space of the Transform object.

The Method `ToScale(float)` defines the scale value, you want to approach during the animation. Note that the parameter is a scalar float value while, in Usinty, a proper scale value is defined as a Vector3. Of course you can also use a Vector as parameter. Using a float value in the `ToScale(float factor)` is simply a shortcut for `ToScale(transform.localScale * factor)`.
There also exists a matching method `FromScale(float)`. As you suspect correctly, this method defines the scale value that should be the start value for the animation. The From-methods can always be omitted for transformation based animations - in this case, the current transformation value is used as animation start point.

The `OnEnd(() => {})` method is an handler that will be called as soon as the animation is finished (after the last frame). There is also a corresponding `OnStart(() => {})`method, that is called when the animation starts (before the first frame).

In the example above, you see no information about how long the animation should take. It will take 1 second, because this is the default value. You are free to specify your prefered animation length using the `Duration(float)`method. The time unit is  seconds, as usual in Unity. 

If you want to delay the execution of your animation, you can use the `Delay(float)` method.

When running the animation example in Unity, you will see that the object you animate shrinks with a constant speed: At the start time of the animation, it will have a scale of (1, 1, 1), after a half second, the scale will be (0.5, 0.5, 0.5) and after one second, the scale will be (0, 0, 0). This linear scaling tends to be a bit of boring, so you should alter the so called ease curve of the animation. Use the method `Ease(float => float)` to set an ease curve that makes the animation a bit more dnamic. An ease curve is simply a function that maps a float from the range of [0, 1] to another float.. mostly in the same range [0, 1]. More about ease curves will follow in a seperat section.

This code example shows how to use the methods described in the section above.

```csharp
   AnimateThis.With(this)
      .Transform()
      .FromScale(transform.localScale)
      .ToScale(0)
      .OnEnd(() => Destroy(this.gameObject))
      .Delay(0.125f)
      .Duration(0.25f)
      .Ease(AnimateThis.EaseInQuintic)
      .Start();
```

The same way you animate the Transforms scale, you also can animate the Tranforms position and rotation:

* `FromScale(float)`, `ToScale(float)`
* `FromScale(Vector3)`, `ToScale(Vector3)`
* `FromPosition(Vector3)`, `ToPosition(Vector3)`
* `FromRotation(Quaternion)`, `ToRotation(Quaternion)`

#### Value Animations ####

You are not restricted to Transforms when animation your objects. You can animate everything that is somehow numeric using the `Value(...)` method instead of the `Transfom(...)` method

```
        // Scaling up the font size of the text mesh
        TextMesh txt = GetComponent<TextMesh>();

        AnimateThis.With(txt.transform)
            .Value(size => txt.fontSize = (int)size)
            .From(0)
            .To(100)
            .Duration(2)
            .Ease(t => t * t)
            .Start();
```

This examples show how to use the `Value(...)` method for scaling up the font size of a text mesh from 0 to 100. The animation takes 2 seconds and the rising speed of the font size starts slowy and then increases, because of the easing curve `t => t * t` (As you see, you can use every function or method `float => float` as ease curve).

The methods `Delay`, `OnStart` and `OnEnd` are also available and can be used in the same way as shown in the section about transformation animation. You even can omit the `From` and the `To` method calls. In this case, the start value of your animation is set to `0` and the end value is set to `1`.

This way, you can animate nearly everything, e.g. the emission value of a texture or the field of view of your main camera.

#### Audio Animations ####

You already know the value animations. Of course, you can use the value animations feature to modify the properties of an Unity AudioSource, but since this is a very common usecase, there is a specialized API available to support this usecase in a convenient way.

```chsarp
AnimateThis.With(obj)
	.Audio()
    .ToVolume(0)
    .Start();
```

Currently supported AudioSource properties are:

* Volume: `FromVolume(float)` and `ToVolume(float)`
* Stereo Pan: `FromPan(float)` and `ToPan(float)`
* Spatial Blend: `FromTo(float)` and `ToSpatial(float)`

### Canceling Animations ###

You can add multiple animations to the same object at the same time. This is very useful if you want to change different object properties (e.g. position and scale) with different ease curves or animation durations. But if two animations change the same properties of an object, the second animation will overwrite the changes made by the first animation. 

In this case, it would make sense to cancel the first animation before the second animation is started.

#### Cancel All Animations ####

The following example uses the `CancelAll(bool callOnEndHandler = false)` method to prevent animation overlapping. This method call cancels all running animations attached to a GameObject. You can get a callback about cancelled animations if you register a handler via the `OnCancel` method. If you set the `callOnEndHandler` flag to `true`, the on end handler of the animation is called additionally to the cancel handler.

```csharp
private IEnumerator StartAnimation()
        {
            while(true)
            {
                AnimateThis.With(this)
                    .CancelAll()
                    .Transform()
                    .ToPosition(UnityEngine.Random.insideUnitSphere * 2)
                    .Duration(3)
                    .Ease(AnimateThis.EaseSmooth)
                    .OnCancel(() => Debug.Log("Cancelled"))
                    .Start();

                yield return new WaitForSeconds(3 * UnityEngine.Random.value);
            }
        }
```

As you see, the `CancelAll` call is done in the same method chain as the rest of the animation building. For better readability, you are free to do this two steps in two separat calls:

```csharp
AnimateThis.With(this).CancelAll();
                
AnimateThis.With(this)
	.Transform()
    (...)
    .Start();
```

#### Cancel Specific Animations ####

Often, you don't want to cancel all animations on a GameObject at once - you want to cancel a specific animation. For this usecase, you can use the `Cancel(AnimateThis.Animation animation, bool callOnEndHandler = false)` method.

```csharp
private IEnumerator StartAnimation()
        {
            AnimateThis.With(this)
                .Transform()
                .ToScale(3).Duration(10).Start();

            AnimateThis.Animation a1 = null; // stores the last animation that should be cancelled.

            while(true)
            {
                a1 = AnimateThis.With(this)
                    .Cancel(a1) // cancel only the one animation stored
                    .Transform()
                    .ToPosition(UnityEngine.Random.insideUnitSphere * 2).Duration(3)
                    .Ease(AnimateThis.EaseSmooth).OnCancel(() => Debug.Log("Cancelled"))
                    .Start();

                yield return new WaitForSeconds(3 * UnityEngine.Random.value);
            }
        }
```

As you see in the example, a reference to the translation animation is stored into a variable `a1` and given to the `Cancel` method in the next iteration of the while loop. The long running scaling animation started at the beginning of the method is not cancelled accidentally.

If you don't want to store animations into variables or if you have a set of animation that should be cancelled at once, you can use tags to specify the set of animation that should be cancelled.

```csharp
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
```

In this example, the while loop starts with a `CancelByTag` call. This call cancels all animations that are tagged with the string `Transformation` (Btw, You don't need to use strings, every object can be used as tag). The second important step is the tag assignment done with the line `.Tag("Transformation")`.

### Sequencing Animations ###

Sometimes, you want to happen a couple of animations in sequence: E.g.: First, a GameObject should pop in, then, it sould move to a certain position and after that, it should pop out again.

You could reach this effect by chaining the animations using the `OnEnd` handler. But this tends to result in ugly code, because you have to nest the code of the following animations in the `OnEnd` handler of the preceding animation.

This is why, there is an API support available for concatenating animations.

```chsarp
        AnimateThis.With(obj).
            Transform()
            .ToPosition(Vector3.down)
            .Duration(1)
            
            .Then().Transform()
            .ToPosition(Vector3.up)
            
            .Then().Transform()
            .ToScale(0)
            
            .Then().Audio()
            .ToVolume(0)
            
            .Start();
```
In this example, an object is first moved down about 1 meter, and then moved up about the same distance. After doing this, the object vanishes, because its scaled to zero and after the object got invisible, the AudioSource (that is attached to the object) is faded to mute.

### A Little Deeper Into Easing Curves ###

In our physical world, almost everything that starts moving accelerates and almost everything that stops moving slows down before the movement ends. This effects are the result of the interaction between factors like mass, force and drag. Animations with AnimateThis use interpolation over time to calculate the animation progress and does not simulate physical behaviour. This leads to a constant change rate of the animated properties between every frame of the animation. The visual impression of such linear animations look unnatural.

You can use [ease curves](http://easings.net/de, "Ease Curve Cheat Sheet") to modify the animation progress for a more physical like impression of your animations. Easing is a surprising prowerful technique, you can smooth the movements of objects and you can even simulate elastic and bouncing physical effects.

The following example shows 3 GameObjects named "Ball 1-3". Every ball should fall from its current position to a position 3 meters lower. Ball 1 uses no ease curve and will move linear. Ball 2 will accelerate slowly and decelerate before it reaches his end position. Ball 3 uses an ease curve that simulates a elastic bounce effect. Look at the corresponding Unity Example Scene to get an impression of the big influence the choosen ease curve has on the animation. 

```csharp
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
```


AnimateThis uses a very simple ease curve format: Every function `float => float` that maps a float from the range of [0, 1] to another float (mostly in the same range [0, 1]) can be used. Normally, an ease curve maps the value `0` to `0` and the value `1` to `1` and all other values in a range between 0 and 1. To define your own curves, you should be fit in trigonometry. If you don't like mathematics or your desired ease curve is too complex for quickly finding a good mathematical solution, you can also use the good old Unity AnimationCurves and the full power of the Curve Editor UI. Just define the curve in the editor and hand-over the `Evaluate` method to the AnimateThis framework:

```csharp
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
```

Have fun!

## Tips, Known Bugs And Common Pitfalls ##


### Collider And Zero-Scale ###

Look at the following snippet:

```csharp
   AnimateThis.With(this)
      .Transform()
      .ToScale(0) // better use Vector3.one * 0.00001f
      .OnEnd(() => Destroy(this.gameObject))
      .Start();
```

It's tempting to use this code for letting objects disappear in a cool way. But be careful if the object has colliders attached to itself or to one of its child. The Physics Engine used by Unity has a randomly appearing bug that crashes your Standalone Build when an object with collider has a scale of zero. Thus, you should use a very small scale factor instead of zero if you have an object with colliders.
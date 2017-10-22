AnimateThis
========

Animate this is a lightweight, simple to use animation framework for interpolation based animations in Unity3D. You can animate the Transform properties like scale, position and rotation and also more specific properties like the light intensity of a Light component or the volume of a AudioSource component. In one sentence: You can almost animate everything you want to have changed over time.

API-Documentation
----------------------

### The Class AnimateThis ###

The class `AnimateThis` is the main API entry point of the framework. It provides a couple of static methods that allow to create animation builders. Animation builders are objects that help you to define your Animation Parameters in a convenient way. For more information about the Builder Pattern, please have a look at [Wikipedia/BuilderPattern](https://en.wikipedia.org/wiki/Builder_pattern "Wikipedia - Builder Pattern").

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

The `Transform()` method determins, that you want to create an transformation based animation. Transformations based anmations allow you to scale, translate and rotate GameObjects. Note: All transformations are done in the local coordinate space of the Transform object.

The Method `ToScale(float)`defines the scale value, you want to approach during the animation. Note that the parameter is a scalar float value while, in Usinty, a proper scale value is defined as a Vector3. Using a float value in the `ToScale(float factor)` is simply a shortcut for `ToScale(transform.localScale * factor)`.  
There also exists a matching method `FromScale(float)`. As you suspect correctly, this method defines the scale value that should be the start value for the animation. The From-methods can always be omitted for transformation based animations - in this case, the current transformation value is used as animation start point.

The `OnEnd(() => {})` method is an handler that will be called as soon as the animation is finished (after the last frame). There is also a corresponding `OnStart(() => {})`method, that is called when the animation starts (before the first frame).

In the example above, you see no information about how long the animation should take. It will take 1 second, because this is the default value. You are free to specify your prefered animation length using the `Duration(float)`method. The time unit is 1 second, as usual in Unity. 

If you want to delay the execution of your animation, you can use the `Delay(float)` method.

When running the animation example in Unity, you will see that the object you animate shrinks with a constant speed: At tha start time of the animation, it will have a scale of (1, 1, 1), after a half second, the scale will be (0.5, 0.5, 0.5) and after one second, the scale will be (0, 0, 0). This linear scaling tends to be a bit of boring, so you should alter the so called ease curve of the animation. Use the method `Ease(float => float)` to set an ease curve that makes the animation a bit more dnamic. A ease curve is simply a function that maps a float in the range of [0, 1] to another float.. mostly in the same range [0, 1]. More about ease curves will follow in a seperat section.

This code example shows how to use the methods described in the section above.

```
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

You are not clamped to Transforms when animation your objects. You can animate everything that is somehow numeric using the `Value(...)` method instead of the `Transfom(...)` method

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

The methods `Dealay`, `OnStart` and `OnEnd` are also available and can be used in the same way as shown in the section about transformation animation. You even can omit the `From`and the `To` method calls. In this case, the start value of your animation is set to `0`and the end value is set to `1`.

This way, you can animate nearly everything, e.g. the emission value of a texture or the field of view of your main camera.

#### Audio Animation ####

You already know the value animations. Of course, you can use the value animations feature to modify the properties of an Unity AudioSource, but since this is a very common usecase, there is a specialized API available to support this usecase in a convenient way.

Currently supported AudioSource properties are:

* Volume: `FromVolume(float)`and `ToVolume(float)
* Stereo Pan: `FromPan(float)`and `ToPan(float)
* Spatial Blend: `FromTo(float)`and `ToSpatial(float)

#### Preview: Light Animations ####
A convenience API for modifying Lights is not available at the moment, but planned.

### Canceling Animations ###
TODO
### Sequencing Animations ###

Sometimes, you want to happen a couple of animations in sequence: E.g.: First, a GomeObject should pop in, then, it sould move to a certain position and after that, it should pop out again.

You could reach this effect by chainging the animations using the `OnEnd`handler. But this tends to result in ugly code, because you have to nest the code of the preceding animations in the `OnEnd`handler.

This is why, there is an API support available for concatenating animations.

```
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
TODO
### Known Bugs And Common Pitfalls ###
TODO
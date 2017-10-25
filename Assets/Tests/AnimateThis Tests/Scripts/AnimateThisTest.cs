using EMP.Animations;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class AnimateThisTest : MonoBehaviour {

    [SerializeField]
    private Transform obj;
    
	void Start () {
        AnimateThis.With(obj).
            Transform()
            .ToPosition(Vector3.down)
            .Duration(1)
            .Then()
            .Transform()
            .ToPosition(Vector3.up)
            .Then()
            .Transform()
            .ToScale(0)
            .Then()
            .Audio()
            .FromVolume(0.5f)
            .ToVolume(0)
            .Duration(float.PositiveInfinity)
            .Start();

        AnimateThis.With(this)
            .Transform()
            .ToScale(0)
            .OnEnd(() => Destroy(this.gameObject))
            .Delay(0.125f)
            .Duration(0.25f)
            .Ease(AnimateThis.EaseInQuintic)
            .Start();

        TextMesh txt = GetComponent<TextMesh>();
        AnimateThis.With(txt.transform)
            .Value(size => txt.fontSize = (int)size)
            .From(0)
            .To(100)
            .Duration(2)
            .Ease(t => t * t)
            .Start();



        Debug.Log("test");
	}

}

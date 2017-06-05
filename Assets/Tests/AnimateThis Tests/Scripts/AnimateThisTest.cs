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
        Debug.Log("test");
	}

}

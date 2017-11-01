using EMP.Animations;
using UnityEngine;

public class SequencingTest : MonoBehaviour {

	void Start () {
        AnimateThis.With(this).
            Transform()
            .ToPosition(Vector3.down)
            .Duration(1)
            .Then()
            .Transform()
            .ToPosition(Vector3.up)
            .Then()
            .Transform()
            .ToScale(0)
            .Start();
	}

}

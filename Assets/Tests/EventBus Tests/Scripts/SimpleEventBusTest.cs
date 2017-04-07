using EMP.EventBus;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class SimpleEventBusTest : MonoBehaviour {

	// Use this for initialization
	void Start () {
        Invoke("RandomColor", 1);
	}

    private void RandomColor()
    {
        EventBus.Post(new ChangeColorEvent(new Color(Random.value, Random.value, Random.value)));
        Invoke("RandomColor", 1);
    }
}

using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ComponentInstantiationTest : MonoBehaviour {
	void Start () {
        new Wire().InstantiateResource("FPS");
	}
}

using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Electron : MonoBehaviour {

    [Inject("Proton")]
    private GameObject proton;

    [Bind("Electron")]
    private GameObject electron;

    private float randomSpeed = 1;
    private Quaternion randomRotation;

    void Start () {
        electron.transform.position = proton.transform.position + Random.insideUnitSphere * 3;
        randomRotation = Quaternion.Euler(Random.insideUnitSphere * 10);
    }
	
	void Update () {
        transform.localRotation = transform.localRotation * randomRotation;
    }
}

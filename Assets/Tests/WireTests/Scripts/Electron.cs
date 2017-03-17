using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Electron : MonoBehaviour {

    [Inject("Proton")]
    private GameObject proton;

    private Vector3 randomSpeed;
    private Vector3 velocity;

    void Start () {
        randomSpeed = Random.insideUnitSphere;
        transform.position = proton.transform.position + Random.insideUnitSphere * 2;
	}
	
	void Update () {
        // I know that this model is bullshit ;-p
        Vector3 distanceVector = proton.transform.position - transform.position;
        float distance = distanceVector.magnitude;
        Vector3 gravity = distanceVector.normalized;
        velocity += (gravity * 1.5f + randomSpeed);
        transform.position = transform.position + Time.deltaTime * (velocity);
	}
}

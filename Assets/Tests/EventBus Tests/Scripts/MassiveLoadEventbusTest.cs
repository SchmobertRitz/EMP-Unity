using EMP.EventBus;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class MassiveLoadEventbusTest : MonoBehaviour {

    [SerializeField]
    private GameObject prototype;
    [SerializeField]
    private int count = 1000;

	// Use this for initialization
	void Start () {
		for(int i=0; i< count; i++)
        {
            Instantiate(prototype);
        }
        float timeStart = Time.realtimeSinceStartup;
        EventBus.Post(this);
        Debug.Log("Distaptching " + count + " events took " + (Time.realtimeSinceStartup - timeStart) + "seconds.");
	}
	
	// Update is called once per frame
	void Update () {
		
	}
}

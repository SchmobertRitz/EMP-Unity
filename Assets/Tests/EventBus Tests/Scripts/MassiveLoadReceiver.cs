using EMP.EventBus;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class MassiveLoadReceiver : MonoBehaviour {

	// Use this for initialization
	void Start () {
        EventBus.Register(this);
	}
	
	private void OnEvent(MassiveLoadEventbusTest e)
    {

    }
}

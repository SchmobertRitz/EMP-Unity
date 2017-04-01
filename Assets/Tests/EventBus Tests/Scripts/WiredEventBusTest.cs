using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class WiredEventBusTest : MonoBehaviour {

    [Inject]
    WiredEventBus bus;

	void Start () {
        Wire wire = new Wire();
        wire.Inject(this);
        wire.InstantiateResource("Color Changing Light");
        Invoke("RandomColor", 1);
	}

    private void RandomColor()
    {
        bus.Post(new ChangeColorEvent(new Color(Random.value, Random.value, Random.value)));
        Invoke("RandomColor", 1);
    }
}

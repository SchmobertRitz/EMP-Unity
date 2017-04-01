using UnityEngine;

public class ChangeMyColorWired : MonoBehaviour {

    [Inject]
    private WiredEventBus bus;

	void Start () {
        bus.Register(this);
	}

    void OnEvent(ChangeColorEvent e)
    {
        GetComponent<Light>().color = e.color;
    }
}

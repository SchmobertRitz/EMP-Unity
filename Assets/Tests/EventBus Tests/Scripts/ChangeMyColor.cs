using EMP.EventBus;
using UnityEngine;

public class ChangeMyColor : MonoBehaviour {

	void Start () {
        EventBus.Register(this);
	}

    void OnEvent(ChangeColorEvent e)
    {
        GetComponent<Light>().color = e.color;
    }
}

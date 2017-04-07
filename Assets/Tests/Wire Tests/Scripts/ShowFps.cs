using EMP.Wire;
using UnityEngine;

public class ShowFps : MonoBehaviour {

    [Inject]
    private FpsCounter counter;

    [Bind]
    private TextMesh textMesh;

	void Update () {
	   	if (textMesh)
        {
            textMesh.text = counter.Fps + " FPS";
        }
	}
}

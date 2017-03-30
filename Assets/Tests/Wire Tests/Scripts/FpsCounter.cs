using UnityEngine;

public class FpsCounter : MonoBehaviour {

    public int Fps
    { get; private set; }

    private int count;
    private int lastSecond;

	void Update () {
        count++;
	    if ((int) Time.time > lastSecond)
        {
            Fps = count;
            count = 0;
            lastSecond = (int)Time.time;
        }	
	}
}

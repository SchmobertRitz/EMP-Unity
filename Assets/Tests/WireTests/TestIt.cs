using UnityEngine;
using System.Collections;

public interface ITestIt { }
public class TestIt : MonoBehaviour, ITestIt
{

	void Start ()
    {
        
    }

    public void OnEvent(string text)
    {
        Debug.Log(text);
    } 
}

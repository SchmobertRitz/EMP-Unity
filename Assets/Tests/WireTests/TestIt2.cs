using System;
using UnityEngine;

public class TestIt2 : MonoBehaviour {

    private string test;

    [Inject("String2")]
    private string test2;

    [Inject]
    private TestIt2 test3
    {
        get; set;
    }

    void Start () {
        Wire wire = new Wire();
        wire.RegisterModule(this);
        Debug.Log(wire.Get<string>("String1"));
        Debug.Log(wire.Get<string>("String2"));
        Debug.Log(wire.Get<TestIt2>());
        wire.Inject(this);
        Debug.Log(test);
    }

    [Inject]
    private void InjectIt(TestIt2 thisObj, [Named("String1")] string text)
    {
        test = thisObj.ToString() + " -> " + text;
    }


    [Provides]
    private TestIt2 ProvideMe()
    {
        return this;
    }

    [Provides("String1")]
    private string ProvideString1()
    {
        return "String 1";
    }

    [Provides("String2")]
    private string ProvideString2([Named("String1")] string string1)
    {
        return string1 + " String 2";
    }
}

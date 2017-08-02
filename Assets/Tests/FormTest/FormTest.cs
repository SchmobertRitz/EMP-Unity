using EMP.Form;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class FormTest : MonoBehaviour {

    private Form form;

	void Start () {
        form = new Form();
        Linear l1 = new Linear(Linear.EOrientation.Vertical);
        Button a1 = new Button("A1");
        a1.FixedHeight = 100;
        l1.Add(a1);
        l1.Add(new Button("A2"));
        l1.Add(new Button("A3"));
        l1.FixedWidth = 100;

        TextField textField = new TextField("Test");
        form.Add(l1);
        form.Add(new Button("B"));
        form.Add(new Button("C", b => Debug.Log(textField.Text)));
        form.Add(textField);
    }

    void OnGUI()
    {
        Rect rect = new Rect(0, 0, Screen.width, Screen.height);
        form.OnGUI(rect);
    }

}

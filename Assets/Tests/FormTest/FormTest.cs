using EMP.Form;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class FormTest : MonoBehaviour {

    private Form form;

	void Start () {
        form = new Form();
        form.Spacing = 5;
        Label label = new Label("Name of Component:");
        label.FixedWidth = 180;
        TextField textField = new TextField();
        Linear componentName = Linear.Horizontal().Add(label).Add(textField);
        componentName.FixedHeight = 25;
        form.Add(componentName);
        form.Add(new Label("Generate following folders in component:"));
        form.Add(new Toggle(true, "Scripts"));
        form.Add(new Toggle(true, "Meshes"));
        form.Add(new Toggle(true, "Materials"));
        form.Add(new Toggle(true, "Prefabs"));
        form.Add(new Toggle(true, "Ressoures"));
    }

    void OnGUI()
    {
        Rect rect = new Rect(0, 0, Screen.width/2, 180);
        form.OnGUI(rect);
    }

}

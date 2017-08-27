using EMP.Forms;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class FormTest : MonoBehaviour {

    private Form form;

	void Start () {
        form = new Form();
        form.Spacing = 5;

        Label labelNamespace = new Label("Component Namespace:");
        labelNamespace.Width = 150;
        TextField txtNamespace = new TextField();
        Linear componentNamespace = Linear.Horizontal().Add(labelNamespace).Add(txtNamespace);
        componentNamespace.Height = 30;
        form.Add(componentNamespace);

        Label labelComponentName = new Label("Name of Component:");
        labelComponentName.Width = 150;
        TextField txtComponentName = new TextField();
        Linear componentName = Linear.Horizontal().Add(labelComponentName).Add(txtComponentName);
        componentName.Height = 30;
        form.Add(componentName);

        form.Add(new Label("Generate following folders in component:"));

        Linear toggles = Linear.Horizontal();
        Linear left = Linear.Vertical();
        left.Width = 100;
        left.Add(new Toggle(true, "Scripts"));
        left.Add(new Toggle(true, "Meshes"));
        left.Add(new Toggle(true, "Materials"));

        Linear right = Linear.Vertical();
        right.Width = 100;
        right.Add(new Toggle(true, "Prefabs"));
        right.Add(new Toggle(true, "Ressoures"));

        toggles.Add(left).Add(right);
        form.Add(toggles);

        Linear buttonContainer = Linear.Horizontal();

        form.Add(buttonContainer);

        Button button = new Button("Ok", _ => { });
        button.Width = 100;
        buttonContainer.Add(new View()).Add(button);
        form.Add(buttonContainer);
    }

    void OnGUI()
    {
        Rect rect = new Rect(0, 0, Screen.width/2, 200);
        form.OnGUI(rect);
    }

}

using EMP.Forms;
using System;
using System.Collections;
using System.Collections.Generic;
using UnityEditor;
using UnityEngine;

namespace EMP.Editor
{
    public class CreateCsFile : FormPopup
    {
        [MenuItem(MenuPaths.CREATE_CLASS, priority = -1000)]
        public static void OnClick()
        {
            new CreateCsFile().Show();
        }

        [MenuItem(MenuPaths.CREATE_CLASS, true)]
        public static bool Check()
        {
            return SelectionHelper.IsDirectorySelected();
        }

        protected override Vector2 GetFormSize()
        {
            return new Vector2(500, 100);
        }

        private TextField txtClassName;
        private TextField txtNamespace;
        private Toggle tglHeaderComment;
        private Toggle tglLogger;

        protected override void OnCreateForm(Form form)
        {
            Vector3 formSize = GetFormSize();
            form.Spacing = 5;

            form.Add(new Headline("Create C# Class"));

            Label lblNamespace = new Label("Namespace:");
            lblNamespace.Width = 150;
            txtNamespace = new TextField();
            Linear lyNamespace = Linear.Horizontal().Add(lblNamespace).Add(txtNamespace);
            lyNamespace.Height = 30;

            Label lblClassName = new Label("Class name:");
            lblClassName.Width = 150;
            txtClassName = new TextField();
            Linear lyClassName = Linear.Horizontal().Add(lblClassName).Add(txtClassName);
            lyClassName.Height = 30;

            form.Add(lyNamespace);
            form.Add(lyClassName);
            

            form.Add(tglHeaderComment = new Toggle(true, "Generate source code header comment"));
            form.Add(tglLogger = new Toggle(true, "Generate logger"));

            Linear lyButton = Linear.Horizontal();
            Button button = new Button("Ok", _ => { });
            button.Width = 100;
            lyButton.Add(new View()).Add(button);
            form.Add(lyButton);

            txtClassName.Focus();
        }  
    }

}

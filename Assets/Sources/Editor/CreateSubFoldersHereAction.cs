//
// MIT License
// Copyright (c) EMP - https://github.com/SchmobertRitz/EMP-Unity
//
using System.Collections.Generic;
using EMP.Forms;
using UnityEngine;
using UnityEditor;
using System.Text.RegularExpressions;
using System.IO;

namespace EMP.Editor
{
    public class CreateSubFoldersHereAction : FormPopup
    {
        private string path;
        
        [MenuItem(MenuPaths.CREATE_SUBFOLDERS_HERE, priority = -1000 + 2)]
        public static void OnClick()
        {
            string path = SelectionHelper.GetSelectedPath();
            if (path == null)
            {
                path = @"Assets/";
            }
            new CreateSubFoldersHereAction(path).Show();
        }

        [MenuItem(MenuPaths.CREATE_SUBFOLDERS_HERE, true)]
        public static bool Check()
        {
            return SelectionHelper.IsDirectorySelected() || SelectionHelper.GetSelectedPath() == null;
        }

        public CreateSubFoldersHereAction(string path)
        {
            this.path = path;
        }

        protected override Vector2 GetFormSize()
        {
            return new Vector2(300, 170);
        }

        private List<Toggle> subfolders = new List<Toggle>();

        protected override void OnCreateForm(Form form)
        {
            form.Spacing = 5;

            form.Add(new Headline("Create Subfolders"));

            LinearLayout lyButton = LinearLayout.Horizontal();
            Button button = new Button("Ok", ButtonClicked);
            button.Width = 100;
            lyButton.Add(new View()).Add(button);
            
            Label lblSubfolders = new Label("Subfolders:");
            lblSubfolders.Width = 150;

            GridLayout lyCheckboxes = new GridLayout(3, GridLayout.EOrientation.Vertical);
            
            foreach(string folder in new string[] { "Scripts", "Scenes", "Prefabs", "Resources", "Textures", "Materials", "Meshes" })
            {
                Toggle toggle = new Toggle(false, folder);
                subfolders.Add(toggle);
                lyCheckboxes.Add(toggle);
            }

            form.Add(lyCheckboxes);
            form.Add(new View());
            form.Add(lyButton);
        }

        private void ButtonClicked(Button obj)
        {
            foreach(Toggle toggle in subfolders)
            {
                if (toggle.Checked)
                {
                    Directory.CreateDirectory(Path.Combine(path, toggle.Text));
                }
            }

            AssetDatabase.Refresh();
            editorWindow.Close();
        }
    }
}

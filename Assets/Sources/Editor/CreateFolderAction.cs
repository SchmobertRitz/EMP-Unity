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
    public class CreateFolderAction : FormPopup
    {
        private string path;
        private SourcesInfo transitiveSourceInfo;
        
        [MenuItem(MenuPaths.CREATE_FOLDER, priority = -1000 + 1)]
        public static void OnClick()
        {
            SourcesInfo sources = new SourcesInfo();
            string path = SelectionHelper.GetSelectedPath();
            if (path == null)
            {
                path = @"Assets/";
            }
            SourcesInfo.FillInSoureData(path, sources);
            new CreateFolderAction(path, sources).Show();
        }

        [MenuItem(MenuPaths.CREATE_FOLDER, true)]
        public static bool Check()
        {
            return SelectionHelper.IsDirectorySelected() || SelectionHelper.GetSelectedPath() == null;
        }

        public CreateFolderAction(string path, SourcesInfo sourcesInfo)
        {
            this.path = path;
            this.transitiveSourceInfo = sourcesInfo;
        }

        protected override Vector2 GetFormSize()
        {
            return new Vector2(500, 250);
        }

        private TextField txtFolderName;
        private TextField txtNamespace;
        private Label lblError;
        private List<Toggle> subfolders = new List<Toggle>();

        protected override void OnCreateForm(Form form)
        {
            form.Spacing = 5;

            form.Add(new Headline("Create Folder"));

            Label lblNamespace = new Label("Namespace:");
            lblNamespace.Width = 150;
            txtNamespace = new TextField(transitiveSourceInfo.@namespace);
            LinearLayout lyNamespace = LinearLayout.Horizontal().Add(lblNamespace).Add(txtNamespace);
            lyNamespace.Height = 30;
            
            Label lblFolder = new Label("Folder:");
            lblFolder.Width = 150;
            txtFolderName = new TextField();
            LinearLayout lyFolder = LinearLayout.Horizontal().Add(lblFolder).Add(txtFolderName);
            lyFolder.Height = 30;
            lblError = new Label("");
            lblError.style.fontStyle = FontStyle.Bold;

            LinearLayout lyButton = LinearLayout.Horizontal();
            Button button = new Button("Ok", ButtonClicked);
            button.Width = 100;
            lyButton.Add(new View()).Add(button);
            
            Label lblSubfolders = new Label("Create subfolders:");
            lblSubfolders.Width = 150;

            GridLayout lyCheckboxes = new GridLayout(3, GridLayout.EOrientation.Vertical);
            
            foreach(string folder in new string[] { "Scripts", "Scenes", "Prefabs", "Resources", "Textures", "Materials", "Meshes" })
            {
                Toggle toggle = new Toggle(false, folder);
                subfolders.Add(toggle);
                lyCheckboxes.Add(toggle);
            }

            LinearLayout lySubfolders = LinearLayout.Horizontal().Add(lblSubfolders).Add(lyCheckboxes);

            form.Add(lyNamespace);
            form.Add(lyFolder);
            form.Add(lblError);
            form.Add(lySubfolders);
            form.Add(lyButton);

            form.RequestFocusForView = txtFolderName;
        }

        private void ButtonClicked(Button obj)
        {
            Regex patternFolderName = new Regex(@"^\s*[a-zA-Z0-9]+\s*$");

            if (!patternFolderName.IsMatch(txtFolderName.Text))
            {
                lblError.Text = "Please enter a valid folder name.";
                return;
            }
            
            Regex patternNamespace = new Regex(@"^\s*[a-zA-Z_][a-zA-Z0-9]*(\.[a-zA-Z_][a-zA-Z0-9]*)*\s*$");
            if (!patternNamespace.IsMatch(txtNamespace.Text))
            {
                lblError.Text = "Please enter a valid namespace.";
                return;
            }
            lblError.Text = "";

            string destPathName = Path.Combine(path, txtFolderName.Text.Trim());

            if (Directory.Exists(destPathName))
            {
                lblError.Text = "There already exists a folder with this name.";
                return;
            }
            
            Directory.CreateDirectory(destPathName);
            foreach(Toggle toggle in subfolders)
            {
                if (toggle.Checked)
                {
                    Directory.CreateDirectory(Path.Combine(destPathName, toggle.Text));
                }
            }

            if (!txtNamespace.Text.Trim().Equals(transitiveSourceInfo.@namespace)) {
                string sourceFileName = Path.Combine(destPathName, SourcesInfo.FILE_NAME);
                SourcesInfo sourceInfo = new SourcesInfo();
                sourceInfo.@namespace = txtNamespace.Text.Trim();
                sourceInfo.Save(sourceFileName);
            }
            
            AssetDatabase.Refresh();
            editorWindow.Close();
        }
    }
}

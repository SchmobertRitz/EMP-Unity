//
// MIT License
// Copyright (c) EMP - https://github.com/SchmobertRitz/EMP-Unity
//
using EMP.Editor;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using EMP.Forms;
using System;
using UnityEditor;
using System.IO;
using EMP.Cs;

namespace EMP.LivingAsset
{
    public class CreateLivingAssetDialog : FormPopup
    {
        private ILogger Logger = Debug.logger;

        [MenuItem("Living Asset/Create LivingAsset", validate = false)]
        public static void LivingAsset_CreateLivingAsset()
        {
            new CreateLivingAssetDialog().Show();
        }

        protected override Vector2 GetFormSize()
        {
            return new Vector2(500, 300);
        }

        private TextField txtName;
        private TextField txtDescr;
        private Toggle tglCompression;
        private Label lblMessage;

        protected override void OnCreateForm(Form form)
        {
            form.Add(new Headline("Create Living Asset") << 30);
            form.Add(new Label("Name:") + new TextField("", 300).Bind(out txtName) << 30);
            form.Add(new Label("Description:") + new TextField("", 300).Bind(out txtDescr) << 30);
            form.Add(new Label("Please enter the name of your new LivingAsset and a brief human-readable description.").Bind(out lblMessage) << 50 >> 400);
            form.Add(new View(), 0.125f);
            form.Add(new Toggle(true, "Use Compression").Bind(out tglCompression) << 30);
            form.Add(new View(), 1);
            form.Add(new View() + (new Button("Ok", OnButtonClick) >> 100) << 30);
        }

        private void OnButtonClick(Button button)
        {
            string path = AssetDatabase.GUIDToAssetPath(AssetDatabase.CreateFolder(SelectionHelper.GetSelectedPath(), txtName.Text));

            ManifestEditor manifestEditor = ScriptableObject.CreateInstance<ManifestEditor>();
            manifestEditor.Manifest = new Manifest();
            manifestEditor.Manifest.Name = txtName.Text;
            manifestEditor.Manifest.Description = txtDescr.Text;
            manifestEditor.UseCompression = tglCompression.Checked;

            AssetDatabase.CreateAsset(manifestEditor, Path.Combine(path, ManifestEditor.FILE_NAME));
            AssetDatabase.CreateFolder(path, "Scripts");
            AssetDatabase.CreateFolder(path, "Libs");
            AssetDatabase.CreateFolder(path, "Assets");

            SourcesInfo sourceInfo = new SourcesInfo();
            sourceInfo.@namespace = txtName.Text;
            sourceInfo.headerComment = "/* Thsi class is part of the LivingAsset " + txtName.Text + " */\n\n";
            sourceInfo.Save(Path.Combine(Path.Combine(path, "Scripts"), SourcesInfo.FILE_NAME));

            InitializerStubGenerator generator = new InitializerStubGenerator();
            string code = generator.Generate(new Dictionary<string, object> { { "NAMESPACE", txtName.Text } });
            File.WriteAllText(Path.Combine(path, "Scripts/Initializer.cs"), code);

            LivingAssetBuilder.CompileCsSources(path);

            Selection.activeObject = manifestEditor;

            AssetDatabase.SaveAssets();
            AssetDatabase.Refresh();

            Close();
        }
    }

    class InitializerStubGenerator : CsGenerator
    {
        protected override string GetTemplate()
        {
            return
@"using EMP.LivingAsset;
using UnityEngine;

namespace #NAMESPACE#
{
    public class Initializer : IInitializer
    {
        public void Initialize(Manifest manifest, AssetBundle[] assetBundles)
        {
            Debug.Log(""LivingAsset '"" + manifest.Name + ""' successfully initialized."");
        }
    }
}";
        }
    }
}

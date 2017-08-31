﻿//
// MIT License
// Copyright (c) EMP - https://github.com/SchmobertRitz/EMP-Unity
//
using EMP.Cs;
using EMP.Forms;
using System.Collections.Generic;
using System.IO;
using System.Text.RegularExpressions;
using System.Xml;
using System.Xml.Serialization;
using UnityEditor;
using UnityEngine;

namespace EMP.Editor
{
    public class CreateCsFileAction : FormPopup
    {
        [MenuItem(MenuPaths.CREATE_CLASS, priority = -1000)]
        public static void OnClick()
        {
            SourcesInfo sources = new SourcesInfo();
            SourcesInfo.FillInSoureData(SelectionHelper.GetSelectedPath(), sources);
            new CreateCsFileAction(SelectionHelper.GetSelectedPath(), sources).Show();
        }

        [MenuItem(MenuPaths.CREATE_CLASS, true)]
        public static bool Check()
        {
            return SelectionHelper.IsDirectorySelected();
        }
        
        protected override Vector2 GetFormSize()
        {
            return new Vector2(500, 250);
        }

        private TextField txtClassName;
        private TextField txtNamespace;
        private Toggle tglHeaderComment;
        private Toggle tglLogger;
        private Label lblMissingClassName;
        private string path;
        private SourcesInfo sourcesInfo;

        public CreateCsFileAction(string path, SourcesInfo sourcesInfo)
        {
            this.path = path;
            this.sourcesInfo = sourcesInfo;
        }

        protected override void OnCreateForm(Form form)
        {
            Vector3 formSize = GetFormSize();
            form.Spacing = 5;

            form.Add(new Headline("Create C# Class"));

            Label lblNamespace = new Label("Namespace:");
            lblNamespace.Width = 150;
            txtNamespace = new TextField(sourcesInfo.@namespace);
            Linear lyNamespace = Linear.Horizontal().Add(lblNamespace).Add(txtNamespace);
            lyNamespace.Height = 30;

            Label lblClassName = new Label("Class name:");
            lblClassName.Width = 150;
            txtClassName = new TextField();
            Linear lyClassName = Linear.Horizontal().Add(lblClassName).Add(txtClassName);
            lyClassName.Height = 30;

            form.Add(lyNamespace);
            form.Add(lyClassName);
            form.Add(lblMissingClassName = new Label(""));
            lblMissingClassName.style.fontStyle = FontStyle.Bold;

            form.Add(tglHeaderComment = new Toggle(true, "Generate source code header comment"));
            form.Add(tglLogger = new Toggle(true, "Generate logger"));

            Linear lyButton = Linear.Horizontal();
            Button button = new Button("Ok", ButtonClicked);
            button.Width = 100;
            lyButton.Add(new View()).Add(button);
            form.Add(lyButton);

            form.RequestFocusForView = txtClassName;
        }

        private void ButtonClicked(Button b)
        {
            Regex patternClassName = new Regex(@"^\s*[a-zA-Z_][a-zA-Z0-9]*\s*$");
            if (!patternClassName.IsMatch(txtClassName.Text))
            {
                lblMissingClassName.Text = "Please enter a valid C# class name.";
                return;
            }
            string destFile = Path.Combine(path, txtClassName.Text.Trim() + ".cs");
            if (File.Exists(destFile))
            {
                lblMissingClassName.Text = "File already exists.";
                return;
            }
            lblMissingClassName.Text = "";

            Dictionary<string, object> data = new Dictionary<string, object>
            {
                { "NAMESPACE", txtNamespace.Text.Trim() },
                { "HASNAMESPACE", !string.IsNullOrEmpty(txtNamespace.Text.Trim()) },
                { "GENLOGGER", tglLogger.Checked },
                { "GENHEADER", tglHeaderComment.Checked && sourcesInfo.headerComment != null},
                { "HEADER", sourcesInfo.headerComment},
                { "CLASS", txtClassName.Text.Trim() }
            };

            string generated = new CsClassTemplate().Generate(data);
            File.WriteAllText(destFile, generated);
            AssetDatabase.Refresh();
            editorWindow.Close();
        }
    }

    public class CsClassTemplate : CsGenerator
    {
        protected override string GetTemplate()
        {
            return
@"#if GENHEADER == True
#HEADER#
#endif
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
#if GENLOGGER == True
// Logger definition here
#endif

#if HASNAMESPACE == True
namespace #NAMESPACE#
{
    public class #CLASS#
    {

    }
}
#else
public class #CLASS#
{

}
#endif";
        }
    }

    [XmlRoot(ElementName = "SourcesInfo")]
    public class SourcesInfo
    {
        public const string FILE_NAME = ".SourcesInfo.xml";

        public static void FillInSoureData(string path, SourcesInfo result)
        {
            if (result.headerComment == null || result.@namespace == null)
            {
                string filename = Path.Combine(path, SourcesInfo.FILE_NAME);
                SourcesInfo sources = Deserialize(filename);
                if (sources != null)
                {
                    if (result.headerComment == null)
                    {
                        result.headerComment = sources.headerComment;
                    }
                    if (result.@namespace == null)
                    {
                        result.@namespace = sources.@namespace;
                    }
                }
                if (result.headerComment == null || result.@namespace == null)
                {
                    string lastSegment = Path.GetFileName(path);
                    if (!lastSegment.Equals(path))
                    {
                        FillInSoureData(path.Substring(0, path.Length - (lastSegment.Length + 1)), result);
                    }
                }
            }
        }

        private static SourcesInfo Deserialize(string filename)
        {
            if (!File.Exists(filename))
            {
                return null;
            }
            var serializer = new XmlSerializer(typeof(SourcesInfo));

            using (var stringReader = new StringReader(File.ReadAllText(filename)))
            using (var xmlTextReader = new XmlTextReader(stringReader))
            {
                return (SourcesInfo)serializer.Deserialize(xmlTextReader);
            }
        }

        [XmlElement(ElementName = "Namespace")]
        public string @namespace;

        [XmlElement(ElementName = "HeaderComment")]
        public string headerComment;
    }

}

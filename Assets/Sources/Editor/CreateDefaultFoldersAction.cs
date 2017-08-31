//
// MIT License
// Copyright (c) EMP - https://github.com/SchmobertRitz/EMP-Unity
//
using System;
using System.Collections;
using System.Collections.Generic;
using EMP.Forms;
using UnityEngine;
using UnityEditor;

namespace EMP.Editor
{
    public class CreateDefaultFoldersAction : FormPopup
    {
        private string path;
        private SourcesInfo sources;
        
        [MenuItem(MenuPaths.CREATE_DEFAULT_FOLDERS, priority = -1000)]
        public static void OnClick()
        {
            SourcesInfo sources = new SourcesInfo();
            SourcesInfo.FillInSoureData(SelectionHelper.GetSelectedPath(), sources);
            new CreateDefaultFoldersAction(SelectionHelper.GetSelectedPath(), sources).Show();
        }

        [MenuItem(MenuPaths.CREATE_DEFAULT_FOLDERS, true)]
        public static bool Check()
        {
            return SelectionHelper.IsDirectorySelected();
        }

        public CreateDefaultFoldersAction(string path, SourcesInfo sources)
        {
            this.path = path;
            this.sources = sources;
        }

        protected override Vector2 GetFormSize()
        {
            return new Vector2(300, 200);
        }

        protected override void OnCreateForm(Form form)
        {
            
        }
    }
}

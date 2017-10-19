//
// MIT License
// Copyright (c) EMP - https://github.com/SchmobertRitz/EMP-Unity
//
using EMP.Editor;
using System.Collections;
using System.Collections.Generic;
using UnityEditor;
using UnityEngine;
using EMP.Forms;
using System;
using System.IO;

namespace EMP.LivingAsset
{
    public class InitLivingAssetAction : MonoBehaviour
    {
        private ILogger Logger = Debug.logger;
        
        [MenuItem(MenuPaths.INIT_LIVINGASSET_HERE, priority = 2000)]
        public static void Action_InitLivingAsset()
        {
            string path = SelectionHelper.GetSelectedPath();

            ManifestEditor manifestEditor = ScriptableObject.CreateInstance<ManifestEditor>();
            AssetDatabase.CreateAsset(manifestEditor, Path.Combine(path, "Manifest.asset"));
            AssetDatabase.CreateFolder(path, "Scripts");
            AssetDatabase.CreateFolder(path, "Libs");
            AssetDatabase.CreateFolder(path, "Assets");
            AssetDatabase.Refresh();
        }

    }
}

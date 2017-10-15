//
// MIT License
// Copyright (c) EMP - https://github.com/SchmobertRitz/EMP-Unity
//
using System.Collections;
using System.Collections.Generic;
using System.IO;
using UnityEditor;
using UnityEngine;

namespace EMP.LivingAsset
{
    public class AssetBundler
    {
        private readonly string buildPath;
        private readonly Manifest manifest;
        private readonly string path;

        public AssetBundler(string path, string buildPath, Manifest manifest)
        {
            this.path = path;
            this.manifest = manifest;
            this.buildPath = buildPath;
        }

        public void GenerateBundle()
        {
            List<string> bundles = new List<string>();
            if (Directory.Exists(path))
            {
                if (Directory.GetFiles(path).Length != 0)
                {
                    bundles.Add("");
                }
                else
                {
                    bundles.AddRange(Directory.GetDirectories(path));
                }

                foreach (string bundle in bundles)
                {
                    List<string> assetFiles = FilesHelper.CollectFiles(Path.Combine(path, bundle),
                        file => !file.EndsWith(".meta") && !file.EndsWith(".dll") && !file.EndsWith(".cs"));

                    string destPath = Path.Combine(buildPath, bundle);
                    Directory.CreateDirectory(destPath);

                    AssetBundleBuild[] buildMap = new AssetBundleBuild[1];
                    buildMap[0].assetBundleName = "bundle.asset";
                    buildMap[0].assetNames = assetFiles.ToArray();

                    BuildPipeline.BuildAssetBundles(destPath, buildMap, BuildAssetBundleOptions.None, BuildTarget.StandaloneWindows);
                }
            }
        }
    }
}

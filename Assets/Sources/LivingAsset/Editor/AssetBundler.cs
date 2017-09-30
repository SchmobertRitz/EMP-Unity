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
            List<string> assetFiles = FilesHelper.CollectFiles(path, 
                file => !file.EndsWith(".meta") && !file.EndsWith(".dll") && !file.EndsWith(".cs"));

            Path.GetDirectoryName(manifest.AssetBundles[0].File);
            
            AssetBundleBuild[] buildMap = new AssetBundleBuild[1];
            buildMap[0].assetBundleName = Path.GetFileName(manifest.AssetBundles[0].File);
            buildMap[0].assetNames = assetFiles.ToArray();

            string assetBundlePath = Path.Combine(buildPath, Path.GetDirectoryName(manifest.AssetBundles[0].File));
            Directory.CreateDirectory(assetBundlePath);

            BuildPipeline.BuildAssetBundles(assetBundlePath, buildMap, BuildAssetBundleOptions.None, BuildTarget.StandaloneWindows);
        }
    }
}

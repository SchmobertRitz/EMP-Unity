//
// MIT License
// Copyright (c) EMP - https://github.com/SchmobertRitz/EMP-Unity
//
using EMP.LivingAsset;
using UnityEngine;

namespace EMP.Test2
{
    public class Test2Initializer : IInitializer
    {
        public void Initialize(Manifest manifest, UnityEngine.AssetBundle[] assetBundles)
        {
            Debug.Log(manifest.Description);
        }
    }
}

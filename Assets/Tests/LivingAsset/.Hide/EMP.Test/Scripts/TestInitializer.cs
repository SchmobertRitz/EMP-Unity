//
// MIT License
// Copyright (c) EMP - https://github.com/SchmobertRitz/EMP-Unity
//
using EMP.LivingAsset;
using UnityEngine;

namespace EMP.Test
{
    public class TestInitializer : IInitializer
    {
        public void Initialize(Manifest manifest, UnityEngine.AssetBundle[] assetBundles)
        {
            Debug.Log(manifest.Description);
            GameObject go = assetBundles[0].LoadAsset<GameObject>("RotatingCube.prefab");
            GameObject.Instantiate(go);
        }
    }
}

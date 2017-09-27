//
// MIT License
// Copyright (c) EMP - https://github.com/SchmobertRitz/EMP-Unity
//
using EMP.LivingAsset;
using UnityEngine;

namespace EMP.Test
{
    public class LivingAssetTest : MonoBehaviour
    {
        private void Start()
        {
            Loader loader = new Loader(@"LivingAssets\Test");
            loader.Load();
        }
    }
}

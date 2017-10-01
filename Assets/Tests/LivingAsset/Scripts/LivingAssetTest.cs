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
            LivingAssetLoader loader1 = new LivingAssetLoader(@"EMP.Test.LivingAsset");
            loader1.Load();
        }
    }
}

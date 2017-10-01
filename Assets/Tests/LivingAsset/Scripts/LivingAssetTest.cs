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
            ILivingAssetDatabase database = new LocalLivingAssetDatabase();
            LivingAssetLoader loader1 = new LivingAssetLoader(@"EMP.Test", database);
            loader1.Load();
        }
    }
}

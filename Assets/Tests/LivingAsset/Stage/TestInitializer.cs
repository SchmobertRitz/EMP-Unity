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
        public void Initialize(Manifest manifest)
        {
            Debug.Log(manifest.Description);
        }
    }
}

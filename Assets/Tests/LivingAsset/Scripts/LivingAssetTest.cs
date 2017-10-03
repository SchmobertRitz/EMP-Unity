//
// MIT License
// Copyright (c) EMP - https://github.com/SchmobertRitz/EMP-Unity
//
using EMP.LivingAsset;
using System.IO;
using UnityEngine;

namespace EMP.Test
{
    public class LivingAssetTest : MonoBehaviour
    {
        private void Start()
        {
            ILivingAssetDatabase database = new LocalLivingAssetDatabase();
            LivingAssetLoader loader1 = new LivingAssetLoader(@"EMP.Test", database, dep => { Debug.Log(dep); return true; }, File.ReadAllText("privatekey.xml"));
            loader1.Load();
        }
    }
}

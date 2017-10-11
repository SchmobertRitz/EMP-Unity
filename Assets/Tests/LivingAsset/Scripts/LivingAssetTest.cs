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
            LivingAssetLoader loader1 = new LivingAssetLoader(
                @"EMP.Test",
                database,
                LivingAssetLoader.ESignatureCheckPolicy.SkipVerfication,
                File.ReadAllText("publickey.xml"),
                dep => { Debug.Log("Dep.: " + dep); return true; }
            );
            loader1.Load();
            
            // EMP.Test2.Api.AddRotateComponent(Camera.main.gameObject);
            //Test2.Api.GetRotateComponent(Camera.main.gameObject).enabled = false;
        }
    }
}

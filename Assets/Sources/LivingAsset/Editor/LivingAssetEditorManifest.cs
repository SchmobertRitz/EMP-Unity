//
// MIT License
// Copyright (c) EMP - https://github.com/SchmobertRitz/EMP-Unity
//
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace EMP.LivingAsset
{
    public class LivingAssetEditorManifest : ScriptableObject
    {
        private ILogger Logger = Debug.logger;

        [SerializeField]
        public string Name;

    }
}

//
// MIT License
// Copyright (c) EMP - https://github.com/SchmobertRitz/EMP-Unity
//
using System;
using System.Collections.Generic;
using UnityEditor;
using UnityEngine;

namespace EMP.LivingAsset
{
    [CreateAssetMenu(fileName = "Manifest", menuName = "Create Manifest Editor")]
    public class ManifestEditor : ScriptableObject
    {
        public const string FILE_NAME = "LivingAsset.asset";

        private ILogger Logger = Debug.logger;

        [SerializeField]
        public Manifest Manifest;

        [SerializeField]
        public bool UseCompression = true;

        internal static Manifest CreateFromPath(string manifestPath)
        {
            return AssetDatabase.LoadAssetAtPath<ManifestEditor>(manifestPath).Manifest;
        }
    }
}

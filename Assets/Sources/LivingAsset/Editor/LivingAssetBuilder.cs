//
// MIT License
// Copyright (c) EMP - https://github.com/SchmobertRitz/EMP-Unity
//
using UnityEngine;
using UnityEditor;

namespace EMP.LivingAsset
{
    public class LivingAssetBuilder : MonoBehaviour
    {

        [MenuItem("Living Asset/Build", validate = false)]
        public static void BuildSmartObject()
        {
            if (Selection.activeObject != null && Selection.activeObject.GetType().IsAssignableFrom(typeof(DefaultAsset)))
            {
                string path = AssetDatabase.GetAssetPath(Selection.activeObject.GetInstanceID());
                if (LivingAssetCompiler.IsFilestructureCorrect(path))
                {
                    LivingAssetCompiler compiler = new LivingAssetCompiler(path);
                    compiler.Compile();
                }
            }
        }


        [MenuItem("Living Asset/Build", validate = true)]
        public static bool IsCurrentSelectionAValidPath()
        {
            if (Selection.activeObject != null && Selection.activeObject.GetType().IsAssignableFrom(typeof(DefaultAsset)))
            {
                string path = AssetDatabase.GetAssetPath(Selection.activeObject.GetInstanceID());
                return LivingAssetCompiler.IsFilestructureCorrect(path);
            }
            return false;
        }
    }
}
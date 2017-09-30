//
// MIT License
// Copyright (c) EMP - https://github.com/SchmobertRitz/EMP-Unity
//
using UnityEngine;
using UnityEditor;
using System.IO;
using EMP.Editor;

namespace EMP.LivingAsset
{
    public class LivingAssetBuilder : MonoBehaviour
    {

        [MenuItem("Living Asset/Compile C# Sources", validate = false)]
        public static void LivingAsset_CompileCsSources()
        {
            if (SelectionHelper.IsDirectorySelected())
            {
                string path = SelectionHelper.GetSelectedPath();
                if (DllCompiler.IsFileStructureCorrect(path))
                {
                    string manifestPath = string.Format(@"{0}/{1}", path, Manifest.FILE_NAME);
                    string buildPath = path;

                    Manifest manifest = Manifest.CreateFromPath(manifestPath);
                    DllCompiler dllCompiler = new DllCompiler(path, buildPath, manifest);
                    dllCompiler.Compile();
                    AssetDatabase.Refresh();
                    Debug.Log("*** Finished Compiling C# Sources ***");
                }
            }
        }

        [MenuItem("Living Asset/Build Living Asset", validate = false)]
        public static void LivingAsset_BuildLivingAsset()
        {
            if (Selection.activeObject != null && Selection.activeObject.GetType().IsAssignableFrom(typeof(DefaultAsset)))
            {
                string path = AssetDatabase.GetAssetPath(Selection.activeObject.GetInstanceID());
                if (DllCompiler.IsFileStructureCorrect(path))
                {
                    string manifestPath = string.Format(@"{0}/{1}", path, Manifest.FILE_NAME);
                    string buildPath = FilesHelper.CreateBuildPath(path);

                    Manifest manifest = Manifest.CreateFromPath(manifestPath);
                    File.Copy(manifestPath, string.Format(@"{0}/{1}", buildPath, Manifest.FILE_NAME));
                   
                    DllCompiler dllCompuler = new DllCompiler(path, buildPath, manifest);
                    AssetBundler assetBundler = new AssetBundler(path, buildPath, manifest);
                    Archiver archiver = new Archiver(path, buildPath, manifest, true);

                    dllCompuler.Compile();
                    assetBundler.GenerateBundle();
                    archiver.GenerateArchive();

                    AssetDatabase.Refresh();

                    Debug.Log("*** Finished Building Living Asset ***");
                }
            }
        }

        [MenuItem("Living Asset/Compile C# Sources", validate = true)]
        [MenuItem("Living Asset/Build Living Asset", validate = true)]
        public static bool IsCurrentSelectionAValidPath()
        {
            if (Selection.activeObject != null && Selection.activeObject.GetType().IsAssignableFrom(typeof(DefaultAsset)))
            {
                string path = AssetDatabase.GetAssetPath(Selection.activeObject.GetInstanceID());
                return DllCompiler.IsFileStructureCorrect(path);
            }
            return false;
        }
    }
}
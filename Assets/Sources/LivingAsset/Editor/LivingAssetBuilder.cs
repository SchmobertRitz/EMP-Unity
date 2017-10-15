//
// MIT License
// Copyright (c) EMP - https://github.com/SchmobertRitz/EMP-Unity
//
using UnityEngine;
using UnityEditor;
using System.IO;
using EMP.Editor;
using System.Security;
using System.Security.Cryptography;

namespace EMP.LivingAsset
{
    public class LivingAssetBuilder : MonoBehaviour
    {
        private static SecureString privateKeyPassword;
        private static string privateKeyPath = "privatekey.xml";

        [MenuItem("Living Asset/Create Public And Private Keys", validate = false)]
        public static void LivingAsset_CreatePublicAndPrivateKeys()
        {
            using(RSACryptoServiceProvider RSA = new RSACryptoServiceProvider())
            {
                File.WriteAllText("publickey.xml", RSA.ToXmlString(false));
                File.WriteAllText("privatekey.xml", RSA.ToXmlString(true));
                Debug.Log("*** Public and private key successfully generated ***");
            }
        }

        [MenuItem("Assets/Living Asset/Compile C# Sources", validate = false)]
        public static void LivingAsset_CompileCsSources()
        {
            if (SelectionHelper.IsDirectorySelected())
            {
                string path = SelectionHelper.GetSelectedPath();
                if (DllCompiler.IsFileStructureCorrect(path))
                {
                    string manifestPath = string.Format(@"{0}/{1}", path, Manifest.FILE_NAME);
                    string buildPath = string.Format(@"{0}/{1}", path, DllCompiler.LIBRARY_PATH);

                    Manifest manifest = Manifest.CreateFromPath(manifestPath);
                    DllCompiler dllCompiler = new DllCompiler(path, buildPath, manifest);
                    dllCompiler.Compile();
                    AssetDatabase.Refresh();
                    Debug.Log("*** Finished Compiling C# Sources ***");
                }
            }
        }
        public const string ASSETS_PATH = "Assets";
        public const string API_PATH = "Api";

        [MenuItem("Assets/Living Asset/Build Living Asset", validate = false)]
        public static void LivingAsset_BuildLivingAsset()
        {
            if (Selection.activeObject != null && Selection.activeObject.GetType().IsAssignableFrom(typeof(DefaultAsset)))
            {
                string sourcePath = AssetDatabase.GetAssetPath(Selection.activeObject.GetInstanceID());
                if (DllCompiler.IsFileStructureCorrect(sourcePath))
                {
                    string manifestPath = string.Format(@"{0}/{1}", sourcePath, Manifest.FILE_NAME);
                    string buildPath = FilesHelper.CreateBuildPath(sourcePath);

                    string libsBuildPath = Path.Combine(buildPath, DllCompiler.LIBRARY_PATH);
                    string libsSourcePath = Path.Combine(sourcePath, DllCompiler.LIBRARY_PATH);

                    string apiBuildPath = Path.Combine(buildPath, API_PATH);

                    string assetBundleBuildPath = Path.Combine(buildPath, ASSETS_PATH);
                    string assetBundleSourcePath = Path.Combine(sourcePath, ASSETS_PATH);

                    Directory.CreateDirectory(libsBuildPath);
                    Directory.CreateDirectory(assetBundleBuildPath);

                    Manifest manifest = Manifest.CreateFromPath(manifestPath);

                    // Copy Manifest
                    File.Copy(manifestPath, string.Format(@"{0}/{1}", buildPath, Manifest.FILE_NAME));

                    // Generate Sources and Libs 
                    DllCompiler dllCompiler = new DllCompiler(sourcePath, libsSourcePath, manifest);
                    ApiGenerator apiGenerator = new ApiGenerator(dllCompiler.GetOutputFilePath(), apiBuildPath, manifest);
                    FilesHelper.CollectFiles(libsSourcePath, file => file.EndsWith(".dll")).ForEach(file => File.Copy(file, Path.Combine(libsBuildPath, Path.GetFileName(file))));

                    // Generate Living Asset
                    AssetBundler assetBundler = new AssetBundler(assetBundleSourcePath, assetBundleBuildPath, manifest);

                    Archiver archiver = new Archiver(
                        libsBuildPath,
                        apiBuildPath,
                        assetBundleBuildPath,
                        buildPath,
                        manifest,
                        false,
                        File.Exists(privateKeyPath) ? File.ReadAllText(privateKeyPath) : null
                    );

                    dllCompiler.Compile();
                    AssetDatabase.Refresh();
                    apiGenerator.GenerateApi();
                    assetBundler.GenerateBundle();
                    archiver.GenerateArchive();

                    AssetDatabase.Refresh();

                    Debug.Log("*** Finished Building Living Asset ***");
                }
            }
        }

        [MenuItem("Assets/Living Asset/Compile C# Sources", validate = true)]
        [MenuItem("Assets/Living Asset/Build Living Asset", validate = true)]
        public static bool IsCurrentSelectionAValidPath()
        {
            if (!Application.isPlaying && Selection.activeObject != null && Selection.activeObject.GetType().IsAssignableFrom(typeof(DefaultAsset)))
            {
                string path = AssetDatabase.GetAssetPath(Selection.activeObject.GetInstanceID());
                return DllCompiler.IsFileStructureCorrect(path);
            }
            return false;
        }
    }
}
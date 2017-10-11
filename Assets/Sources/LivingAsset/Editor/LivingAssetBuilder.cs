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
                    string buildPath = path;

                    Manifest manifest = Manifest.CreateFromPath(manifestPath);
                    DllCompiler dllCompiler = new DllCompiler(path, buildPath, manifest.Libraries[0].File, manifest);
                    dllCompiler.Compile();
                    AssetDatabase.Refresh();
                    Debug.Log("*** Finished Compiling C# Sources ***");
                }
            }
        }

        [MenuItem("Assets/Living Asset/Build Living Asset", validate = false)]
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
                    string apiPath = Path.Combine(buildPath, "Api");
                    string libName = manifest.Libraries[0].File;

                    DllCompiler dllCompiler = new DllCompiler(path, buildPath, libName, manifest);
                    ApiGenerator apiGenerator = new ApiGenerator(dllCompiler.GetOutputFilePath(), apiPath, manifest);
                    DllCompiler apiCompiler = new DllCompiler(apiPath, buildPath, "api." + libName, manifest);
                    AssetBundler assetBundler = new AssetBundler(path, buildPath, manifest);
                    Archiver archiver = new Archiver(
                        path,
                        buildPath,
                        manifest,
                        false,
                        File.Exists(privateKeyPath) ? File.ReadAllText(privateKeyPath) : null
                    );

                    dllCompiler.Compile();
                    apiGenerator.GenerateApi();
                    apiCompiler.Compile();
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
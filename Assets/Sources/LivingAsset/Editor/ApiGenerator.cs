//
// MIT License
// Copyright (c) EMP - https://github.com/SchmobertRitz/EMP-Unity
//
using EMP.Cs;
using System;
using System.Collections.Generic;
using System.IO;
using System.Reflection;
using UnityEngine;

namespace EMP.LivingAsset
{
    public class ApiGenerator
    {
        private readonly string filepath;
        private readonly Manifest manifest;
        private readonly string buildPath;

        public static bool IsFileStructureCorrect(string path)
        {
            return Directory.Exists(path)
                    && ((File.GetAttributes(path) & FileAttributes.Directory) == FileAttributes.Directory)
                    && File.Exists(Path.Combine(path, Manifest.FILE_NAME));
        }

        public ApiGenerator(string filepath, string buildPath, Manifest manifest)
        {
            this.filepath = filepath;
            this.manifest = manifest;
            this.buildPath = buildPath;
        }

        public void GenerateApi()
        {
            if (File.Exists(filepath))
            {
                Assembly assembly = Assembly.Load(File.ReadAllBytes(filepath));
                List<Type> unityScripts = new List<Type>(Array.FindAll(assembly.GetTypes(), type => type.IsSubclassOf(typeof(MonoBehaviour))));

                Dictionary<string, object> data = new Dictionary<string, object> {
                { "NAMESPACE", manifest.Name },
                { "LIVNGASSETNAME", manifest.Name },
                { "DATA",
                  unityScripts.ConvertAll(type => {
                        return new Dictionary<string, object> {
                            { "COMPONENTNAME", DeriveComponentName(type) },
                            { "LIVNGASSETNAME", manifest.Name },
                            { "TYPENAME", type.FullName }
                        };
                    })
                }
            };

                CsGenerator generator = new UnityScriptsGenerator();
                string generatedCode = generator.Generate(data);
                Directory.CreateDirectory(buildPath);
                File.WriteAllText(Path.Combine(buildPath, manifest.Name + ".api.cs"), generatedCode);
            } else
            {
                Directory.CreateDirectory(buildPath);
                File.WriteAllText(Path.Combine(buildPath, manifest.Name + ".api.cs"), "// No API generated");
            }
        }

        private object DeriveComponentName(Type type)
        {
            string result = type.FullName;
            if (result.StartsWith(manifest.Name))
            {
                result = type.FullName.Substring(manifest.Name.Length);
            }
            return result.Replace(".", "");
        }

        private class UnityScriptsGenerator : CsGenerator
        {
            protected override string GetTemplate()
            {
                return
                @" // This a a generated file. Do not modify.

using UnityEngine;
using System;

namespace #NAMESPACE#
{
    public class Api
    {
        private const string LIVING_ASSET_NAME = ""#LIVNGASSETNAME#"";

        private static Type FindType(string name) {
            EMP.LivingAsset.LivingAsset livingAsset = EMP.LivingAsset.LivingAsset.GetRegistry().Get(LIVING_ASSET_NAME);
            Type type = livingAsset.FindType(name);
            if (type == null) {
                throw new Exception(""Type '"" + name + ""' is not available in LivingAsset '"" + LIVING_ASSET_NAME + ""'."");
            }
            return type;
        }

        #block BLOCK_METHOD_ADD_COMPONENT
        public static void Add#COMPONENTNAME#Component(GameObject gameObject) {
            Type type = FindType(""#TYPENAME#"");
            gameObject.AddComponent(type);
        }

        public static MonoBehaviour Get#COMPONENTNAME#Component(GameObject gameObject) {
            Type type = FindType(""#TYPENAME#"");
            return gameObject.GetComponent(type) as MonoBehaviour;
        }

        #endblock
        #foreach in DATA with BLOCK_METHOD_ADD_COMPONENT
    }
}
";
            }
        }
    }
}

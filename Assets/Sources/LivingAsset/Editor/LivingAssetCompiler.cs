//
// MIT License
// Copyright (c) EMP - https://github.com/SchmobertRitz/EMP-Unity
//
using System.Collections.Generic;
using System.CodeDom.Compiler;
using System.Reflection;
using System;
using System.IO;
using UnityEngine;
using UnityEditor;
using Microsoft.CSharp;

namespace EMP.LivingAsset
{
    public class LivingAssetCompiler
    {

        private readonly List<string> sourceFiles = new List<string>();
        private readonly string path;
        private readonly Manifest manifest;

        public static bool IsFilestructureCorrect(string path)
        {
            return Directory.Exists(path)
                    && ((File.GetAttributes(path) & FileAttributes.Directory) == FileAttributes.Directory)
                    && File.Exists(Path.Combine(path, Manifest.FILE_NAME));
        }

        public LivingAssetCompiler(string path)
        {
            if (!IsFilestructureCorrect(path))
            {
                throw new ArgumentException();
            }
            this.path = path;
            this.manifest = Manifest.CreateFromPath(string.Format(@"{0}/{1}", path, Manifest.FILE_NAME));
        }

        private void CollectSourceFiles(string path)
        {
            foreach (string file in Directory.GetFiles(path))
            {
                if (file.EndsWith(".cs"))
                {
                    sourceFiles.Add(file);
                }
            }
            foreach (string subDir in Directory.GetDirectories(path))
            {
                CollectSourceFiles(subDir);
            }
        }

        public void Compile()
        {
            CollectSourceFiles(path);

            string libName = manifest.Libraries[0].File;
            CompilerParameters compilerParameters = new CompilerParameters();
            compilerParameters.GenerateExecutable = false;
            compilerParameters.OutputAssembly = libName;
            compilerParameters.IncludeDebugInformation = false;

            foreach (Assembly assembly in AppDomain.CurrentDomain.GetAssemblies())
            {
                compilerParameters.ReferencedAssemblies.Add(assembly.Location);
            }
            Dictionary<string, string> compilerOptions = new Dictionary<string, string>();
            compilerOptions.Add("CompilerVersion", "v4.0");

            CodeDomProvider codeDomProvider = new CSharpCodeProvider(compilerOptions);

            CompilerResults results = codeDomProvider.CompileAssemblyFromFile(compilerParameters, sourceFiles.ToArray());

            if (results.Errors.Count != 0)
            {
                foreach (string err in results.Output)
                {
                    Debug.Log(err);
                }
            }
            else
            {
                File.Move(results.PathToAssembly, Path.Combine(path, libName));
                AssetDatabase.Refresh();
            }
        }
    }
}
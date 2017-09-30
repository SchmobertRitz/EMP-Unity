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
    public class DllCompiler
    {
        private readonly string path;
        private readonly Manifest manifest;
        private readonly string buildPath;

        public static bool IsFileStructureCorrect(string path)
        {
            return Directory.Exists(path)
                    && ((File.GetAttributes(path) & FileAttributes.Directory) == FileAttributes.Directory)
                    && File.Exists(Path.Combine(path, Manifest.FILE_NAME));
        }

        public DllCompiler(string path, string buildPath, Manifest manifest)
        {
            if (!IsFileStructureCorrect(path))
            {
                throw new ArgumentException();
            }
            this.path = path;
            this.manifest = manifest;
            this.buildPath = buildPath;
        }
        
        public void Compile()
        {
            List<string> sourceFiles = FilesHelper.CollectFiles(path, file => !file.StartsWith("-") && file.EndsWith(".cs"));

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

            if (results.Errors.HasErrors)
            {
                foreach (string err in results.Output)
                {
                    Debug.Log(err);
                }
            }
            else
            {
                File.Move(results.PathToAssembly, Path.Combine(buildPath, libName));
            }
        }
    }
}
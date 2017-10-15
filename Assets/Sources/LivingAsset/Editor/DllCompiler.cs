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
using Microsoft.CSharp;

namespace EMP.LivingAsset
{
    public class DllCompiler
    {
        private readonly string path;
        private readonly Manifest manifest;
        private readonly string buildPath;

        public const string LIBRARY_PATH = "Libs";

        public static bool IsFileStructureCorrect(string path)
        {
            return Directory.Exists(path)
                    && ((File.GetAttributes(path) & FileAttributes.Directory) == FileAttributes.Directory)
                    && File.Exists(Path.Combine(path, Manifest.FILE_NAME));
        }

        public DllCompiler(string path, string buildPath, Manifest manifest)
        {
            this.path = path;
            this.manifest = manifest;
            this.buildPath = buildPath;
        }

        public void Compile()
        {
            CompilerParameters compilerParameters = new CompilerParameters();
            compilerParameters.GenerateExecutable = false;
            compilerParameters.OutputAssembly = GetLibName();
            compilerParameters.IncludeDebugInformation = false;

            foreach (Assembly assembly in AppDomain.CurrentDomain.GetAssemblies())
            {
                compilerParameters.ReferencedAssemblies.Add(assembly.Location);
            }
            Dictionary<string, string> compilerOptions = new Dictionary<string, string>();
            compilerOptions.Add("CompilerVersion", "v4.0");

            CodeDomProvider codeDomProvider = new CSharpCodeProvider(compilerOptions);

            CompilerResults results = CompileFromSourceFiles(compilerParameters, codeDomProvider);
            
            if (results.Errors.HasErrors)
            {
                foreach (string err in results.Output)
                {
                    Debug.Log(err);
                }
            }
            else
            {
                if (File.Exists(GetOutputFilePath()))
                {
                    File.Delete(GetOutputFilePath());
                }
                Directory.CreateDirectory(buildPath);
                File.Move(results.PathToAssembly, GetOutputFilePath());
            }
        }

        private CompilerResults CompileFromSourceFiles(CompilerParameters compilerParameters, CodeDomProvider codeDomProvider)
        {
            List<string> sourceFiles = FilesHelper.CollectFiles(path, file => !file.StartsWith("-") && file.EndsWith(".cs"));
            return codeDomProvider.CompileAssemblyFromFile(compilerParameters, sourceFiles.ToArray());
        }

        public string GetOutputFilePath()
        {
            return Path.Combine(buildPath, GetLibName());
        }

        private string GetLibName()
        {
            return manifest.Name + ".dll";
        }
    }
}
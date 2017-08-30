//
// MIT License
// Copyright (c) EMP - https://github.com/SchmobertRitz/EMP-Unity
//
using EMP.Editor;
using Microsoft.CSharp;
using System;
using System.CodeDom.Compiler;
using System.Collections.Generic;
using System.IO;
using System.Reflection;
using UnityEditor;
using UnityEngine;

public class CompileLib {
    private List<string> sourceFiles = new List<string>();
    private string path;

    [MenuItem("Assets/Create/DLL from all C# files in folder")]
    public static void OnCompileAll()
    {
        new CompileLib(SelectionHelper.GetSelectedPath()).Compile();
    }

    [MenuItem("Assets/Create/DLL from all C# files in folder", true)]
    public static bool CheckCompileAll()
    {
        return SelectionHelper.IsDirectorySelected();
    }
    /*
    [MenuItem("Assets/Create/DLL from C# files in folder...")]
    public static void OnCompileSelected()
    {
        
    }

    [MenuItem("Assets/Create/DLL from C# files in folder...", true)]
    public static bool CheckCompileSelected()
    {
        return SelectionHelper.IsDirectorySelected();
    }
    */
    public CompileLib(string path)
    {
        this.path = path;
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

        string libName = path.Replace("/", ".").Replace(@"\", ".") + ".dll";
        string tmpLibName = Path.GetFileName(path) + "-" + Guid.NewGuid() + ".dll";

        CompilerParameters compilerParameters = new CompilerParameters();
        compilerParameters.GenerateExecutable = false;
        compilerParameters.OutputAssembly = tmpLibName;
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
            Debug.LogError("Errors occured while compiling:");
            foreach (string err in results.Output)
            {
                Debug.LogError(err);
            }
        }

        File.Move(tmpLibName, Path.Combine(path, libName));
        AssetDatabase.Refresh();
    }
}

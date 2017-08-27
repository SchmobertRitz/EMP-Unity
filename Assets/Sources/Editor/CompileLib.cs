using EMP.Editor;
using System.Collections;
using System.Collections.Generic;
using UnityEditor;
using UnityEngine;

public class CompileLib : MonoBehaviour {
    private List<string> sourceFiles = new List<string>();
    private string smartObjectRoot;

    [MenuItem("Compile C#s")]
    public static void OnClick()
    {
    
    }

    [MenuItem("Compile C#s", true)]
    public static bool Check()
    {
        return SelectionHelper.IsDirectorySelected();
    }

    public CompileLib(string smartObjectRoot)
    {
        if (!IsFilestructureCorrect(smartObjectRoot))
        {
            throw new ArgumentException();
        }
        this.smartObjectRoot = smartObjectRoot;
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
        CollectSourceFiles(smartObjectRoot);

        string libName = Path.GetFileName(smartObjectRoot) + ".dll";
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
            File.Move(results.PathToAssembly, Path.Combine(smartObjectRoot, libName));
            AssetDatabase.Refresh();
        }
    }
}

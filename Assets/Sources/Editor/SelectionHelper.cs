//
// MIT License
// Copyright (c) EMP - https://github.com/SchmobertRitz/EMP-Unity
//
using System.IO;
using UnityEditor;
using UnityEngine;

namespace EMP.Editor
{
    public static class SelectionHelper
    {

        public static bool IsDirectorySelected()
        {
            string path = GetSelectedPath();
            return path != null && ((File.GetAttributes(path) & FileAttributes.Directory) == FileAttributes.Directory);
        }

        public static bool IsFileSelected(string endsWith = null)
        {
            string path = GetSelectedPath();
            bool isFile = path != null && ((File.GetAttributes(path) & FileAttributes.Directory) != FileAttributes.Directory);
            if (isFile && endsWith != null && path.EndsWith(endsWith))
            {
                return true;
            }
            return isFile;
        }

        public static string GetSelectedPath()
        {
            Object obj = Selection.activeObject;
            if (obj == null)
            {
                return null;
            }
            return AssetDatabase.GetAssetPath(obj.GetInstanceID());
        }
    }

}

//
// MIT License
// Copyright (c) EMP - https://github.com/SchmobertRitz/EMP-Unity
//
using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using UnityEngine;

namespace EMP.LivingAsset
{
    public static class FilesHelper
    {
        public static string CreateBuildPath(string path)
        {
            string buildPath = string.Format(@"{0}/{1}", path, ".Build");
            int c = 1;
            while(Directory.Exists(buildPath))
            {
                buildPath = string.Format(@"{0}/{1} {2}", path, ".Build", c++);
            }
            Directory.CreateDirectory(buildPath);
            return buildPath;
        }

        public static List<string> CollectFiles(string path, Func<string, bool> matcher)
        {
            List<String> result = new List<string>();
            if (Directory.Exists(path))
            {
                CollectFiles(path, matcher, result);
            }
            return result;
        }

        private static void CollectFiles(string path, Func<string, bool> matcher, List<string> result)
        {
            foreach (string file in Directory.GetFiles(path))
            {
                if (!Path.GetFileName(path).StartsWith(".") && matcher(file))
                {
                    result.Add(file);
                }
            }
            foreach (string subDir in Directory.GetDirectories(path))
            {
                if (!Path.GetFileName(subDir).StartsWith("."))
                {
                    CollectFiles(subDir, matcher, result);
                }
            }
        }
    }
}

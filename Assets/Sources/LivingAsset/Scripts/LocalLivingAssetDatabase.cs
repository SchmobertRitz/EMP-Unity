//
// MIT License
// Copyright (c) EMP - https://github.com/SchmobertRitz/EMP-Unity
//
using System;
using System.Collections.Generic;
using System.IO;

namespace EMP.LivingAsset
{
    public class LocalLivingAssetDatabase : ILivingAssetDatabase
    {
        private readonly List<string> searchPaths = new List<string>();

        private readonly ILivingAssetDatabase fallbackDatabase;

        public LocalLivingAssetDatabase(List<string> searchPaths = null, ILivingAssetDatabase fallbackDatabase = null)
        {
            this.fallbackDatabase = fallbackDatabase;

            if (searchPaths == null || searchPaths.Count == 0)
            {
                this.searchPaths.Add(".");
            } else
            {
                this.searchPaths.AddRange(searchPaths);
            }
        }

        public void Lookup(string name, Action<Stream> inputStreamHandler)
        {
            foreach(string path in searchPaths)
            {
                string filename = Path.Combine(path, name + "." + LivingAssetLoader.LIVING_ASSET_FILE_EXTENSION);
                if (File.Exists(filename))
                {
                    using (Stream fileStream = File.OpenRead(filename))
                    {
                        inputStreamHandler(fileStream);
                    }
                    return;
                }
            }
            if (fallbackDatabase != null)
            {
                fallbackDatabase.Lookup(name, inputStreamHandler);
            } else
            {
                inputStreamHandler(null);
            }
        }
    }
}

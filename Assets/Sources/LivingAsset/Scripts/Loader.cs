//
// MIT License
// Copyright (c) EMP - https://github.com/SchmobertRitz/EMP-Unity
//
using System;
using System.IO;
using System.Reflection;
using UnityEngine;

namespace EMP.LivingAsset
{
    public class Loader
    {
        //private List<ILivingAssetFactory> factories = new List<ILivingAssetFactory>();
        private readonly string path;
        private bool loaded;

        public Loader(string path)
        {
            if (!Directory.Exists(path))
            {
                throw new ArgumentException("Unable to load smart object. Directory does not exist: " + path);
            }

            this.path = path;
        }

        public bool Load()
        {
            if (loaded)
            {
                throw new Exception("Already loaded"); // TODO: Better exception
            }
            loaded = true;
            try
            {
                string manifestPath = Path.Combine(path, "Manifest.xml");
                Manifest manifest = Manifest.CreateFromPath(manifestPath);

                foreach (Library lib in manifest.Libraries)
                {
                    LoadLib(path, manifest, lib);
                }
                return true;
            } catch(Exception e)
            {
                Debug.LogError(e);
                return false;
            }
        }

        /*
        public List<ILivingAssetFactory> GetFactories()
        {
            return new List<ILivingAssetFactory>(factories);
        }
        */

        private void LoadLib(string path, Manifest manifest, Library lib)
        {
            LivingAssetPolicy policy = new LivingAssetPolicy(manifest);

            if (string.IsNullOrEmpty(lib.File))
            {
                return;
            }
            string filename = Path.Combine(path, lib.File);
            if (!File.Exists(filename))
            {
                throw new Exception("Unable to load library. File not found: " + filename);
            }
            Assembly dll = Assembly.LoadFile(filename);
            if (!string.IsNullOrEmpty(lib.Initializer))
            {
                if (!policy.IsNamespaceValid(lib.Initializer))
                {
                    throw new Exception("Unable to execute initializer for library " + manifest.Name + ". The initializer class is not in the correct namespace.");
                }
                IInitializer initializer = (IInitializer) dll.CreateInstance(lib.Initializer);
                initializer.Initialize(manifest); // TODO: Defensive copying
            }
            /*foreach(FactoryClass fc in lib.SmartObjectFactoryClasses)
            {
                if (!string.IsNullOrEmpty(fc.Name))
                {
                    if (!fc.Name.StartsWith(manifest.Name))
                    {
                        throw new Exception("Unable to execute factory class " + fc.Name + ". The class is not in the correct namespace.");
                    }
                    ISmartObjectFactory factory = (ISmartObjectFactory) dll.CreateInstance(fc.Name);
                    factories.Add(factory);
                }
            }*/
        }
    }
}
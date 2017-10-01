//
// MIT License
// Copyright (c) EMP - https://github.com/SchmobertRitz/EMP-Unity
//
using System;
using System.Collections.Generic;
using System.Reflection;

namespace EMP.LivingAsset
{
    public class LivingAsset
    {
        private readonly static object LOCK = new object();
        private static Registry registryInstance;

        public static Registry GetRegistry()
        {
            lock(LOCK)
            {
                if (registryInstance == null)
                {
                    registryInstance = new Registry();
                }
                return registryInstance;
            }
        }

        public class Registry
        {
            internal Registry() { }

            private readonly Dictionary<string, LivingAsset> registeredAssets = new Dictionary<string, LivingAsset>();

            internal void Register(LivingAsset livingAsset)
            {
                if (IsRegistered(livingAsset.manifest.Name))
                {
                    throw new Exception("LivingAsset with the name " + livingAsset.manifest.Name + " is already registered.");
                }
                registeredAssets.Add(livingAsset.manifest.Name, livingAsset);
            }

            public bool IsRegistered(string name)
            {
                return registeredAssets.ContainsKey(name);
            }

            public bool IsRegisteredAndLoaded(string name)
            {
                LivingAsset livingAsset;
                return registeredAssets.TryGetValue(name, out livingAsset) && livingAsset.IsLoaded();
            }
        }

        private bool IsLoaded()
        {
            return loaded;
        }

        private readonly Manifest manifest;
        private byte[][] assembliesBytes;
        private byte[][] assetBundleBytes;
        
        private Assembly[] assemblies;
        private UnityEngine.AssetBundle[] assetBundles;

        private bool loaded;
        
        internal LivingAsset(Manifest manifest, byte[][] assembliesBytes, byte[][] assetBundleBytes)
        {
            this.manifest = manifest;
            this.assembliesBytes = assembliesBytes;
            this.assetBundleBytes = assetBundleBytes;
        }

        internal bool Load()
        {
            if (loaded)
            {
                throw new Exception("LivingAsset is already loaded");
            }
            
            LoadBytesIntoAssembliesAndAssetBundles();
            return true;
        }

        public string GetName()
        {
            return manifest.Name;
        }

        internal Dependency[] GetDependencies()
        {
            return manifest.Dependencies;
        }

        private void LoadBytesIntoAssembliesAndAssetBundles()
        {
            assemblies = new Assembly[assembliesBytes.Length];
            for (int i = 0; i < assembliesBytes.Length; i++)
            {
                assemblies[i] = Assembly.Load(assembliesBytes[i]);
            }

            assetBundles = new UnityEngine.AssetBundle[assetBundleBytes.Length];
            for (int i = 0; i < assetBundleBytes.Length; i++)
            {
                assetBundles[i] = UnityEngine.AssetBundle.LoadFromMemory(assetBundleBytes[i]);
            }

            // Free data
            assembliesBytes = null;
            assetBundleBytes = null;
        }

        internal void Initialize()
        {
            LivingAssetPolicy policy = new LivingAssetPolicy(manifest);
            for(int i=0; i< manifest.Libraries.Length; i++)
            {
                Library library = manifest.Libraries[i];
                if (!string.IsNullOrEmpty(library.Initializer))
                {
                    if (!policy.IsNamespaceValid(library.Initializer))
                    {
                        throw new Exception("Unable to execute initializer for library " + manifest.Name + ". The initializer class is not in the correct namespace.");
                    }
                    IInitializer initializer = (IInitializer) assemblies[i].CreateInstance(library.Initializer);
                    if (initializer == null)
                    {
                        throw new LoadingException("Unable to find initializer '" + library.Initializer + "' for LivingAsset '" + manifest.Name + "'");
                    } else
                    {
                        initializer.Initialize(manifest, assetBundles); // TODO: Defensive copying
                    }
                }
            }
        }
    }
}

//
// MIT License
// Copyright (c) EMP - https://github.com/SchmobertRitz/EMP-Unity
//
using System;
using System.Collections.Generic;
using System.Reflection;
using UnityEngine;

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

            public LivingAsset Get(string name)
            {
                if (!IsRegistered(name))
                {
                    throw new Exception("LivingAsset " + name + " is not registered.");
                }
                return registeredAssets[name];
             }
        }

        private bool IsLoaded()
        {
            return loaded;
        }

        private readonly Manifest manifest;
        private byte[][] assembliesBytes;
        private byte[][] apiBytes;
        private byte[][] assetBundleBytes;
        
        private Assembly[] assemblies;
        private UnityEngine.AssetBundle[] assetBundles;

        private bool loaded;
        
        internal LivingAsset(Manifest manifest, byte[][] assembliesBytes, byte[][] apiBytes, byte[][] assetBundleBytes)
        {
            this.manifest = manifest;
            this.assembliesBytes = assembliesBytes;
            this.apiBytes = apiBytes;
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

        public Type FindType(string name)
        {
            foreach(Assembly assembly in assemblies)
            {
                try
                {
                    return assembly.GetType(name);
                }
                catch (Exception e) { /* ignore */ } 
            }
            return null;
        }

        internal List<Dependency> GetDependencies()
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

            foreach(Assembly assembly in assemblies)
            {
                foreach(Type type in assembly.GetTypes())
                {
                    if (typeof(IInitializer).IsAssignableFrom(type))
                    {
                        if (!policy.IsNamespaceValid(type.FullName))
                        {
                            Debug.LogWarning("Type " + type.FullName + " in LivingAsset " + manifest.Name + " implements IInitializer but does not has the correct namespace. Ignoring.");
                        } else
                        {
                            IInitializer initializer = (IInitializer)Activator.CreateInstance(type);
                            if (initializer == null)
                            {
                                throw new LoadingException("Unable to instantiate initializer '" + type.FullName + "' for LivingAsset '" + manifest.Name + "'. Make sure the class has a no-arg constructor.");
                            }
                            else
                            {
                                initializer.Initialize(manifest, assetBundles); // TODO: Defensive copying
                            }
                        }
                    }
                }
            }
        }
    }
}

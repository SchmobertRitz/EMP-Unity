//
// MIT License
// Copyright (c) EMP - https://github.com/SchmobertRitz/EMP-Unity
//
using System;
using System.Collections.Generic;
using System.IO;
using System.IO.Compression;
using System.Security.Cryptography;
using UnityEngine;

namespace EMP.LivingAsset
{
    public class LivingAssetLoader
    {
        public const string LIVING_ASSET_HEADER = "/* EMP LivingAsset - Version 1.0.0 */\n";
        public const string LIVING_ASSET_FILE_EXTENSION = "LivingAsset";
        private readonly string name;
        private readonly ILivingAssetDatabase livingAssetDatabase;
        private Func<string, bool> dependencyLoadingPolicy;
        private readonly string rsaKeyXml;

        private bool alreadyCalled;

        public LivingAssetLoader(string name, ILivingAssetDatabase livingAssetDatabase, Func<string, bool> dependencyLoadingPolicy = null, string rsaKeyXml = null)
        {
            this.name = name;
            this.livingAssetDatabase = livingAssetDatabase;
            this.dependencyLoadingPolicy = dependencyLoadingPolicy;
            this.rsaKeyXml = rsaKeyXml;
        }
        
        public void Load(Action<LivingAsset> livingAssetHandler = null)
        {
            if (alreadyCalled)
            {
                throw new LoadingException("LivingAssetLoader.Load() must not be called more than once.");
            }
            alreadyCalled = true;

            Debug.Log("Start loading LivingAsset '" + name + "'");

            LookupAndParseLivingAsset(livingAssetHandler);
        }

        private void OnLivingAssetParsed(LivingAsset livingAsset, Action<LivingAsset> livingAssetHandler)
        {
            if (livingAssetHandler == null)
            {
                livingAssetHandler = _ => { };
            }
            if (livingAsset == null) {
                Debug.LogWarning("There where problems while lookup for " + name);
                livingAssetHandler(null);
                return;
            }
            if (!LivingAsset.GetRegistry().IsRegistered(livingAsset.GetName()))
            {
                LivingAsset.GetRegistry().Register(livingAsset);
                LoadDependencies(livingAsset, success => {
                    if (success)
                    {
                        livingAsset.Load();
                        livingAsset.Initialize();
                        Debug.Log("Successfully loaded LivingAsset '" + livingAsset.GetName() + "'");

                        livingAssetHandler(livingAsset);
                    } else
                    {
                        Debug.LogWarning("Unable to load dependencies for LivingAsset " + livingAsset.GetName());
                        livingAssetHandler(null);
                    }
                });
            }
            else
            {
                Debug.LogWarning("There is already a LivingAsset loaded with the name " + livingAsset.GetName());
                livingAssetHandler(null);
            }
        }

        private void LoadDependencies(LivingAsset livingAsset, Action<bool> successHandler)
        {
            if (livingAsset.GetDependencies() != null)
            {
                List<Dependency> dependencyList = new List<Dependency>(livingAsset.GetDependencies());
                ProcessDependencyList(dependencyList, successHandler);
                
            } else
            {
                successHandler(true);
            }
        }

        private void ProcessDependencyList(List<Dependency> dependencyList, Action<bool> successHandler)
        {
            if (dependencyList.Count == 0)
            {
                successHandler(true);
                return;
            }
            Dependency dependency = dependencyList[0];
            dependencyList.RemoveAt(0);

            if (!LivingAsset.GetRegistry().IsRegistered(dependency.Name))
            {
                if (dependencyLoadingPolicy == null || dependencyLoadingPolicy(dependency.Name))
                {
                    LivingAssetLoader loader = new LivingAssetLoader(dependency.Name, livingAssetDatabase, dependencyLoadingPolicy, rsaKeyXml);
                    loader.Load(livingAsset => {
                        if (livingAsset == null)
                        {
                            // There were problems loading the dependency. Abort.
                            successHandler(false);
                        } else
                        {
                            ProcessDependencyList(dependencyList, successHandler);
                        }
                    });
                }
                else
                {
                    Debug.LogWarning("Unable to load LivingAsset '" + dependency.Name + "': Rejected by loading policy.");
                    successHandler(false);
                }
            } else
            {
                ProcessDependencyList(dependencyList, successHandler);
            }

        }

        private void LookupAndParseLivingAsset(Action<LivingAsset> livingAssetHandler)
        {
            livingAssetDatabase.Lookup(name, inputStream => {
                if (inputStream == null) {
                    OnLivingAssetParsed(null, livingAssetHandler);
                    return;
                }
                bool usesCompression;
                ReadHeader(inputStream, out usesCompression);
                LivingAsset livingAsset;
                if (usesCompression)
                {
                    livingAsset = ReadFromCompressedStream(inputStream);
                }
                else
                {
                    livingAsset = ReadFromStream(inputStream);
                }
                OnLivingAssetParsed(livingAsset, livingAssetHandler);
            });
        }

        private Stream WrapInDecryptionStream(Stream inputStream)
        {
            BinaryReader reader = new BinaryReader(inputStream);

            int encryptedSymmetricKeyLen = reader.ReadInt32();
            byte[] encryptedSymmetricKey = reader.ReadBytes(encryptedSymmetricKeyLen);

            int encryptedSymmetricIVLen = reader.ReadInt32();
            byte[] encryptedSymmetricIV = reader.ReadBytes(encryptedSymmetricIVLen);

            if (rsaKeyXml == null)
            {
                if (encryptedSymmetricKey.Length == 0 && encryptedSymmetricIV.Length == 0)
                {
                    return inputStream;
                }
                else
                {
                    throw new LoadingException("Unable to decrypt LivingAsset because no public key was given.");
                }
            }
            else if (encryptedSymmetricKey.Length == 0 || encryptedSymmetricIV.Length == 0) {
                throw new LoadingException("Loading LivingAsset rejected: LivingAsset seems to be unencrypted but a public key was given to loader.");
            }

            RSACryptoServiceProvider RSA = new RSACryptoServiceProvider();
            RSA.FromXmlString(rsaKeyXml);

            RijndaelManaged AES = new RijndaelManaged();
            AES.Key = RSA.Decrypt(encryptedSymmetricKey, false);
            AES.IV = RSA.Decrypt(encryptedSymmetricIV, false);

            return new CryptoStream(inputStream, AES.CreateDecryptor(), CryptoStreamMode.Read);
        }

        private void ReadHeader(Stream inputStream, out bool usesCompression)
        {
            BinaryReader reader = new BinaryReader(inputStream);
            char[] expected = LIVING_ASSET_HEADER.ToCharArray();
            char[] read = reader.ReadChars(expected.Length);

            for (int i=0; i<expected.Length; i++)
            {
                if (expected[i] != read[i])
                {
                    throw new LoadingException("Unable to load Living Asset: Unsupported file format.");
                }
            }
            
            usesCompression = reader.ReadBoolean();
            // Do not close the reader
        }

        private LivingAsset ReadFromCompressedStream(Stream inputStream)
        {
            using (GZipStream decompressionStream = new GZipStream(inputStream, CompressionMode.Decompress))
            {
                return ReadFromStream(decompressionStream);
            }
        }

        private LivingAsset ReadFromStream(Stream inputStream)
        {
            using (BinaryReader reader = new BinaryReader(WrapInDecryptionStream(inputStream)))
            {
                return new LivingAsset(
                    Manifest.CreateFromBytes(ReadLengthPrefixedBytes(reader)),
                    ReadByteArrays(reader), // Assemblies
                    ReadByteArrays(reader) // AssetBundles
                );
            }
        }

        private byte[][] ReadByteArrays(BinaryReader reader)
        {
            byte[][] result = new byte[reader.ReadInt32()][];
            for(int i=0; i< result.Length; i++)
            {
                result[i] = ReadLengthPrefixedBytes(reader);
            }
            return result;
        }

        private byte[] ReadLengthPrefixedBytes(BinaryReader reader)
        {
            int len = reader.ReadInt32();
            byte[] buffer = new byte[len];
            reader.Read(buffer, 0, len);
            return buffer;
        }
    }
}

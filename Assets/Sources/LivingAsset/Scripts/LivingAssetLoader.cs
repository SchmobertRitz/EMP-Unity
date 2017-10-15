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
        public enum ESignatureCheckPolicy
        {
            SignatureMustAlwaysBePresentAndVerified,
            SignatureMustBeVerifiedIfPresent,
            SkipVerfication
        }

        public const string LIVING_ASSET_HEADER = "/* EMP LivingAsset - Version 1.0.0 */\n";
        public const string LIVING_ASSET_FILE_EXTENSION = "LivingAsset";

        private readonly string name;
        private readonly ILivingAssetDatabase livingAssetDatabase;
        private readonly Func<string, bool> dependencyLoadingPolicy;
        private readonly string rsaKeyXml;
        private readonly ESignatureCheckPolicy signatureCheckPolicy;

        private bool alreadyCalled;


        public LivingAssetLoader(string name, ILivingAssetDatabase livingAssetDatabase, ESignatureCheckPolicy signatureCheckPolicy, string rsaKeyXml = null, Func < string, bool> dependencyLoadingPolicy = null)
        {
            this.name = name;
            this.livingAssetDatabase = livingAssetDatabase;
            this.dependencyLoadingPolicy = dependencyLoadingPolicy;
            this.rsaKeyXml = rsaKeyXml;
            this.signatureCheckPolicy = signatureCheckPolicy;
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
                    LivingAssetLoader loader = new LivingAssetLoader(dependency.Name, livingAssetDatabase, signatureCheckPolicy, rsaKeyXml, dependencyLoadingPolicy);
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
                VerifySignature(inputStream);
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

        private void VerifySignature(Stream inputStream)
        {
            BinaryReader reader = new BinaryReader(inputStream);
            int signatureLen = reader.ReadInt32();
            byte[] signingData = reader.ReadBytes(signatureLen);

            if (signatureCheckPolicy == ESignatureCheckPolicy.SignatureMustAlwaysBePresentAndVerified)
            {
                if (signingData.Length == 0)
                {
                    throw new LoadingException("Unable to load '" + name + "': Signature check is mandatory but LivingAsset is not signed.");
                }
                if (rsaKeyXml == null)
                {
                    throw new LoadingException("Unable to load '" + name + "': Signature check is mandatory but no RSA key is given for verifying.");
                }
            }

            if (signatureCheckPolicy == ESignatureCheckPolicy.SignatureMustBeVerifiedIfPresent && signingData.Length != 0)
            {
                if (rsaKeyXml == null)
                {
                    throw new LoadingException("Unable to load '" + name + "': Signature is present but no RSA key is given for verifying.");
                }

                long position = inputStream.Position;

                using (RSACryptoServiceProvider RSA = new RSACryptoServiceProvider())
                {
                    RSA.FromXmlString(rsaKeyXml);
                    SHA1CryptoServiceProvider hasher = new SHA1CryptoServiceProvider();
                    byte[] hash = hasher.ComputeHash(inputStream);
                    if (!RSA.VerifyData(hash, new SHA1CryptoServiceProvider(), signingData))
                    {
                        throw new LoadingException("Unable to load '" + name + "': Signature does not match the content of the LivingAsset or public key does not apply.");
                    }
                }
                inputStream.Seek(position, SeekOrigin.Begin);
            }
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
            using (BinaryReader reader = new BinaryReader(inputStream))
            {
                return new LivingAsset(
                    Manifest.CreateFromBytes(ReadLengthPrefixedBytes(reader)),
                    ReadByteArrays(reader), // Assemblies
                    ReadByteArrays(reader), // APIs
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

//
// MIT License
// Copyright (c) EMP - https://github.com/SchmobertRitz/EMP-Unity
//
using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.IO.Compression;
using System.Reflection;
using UnityEngine;

namespace EMP.LivingAsset
{
    public class LivingAssetLoader
    {
        public const string LIVING_ASSET_HEADER = "/* EMP LivingAsset - Version 1.0.0 */\n";
        private readonly string file;

        private bool alreadyCalled;

        public LivingAssetLoader(string file)
        {
            this.file = file;
        }

        // Todo: Register laoded asset
        public LivingAsset Load()
        {
            if (alreadyCalled)
            {
                throw new LoadingException("LivingAssetLoader.Load() must not be called more than once.");
            }
            alreadyCalled = true;

            Debug.Log("Start loading LivingAsset '" + file + "'");

            LivingAsset livingAsset = OpenFileAndReadLivingAsset();

            if (!LivingAsset.GetRegistry().IsRegistered(livingAsset.GetName()))
            {
                LivingAsset.GetRegistry().Register(livingAsset);
                if (LoadDependencies(livingAsset))
                {
                    livingAsset.Load();
                    livingAsset.Initialize();
                    Debug.Log("Successfully loaded LivingAsset '" + livingAsset.GetName() + "' from file '" + file + "'");

                    return livingAsset;
                } else
                {
                    Debug.LogWarning("Unable to load dependencies for LivingAsset " + livingAsset.GetName());
                    return null;
                }

            } else
            {
                Debug.LogWarning("There is already a LivingAsset loaded with the name " + livingAsset.GetName());
                return null;
            }
        }

        private bool LoadDependencies(LivingAsset livingAsset)
        {
            bool success = true;
            if (livingAsset.GetDependencies() != null)
            {
                foreach (Dependency dependency in livingAsset.GetDependencies())
                {
                    if (!LivingAsset.GetRegistry().IsRegistered(dependency.Name))
                    {
                        LivingAssetLoader loader = new LivingAssetLoader(dependency.File);
                        success &= loader.Load() != null;
                    }
                }
            }
            return success;
        }

        private LivingAsset OpenFileAndReadLivingAsset()
        {
            using (FileStream fileStream = File.OpenRead(file))
            {
                bool usesCompression;
                ReadHeader(fileStream, out usesCompression);
                if (usesCompression)
                {
                    return ReadFromCompressedStream(fileStream);
                }
                else
                {
                    return ReadFromStream(fileStream);
                }
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

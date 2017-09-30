//
// MIT License
// Copyright (c) EMP - https://github.com/SchmobertRitz/EMP-Unity
//
using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.IO.Compression;
using UnityEngine;

namespace EMP.LivingAsset
{
    public class Archiver
    {
        private readonly string buildPath;
        private readonly Manifest manifest;
        private readonly string path;
        private readonly bool useCompression;

        public Archiver(string path, string buildPath, Manifest manifest, bool useCompression = true)
        {
            this.path = path;
            this.buildPath = buildPath;
            this.manifest = manifest;
            this.useCompression = useCompression;
        }

        public void GenerateArchive()
        {
            using (FileStream fileOutputStream = File.Create(Path.Combine(buildPath, manifest.Name + ".LivingAsset")))
            {
                WriteHeader(fileOutputStream);

                if (useCompression)
                {
                    WriteDataCompressedInFile(fileOutputStream);
                } else
                {
                    WriteDataInFile(fileOutputStream);
                }
            }
        }

        private void WriteHeader(FileStream fileOutputStream)
        {
            BinaryWriter writer = new BinaryWriter(fileOutputStream);
            writer.Write(LivingAssetLoader.LIVING_ASSET_HEADER.ToCharArray());
            writer.Write(useCompression);
            // do not close the stream
        }

        private void WriteDataCompressedInFile(Stream outputStream)
        {
            using (GZipStream compressionStream = new GZipStream(outputStream, CompressionMode.Compress))
            {
                WriteDataInFile(compressionStream);
            }
        }

        private void WriteDataInFile(Stream outputStream)
        {
            using (BinaryWriter writer = new BinaryWriter(outputStream))
            {
                WriteFile(Path.Combine(buildPath, Manifest.FILE_NAME), writer);
                WriteLibraries(buildPath, manifest.Libraries, writer);
                WriteAssetBundles(buildPath, manifest.AssetBundles, writer);
            }
        }

        private void WriteLibraries(string buildPath, Library[] libraries, BinaryWriter writer)
        {
            writer.Write(libraries.Length);
            foreach (Library library in libraries)
            {
                WriteFile(Path.Combine(buildPath, library.File), writer);
            }
        }

        private void WriteAssetBundles(string buildPath, AssetBundle[] assetBundles, BinaryWriter writer)
        {
            writer.Write(assetBundles.Length);
            foreach (AssetBundle assetBundle in assetBundles)
            {
                WriteFile(Path.Combine(buildPath, assetBundle.File), writer);
            }
        }

        private void WriteFile(string filename, BinaryWriter writer)
        {
            writer.Write((int) new FileInfo(filename).Length);
            using (FileStream manifestStream = File.OpenRead(filename))
            {
                byte[] buffer = new byte[512];
                int read = 0;
                while ((read = manifestStream.Read(buffer, 0, buffer.Length)) > 0)
                {
                    writer.Write(buffer, 0, read);
                }
            }
        }
        
    }
}

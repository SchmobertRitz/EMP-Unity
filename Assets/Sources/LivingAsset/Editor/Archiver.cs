//
// MIT License
// Copyright (c) EMP - https://github.com/SchmobertRitz/EMP-Unity
//
using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.IO.Compression;
using System.Security.Cryptography;
using UnityEngine;

namespace EMP.LivingAsset
{
    public class Archiver
    {
        private readonly string buildPath;
        private readonly Manifest manifest;
        private readonly string libsPath;
        private readonly string apiPath;
        private readonly string assetsPath;
        private readonly bool useCompression;
        private readonly string rsaKeyXml;

        public Archiver(string libsPath, string apiPath, string assetsPath, string buildPath, Manifest manifest, bool useCompression = true, string rsaKeyXml = null)
        {
            this.libsPath = libsPath;
            this.apiPath = apiPath;
            this.assetsPath = assetsPath;
            this.buildPath = buildPath;
            this.manifest = manifest;
            this.useCompression = useCompression;
            this.rsaKeyXml = rsaKeyXml;
        }

        public void GenerateArchive()
        {
            // Write payload in a separat file
            string tmpFile = Path.Combine(buildPath, manifest.Name + ".tmp");
            using (FileStream fileOutputStream = File.Create(tmpFile))
            {
                WriteDataCompressedInFile(fileOutputStream);
            }

            byte[] signingData = new byte[0];
            if (rsaKeyXml != null)
            {
                // Generate signing hash of content of temp file
                using (RSACryptoServiceProvider RSA = new RSACryptoServiceProvider())
                {
                    RSA.FromXmlString(rsaKeyXml);
                    if (RSA.PublicOnly)
                    {
                        throw new Exception("Missing private key info for signing the content");
                    }
                    
                    using (FileStream stream = File.OpenRead(tmpFile))
                    {
                        SHA1CryptoServiceProvider hasher = new SHA1CryptoServiceProvider();
                        byte[] hash = hasher.ComputeHash(stream);
                        signingData = RSA.SignData(hash, new SHA1CryptoServiceProvider());
                    }
                }   
            }

            using (FileStream fileOutputStream = File.Create(Path.Combine(buildPath, manifest.Name + ".LivingAsset")))
            {
                using(BinaryWriter writer = new BinaryWriter(fileOutputStream))
                {
                    writer.Write(LivingAssetLoader.LIVING_ASSET_HEADER.ToCharArray());
                    writer.Write(useCompression);
                    writer.Write(signingData.Length);
                    writer.Write(signingData);

                    using (FileStream fileInputStream = File.OpenRead(tmpFile))
                    {
                        byte[] buffer = new byte[1024];
                        int read = 0;
                        while ((read = fileInputStream.Read(buffer, 0, buffer.Length)) > 0)
                        {
                            writer.Write(buffer, 0, read);
                        }
                    }
                }
            }
        }

        private void WriteDataCompressedInFile(Stream outputStream)
        {
            if (useCompression)
            {
                using (GZipStream compressionStream = new GZipStream(outputStream, CompressionMode.Compress))
                {
                    WriteDataInArchive(compressionStream);
                }
            } else
            {
                WriteDataInArchive(outputStream);
            }
        }

        private void WriteDataInArchive(Stream outputStream)
        {
            using (BinaryWriter writer = new BinaryWriter(outputStream))
            {
                WriteFile(Path.Combine(buildPath, Manifest.FILE_NAME), writer);
                WriteListOfFiles(buildPath, FilesHelper.CollectFiles(libsPath, file => file.EndsWith(".dll")), writer);
                WriteListOfFiles(buildPath, FilesHelper.CollectFiles(apiPath, file => file.EndsWith(".cs")), writer);
                WriteListOfFiles(buildPath, FilesHelper.CollectFiles(assetsPath, file => Path.GetFileName(file).Equals("bundle.asset")), writer);
            }
        }

        private void WriteListOfFiles(string buildPath, List<string> files, BinaryWriter writer)
        {
            writer.Write(files.Count);
            foreach (string library in files)
            {
                WriteFile(library, writer);
            }
        }
        
        private void WriteFile(string filename, BinaryWriter writer)
        {
            writer.Write((int) new FileInfo(filename).Length);
            using (FileStream stream = File.OpenRead(filename))
            {
                byte[] buffer = new byte[1024];
                int read = 0;
                while ((read = stream.Read(buffer, 0, buffer.Length)) > 0)
                {
                    writer.Write(buffer, 0, read);
                }
            }
        }
        
    }
}

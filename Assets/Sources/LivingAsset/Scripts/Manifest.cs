//
// MIT License
// Copyright (c) EMP - https://github.com/SchmobertRitz/EMP-Unity
//
using System.IO;
using System.Xml.Serialization;
using UnityEngine;

namespace EMP.LivingAsset
{
    [XmlRoot]
    public class Manifest
    {
        public const string FILE_NAME = "Manifest.xml";

        public static Manifest CreateFromPath(string manifestPath)
        {
            XmlSerializer serializer = new XmlSerializer(typeof(Manifest));
            FileStream fs = new FileStream(manifestPath, FileMode.Open);
            Manifest manifest = (Manifest)serializer.Deserialize(fs);
            return manifest;
        }

        public static Manifest CreateFromBytes(byte[] bytes)
        {
            XmlSerializer serializer = new XmlSerializer(typeof(Manifest));
            MemoryStream ms = new MemoryStream(bytes);
            Manifest manifest = (Manifest)serializer.Deserialize(ms);
            return manifest;
        }

        [XmlAttribute("name")]
        public string Name;

        [XmlAttribute("description")]
        public string Description;
        
        [XmlArray]
        public AssetBundle[] AssetBundles;

        [XmlArray]
        public Library[] Libraries;
    }

    public class Library
    {
        [XmlAttribute("file")]
        public string File;

        [XmlAttribute("initializer")]
        public string Initializer;
    }

    public class AssetBundle
    {
        [XmlAttribute("file")]
        public string File;
    }
}

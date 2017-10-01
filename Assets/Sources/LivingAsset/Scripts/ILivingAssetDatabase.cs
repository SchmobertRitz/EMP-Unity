//
// MIT License
// Copyright (c) EMP - https://github.com/SchmobertRitz/EMP-Unity
//
using System;
using System.IO;

namespace EMP.LivingAsset
{
    public interface ILivingAssetDatabase
    {
        void Lookup(string name, Action<Stream> inputStreamHandler);
    }
}

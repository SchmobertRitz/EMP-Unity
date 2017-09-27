//
// MIT License
// Copyright (c) EMP - https://github.com/SchmobertRitz/EMP-Unity
//
using UnityEngine;

namespace EMP.LivingAsset
{
    public interface ILivingAssetFactory
    {
        GameObject CreateGameObject(string smartObjectName = null);
    }
}

//
// MIT License
// Copyright (c) EMP - https://github.com/SchmobertRitz/EMP-Unity
//
using System;

namespace EMP.Wire
{
    [AttributeUsage(AttributeTargets.Method | AttributeTargets.Class)]
    public class SingletonAttribute : Attribute
    {
    }
}
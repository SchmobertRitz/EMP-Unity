//
// MIT License
// Copyright (c) EMP - https://github.com/SchmobertRitz/EMP-Unity
//
using System;

namespace EMP.Wire
{
    [AttributeUsage(AttributeTargets.Method | AttributeTargets.Parameter)]
    public class NamedAttribute : Attribute
    {
        public string name;

        public NamedAttribute(string name)
        {
            this.name = name;
        }
    }
}
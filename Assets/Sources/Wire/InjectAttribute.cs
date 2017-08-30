//
// MIT License
// Copyright (c) EMP - https://github.com/SchmobertRitz/EMP-Unity
//
using System;

namespace EMP.Wire
{
    [AttributeUsage(AttributeTargets.Method | AttributeTargets.Field | AttributeTargets.Property | AttributeTargets.Constructor)]
    public class InjectAttribute : Attribute
    {
        public string name;

        public InjectAttribute(string name)
        {
            this.name = name;
        }

        public InjectAttribute() : this(null) { }
    }
}

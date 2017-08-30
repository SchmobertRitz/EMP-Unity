//
// MIT License
// Copyright (c) EMP - https://github.com/SchmobertRitz/EMP-Unity
//
using System;

namespace EMP.Wire {

    [AttributeUsage(AttributeTargets.Field | AttributeTargets.Property)]
    public class BindAttribute : Attribute
    {
        public string path;

        public BindAttribute(string path)
        {
            this.path = path;
        }

        public BindAttribute() : this(null) { }
    }

}
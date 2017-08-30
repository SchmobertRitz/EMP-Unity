//
// MIT License
// Copyright (c) EMP - https://github.com/SchmobertRitz/EMP-Unity
//
using System;

namespace EMP.Wire
{
    public class InstanceBinding : AbstractBinding
    {
        private object instance;

        public InstanceBinding(Wire wire, string name, Type type, object instance)
            : base(wire, name, type)
        {
            this.instance = instance;
        }

        public override object GetInstance()
        {
            return instance;
        }
    }
}
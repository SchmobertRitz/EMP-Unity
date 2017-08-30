//
// MIT License
// Copyright (c) EMP - https://github.com/SchmobertRitz/EMP-Unity
//
using System;

namespace EMP.Wire
{
    [AttributeUsage(AttributeTargets.Method)]
    public class GameObjectAttribute : Attribute
    {
        public string name;

        public GameObjectAttribute(string name)
        {
            this.name = name;
        }

        public GameObjectAttribute() : this(null) { }
    }
}


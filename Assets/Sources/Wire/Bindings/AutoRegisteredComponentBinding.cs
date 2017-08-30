//
// MIT License
// Copyright (c) EMP - https://github.com/SchmobertRitz/EMP-Unity
//
using System;
using UnityEngine;

namespace EMP.Wire
{
    public class AutoRegisteredComponentBinding : AbstractBinding
    {
        public AutoRegisteredComponentBinding(Wire wire, string name, Type type)
            : base(wire, name, type)
        { }

        public override object GetInstance()
        {
            string gameObjectName = BoundType.Name + (BoundName == null ? "" : "_" + BoundName);
            int c = 1;
            while (UnityEngine.GameObject.Find("/" + gameObjectName) != null)
            {
                gameObjectName = BoundType.Name + (BoundName == null ? "" : "_" + BoundName) + "(" + (c++) + ")";
            }

            GameObject instance = new GameObject(gameObjectName);
            return wire.Inject(instance.AddComponent(BoundType));
        }
    }
}
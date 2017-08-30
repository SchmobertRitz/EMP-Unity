//
// MIT License
// Copyright (c) EMP - https://github.com/SchmobertRitz/EMP-Unity
//
using System;
using System.Reflection;
using UnityEngine;

namespace EMP.Wire
{
    public class ModuleBinding : AbstractBinding
    {
        private MethodInfo methodInfo;
        private object module;

        public ModuleBinding(Wire wire, string name, Type type, MethodInfo methodInfo, object module)
            : base(wire, name, type)
        {
            this.methodInfo = methodInfo;
            this.module = module;
        }

        public override object GetInstance()
        {
            object instance = InvokationHelper.ResolveParametersAndInvokeMethod(wire, methodInfo, module);
            GameObjectAttribute gameObjectAttribute;
            if (instance is Type && TypeHelper.TryToGetAttribute(methodInfo, out gameObjectAttribute))
            {
                return Instantiate((Type)instance, gameObjectAttribute);
            }
            else if (instance is UnityEngine.Object && TypeHelper.TryToGetAttribute(methodInfo, out gameObjectAttribute))
            {
                return Instantiate((UnityEngine.Object)instance, gameObjectAttribute);
            }
            else
            {
                return instance;
            }
        }

        private object Instantiate(Type componentType, GameObjectAttribute gameObjectAttribute)
        {
            string gameObjectName;
            if (gameObjectAttribute.name == null)
            {
                gameObjectName = componentType.Name + (BoundName == null ? "" : "_" + BoundName);
                int c = 1;
                while (UnityEngine.GameObject.Find("/" + gameObjectName) != null)
                {
                    gameObjectName = componentType.Name + (BoundName == null ? "" : "_" + BoundName) + "(" + (c++) + ")";
                }
            }
            else
            {
                gameObjectName = gameObjectAttribute.name;
            }
            UnityEngine.GameObject instance = new UnityEngine.GameObject(gameObjectName);
            return instance.AddComponent(componentType);
        }

        private object Instantiate(UnityEngine.Object unityObject, GameObjectAttribute gameObjectAttribute)
        {
            string gameObjectName;
            if (gameObjectAttribute.name == null)
            {
                gameObjectName = BoundType.Name + (BoundName == null ? "" : "_" + BoundName);
                int c = 1;
                while (UnityEngine.GameObject.Find("/" + gameObjectName) != null)
                {
                    gameObjectName = BoundType.Name + (BoundName == null ? "" : "_" + BoundName) + "(" + (c++) + ")";
                }
            }
            else
            {
                gameObjectName = gameObjectAttribute.name;
            }
            //UnityEngine.GameObject instanceParent = new UnityEngine.GameObject(gameObjectName);
            GameObject instance = GameObject.Instantiate(unityObject as GameObject /*, instanceParent.transform*/) as GameObject;
            instance.name = gameObjectName;

            foreach (MonoBehaviour monoBehaviour in instance.GetComponentsInChildren<MonoBehaviour>())
            {
                if (monoBehaviour != null)
                {
                    wire.Inject(monoBehaviour);
                }
            }

            /* if (type.IsAssignableFrom(typeof(Component)))
            {
                return instance.GetComponent(type);
            } else*/
            if (BoundType.IsAssignableFrom(typeof(GameObject)))
            {
                return instance;
            }
            return instance.GetComponent(BoundType);
            //throw new Exception("Unexpected binding type " + type);
        }
    }
}
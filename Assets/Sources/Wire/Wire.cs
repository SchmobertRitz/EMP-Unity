using UnityEngine;
using System.Collections.Generic;
using System;
using System.Reflection;

public class Wire
{

    private class InvokationHelper
    {
        public static object ResolveParametersAndInvokeMethod(Wire wire, MethodInfo methodInfo, object @object)
        {
            ParameterInfo[] parameterInfos = methodInfo.GetParameters();
            object[] parameters = new object[parameterInfos.Length];
            for (int i = 0; i < parameterInfos.Length; i++)
            {
                ParameterInfo parameterInfo = parameterInfos[i];
                Type type = parameterInfo.ParameterType;
                string name = null;
                NamedAttribute namedAttribute;
                if (TypeHelper.TryToGetAttribute(parameterInfo, out namedAttribute))
                {
                    name = namedAttribute.name;
                }
                parameters[i] = wire.Get(name, type);
            }

            return methodInfo.Invoke(@object, parameters);
        }
    }

    private abstract class Binding
    {
        protected Wire wire;
        public Type type;
        public string name;

        public Binding(Wire wire, string name, Type type)
        {
            this.wire = wire;
            this.name = name;
            this.type = type;
        }

        public abstract object GetInstance();
    }

    private class InstanceBinding : Binding
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

    private class ModuleBinding : Binding
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
            if (instance is Type && TypeHelper.TryToGetAttribute(methodInfo, out gameObjectAttribute)) {
                return Instantiate((Type) instance, gameObjectAttribute);
            } else if (instance is UnityEngine.Object && TypeHelper.TryToGetAttribute(methodInfo, out gameObjectAttribute))
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
                gameObjectName = componentType.Name + (name == null ? "" : "_" + name);
                int c = 1;
                while (UnityEngine.GameObject.Find("/" + gameObjectName) != null)
                {
                    gameObjectName = componentType.Name + (name == null ? "" : "_" + name) + "(" + (c++) + ")";
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
                gameObjectName = type.Name + (name == null ? "" : "_" + name);
                int c = 1;
                while (UnityEngine.GameObject.Find("/" + gameObjectName) != null)
                {
                    gameObjectName = type.Name + (name == null ? "" : "_" + name) + "(" + (c++) + ")";
                }
            }
            else
            {
                gameObjectName = gameObjectAttribute.name;
            }
            //UnityEngine.GameObject instanceParent = new UnityEngine.GameObject(gameObjectName);
            GameObject instance = GameObject.Instantiate(unityObject as GameObject /*, instanceParent.transform*/) as GameObject;
            instance.name = gameObjectName;

            foreach(MonoBehaviour monoBehaviour in instance.GetComponentsInChildren<MonoBehaviour>())
            {
                wire.Inject(monoBehaviour);
            }

            if (type.IsAssignableFrom(typeof(Component)))
            {
                return instance.GetComponent(type);
            } else if (type.IsAssignableFrom(typeof(GameObject)))
            {
                return instance;
            }
            throw new Exception("Unexpected binding type " + type);
        }
    }

    private class SingletonModuleBinding : ModuleBinding
    {
        private object instance;

        public SingletonModuleBinding(Wire wire, string name, Type type, MethodInfo methodInfo, object module)
            : base(wire, name, type, methodInfo, module) { }

        public override object GetInstance()
        {
            if (instance == null)
            {
                instance = base.GetInstance();
            }
            return instance;
        }
    }

    private Dictionary<string, Binding> bindings = new Dictionary<string, Binding>();

    public void BindInstance<T>(string name, object instance)
    {
        AddBinding(new InstanceBinding(this, name, typeof(T), instance));
    }

    public void BindInstance<T>(object instance)
    {
        BindInstance<T>(null, instance);
    }

    public void RegisterModule(object module)
    {
        foreach (MethodInfo methodInfo in TypeHelper.AllMethodsOf(module.GetType()))
        {
            ProvidesAttribute providesAttribute;
            if (TypeHelper.TryToGetAttribute(methodInfo, out providesAttribute))
            {
                string name = providesAttribute.name;
                Type type = (providesAttribute.interfaceType != null ? providesAttribute.interfaceType : methodInfo.ReturnType);
                
                if (TypeHelper.HasAttribute<SingletonAttribute>(methodInfo))
                {
                    AddBinding(new SingletonModuleBinding(this, name, type, methodInfo, module));
                } else
                {
                    AddBinding(new ModuleBinding(this, name, type, methodInfo, module));
                }
            }
        }
    }

    public Wire()
    {
        BindInstance<Wire>(this);
    }

    public object Inject(object @object)
    {
        List<MemberInfo> injectionMembers = FindInjectionMembers(@object);
        List<MethodInfo> injectionMethods = FindInjectionMethods(@object);

        foreach (MemberInfo memberInfo in injectionMembers)
        {
            InjectAttribute injectAttribute;
            if (TypeHelper.TryToGetAttribute(memberInfo, out injectAttribute))
            {
                if (memberInfo is FieldInfo)
                {
                    (memberInfo as FieldInfo).SetValue(@object, this.Get(injectAttribute.name, (memberInfo as FieldInfo).FieldType));
                }
                else if (memberInfo is PropertyInfo)
                {
                    (memberInfo as PropertyInfo).SetValue(@object, this.Get(injectAttribute.name, (memberInfo as PropertyInfo).PropertyType), null);
                }
            }
        }

        foreach (MethodInfo methodInfo in injectionMethods)
        {
            InvokationHelper.ResolveParametersAndInvokeMethod(this, methodInfo, @object);
        }

        return @object;
    }

    private List<MethodInfo> FindInjectionMethods(object @object)
    {
        List<MethodInfo> result = new List<MethodInfo>();
        foreach(MethodInfo methodInfo in TypeHelper.AllMethodsOf(@object.GetType()))
        {
            if (TypeHelper.HasAttribute<InjectAttribute>(methodInfo))
            {
                result.Add(methodInfo);
            }
        }
        return result;
    }

    private List<MemberInfo> FindInjectionMembers(object @object)
    {
        List<MemberInfo> result = new List<MemberInfo>();
        foreach (MemberInfo memberInfo in TypeHelper.AllMembersOf(@object.GetType()))
        {
            if (TypeHelper.HasAttribute<InjectAttribute>(memberInfo))
            {
                result.Add(memberInfo);
            }
        }
        return result;
    }

    private void AddBinding(Binding binding)
    {
        string key = GetKeyForBinding(binding.name, binding.type);
        if (!bindings.ContainsKey(key))
        {
            bindings.Add(key, binding);
        }
        else
        {
            throw new Exception("Already a binding available for " + binding.type.Name + (binding.name != null ? " named with " + binding.name + "." : "."));
        }
    }

    public object Get(string name, Type type)
    {
        Binding binding;
        if (bindings.TryGetValue(GetKeyForBinding(name, type), out binding))
        {
            return binding.GetInstance();
        }
        throw new Exception("No binding found for " + type.Name + (name != null ? " named with " + name + "." : "."));
    }

    public T Get<T>(string name)
    {
        return (T)Get(name, typeof(T));
    }

    public T Get<T>()
    {
        return Get<T>(null);
    }



    private string GetKeyForBinding(string name, Type type)
    {
        return (name != null ? name.Trim() + "::" : "") + type.Name;
    }

}

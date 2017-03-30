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
            return methodInfo.Invoke(@object, ResolveParameters(wire, methodInfo));
        }

        public static object ResolveParametersAndInvokeConstructor(Wire wire, ConstructorInfo constructorInfo)
        {
            return constructorInfo.Invoke(ResolveParameters(wire, constructorInfo));
        }

        public static object[] ResolveParameters(Wire wire, MethodBase methodBase)
        {
            ParameterInfo[] parameterInfos = methodBase.GetParameters();
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
            return parameters;
        }
    }

    private interface IBinding
    {
        Type BoundType
        { get; }

        string BoundName
        { get; }

        object GetInstance();
    }

    private class SingletonBindingDecorator : IBinding
    {
        private object instance;
        private IBinding delegateBinding;

        public Type BoundType
        { get { return delegateBinding.BoundType;  } }

        public string BoundName
        { get { return delegateBinding.BoundName; } }

        public SingletonBindingDecorator(IBinding delegateBinding)
        {
            this.delegateBinding = delegateBinding;
        }

        public object GetInstance()
        {
            if (instance == null)
            {
                instance = delegateBinding.GetInstance();
            }
            return instance;
        }
    }

    private abstract class Binding : IBinding
    {
        protected Wire wire;

        public Type BoundType
        { get; private set; }

        public string BoundName
        { get; private set; }

    public Binding(Wire wire, string name, Type type)
        {
            this.wire = wire;
            this.BoundName = name;
            this.BoundType = type;
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

            foreach(MonoBehaviour monoBehaviour in instance.GetComponentsInChildren<MonoBehaviour>())
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

    private class AutoRegisteredPocoBinding : Binding
    {
        private ConstructorInfo constructorInfo;

        public AutoRegisteredPocoBinding(Wire wire, string name, Type type, ConstructorInfo methodInfo)
            : base(wire, name, type)
        {
            this.constructorInfo = methodInfo;
        }

        public override object GetInstance()
        {
            object instance = InvokationHelper.ResolveParametersAndInvokeConstructor(wire, constructorInfo);
            return wire.Inject(instance);
        }
    }


    private class AutoRegisteredComponentBinding : Binding
    {
        public AutoRegisteredComponentBinding(Wire wire, string name, Type type)
            : base(wire, name, type)
        {}

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

    private Dictionary<string, IBinding> bindings = new Dictionary<string, IBinding>();

    public Wire()
    {
        BindInstance<Wire>(this);
    }

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

                IBinding binding = new ModuleBinding(this, name, type, methodInfo, module);
                if (TypeHelper.HasAttribute<SingletonAttribute>(methodInfo))
                {
                    binding = new SingletonBindingDecorator(binding);
                }
                AddBinding(binding);
            }
        }
    }

    private void TryToAutoRegisterBinding(string name, Type type)
    {
        ConstructorInfo defaultConstructor = null;
        ConstructorInfo injectableConstructor = null;
        foreach (ConstructorInfo constructorInfo in type.GetConstructors())
        {
            ParameterInfo[] parameterInfos = constructorInfo.GetParameters();
            if (parameterInfos.Length == 0)
            {
                defaultConstructor = constructorInfo;
            }
            if (TypeHelper.HasAttribute<InjectAttribute>(constructorInfo))
            {
                if (injectableConstructor != null)
                {
                    throw new Exception("Multiple injectable constructors found in type " + type + ". Only one is allowed.");
                }
                injectableConstructor = constructorInfo;
            }
        }
        ConstructorInfo constructor = null;
        if (injectableConstructor != null) {
            constructor = injectableConstructor;
        } else if (defaultConstructor != null)
        {
            constructor = defaultConstructor;
        } else
        {
            throw new Exception("Unable to auto bind type " + type + ". No injectable constructor and no default constructor found.");
        }
        IBinding binding;
        if (type.IsSubclassOf(typeof(Component)))
        {
            binding = new AutoRegisteredComponentBinding(this, name, type);
        }
        else
        {
            binding = new AutoRegisteredPocoBinding(this, name, type, constructor);
        }
        if (TypeHelper.HasAttribute<SingletonAttribute>(type))
        {
            binding = new SingletonBindingDecorator(binding);
        }
        AddBinding(binding);
    }

    public T Inject<T>(T @object)
    {
        if (@object == null)
        {
            throw new NullReferenceException("Null was given to Wire.inject.");
        }
        List<MemberInfo> injectionMembers = FindInjectionMembers(@object);
        List<MethodInfo> injectionMethods = FindInjectionMethods(@object);

        foreach (MemberInfo memberInfo in injectionMembers)
        {
            InjectAttribute injectAttribute;
            BindAttribute bindAttribute;

            if (TypeHelper.TryToGetAttribute(memberInfo, out injectAttribute))
            {
                if (memberInfo is FieldInfo)
                {
                    (memberInfo as FieldInfo).SetValue(@object, this.Get(injectAttribute.name, (memberInfo as FieldInfo).FieldType));
                }
                else if(memberInfo is PropertyInfo)
                {
                    (memberInfo as PropertyInfo).SetValue(@object, this.Get(injectAttribute.name, (memberInfo as PropertyInfo).PropertyType), null);
                }
            }

            if (@object is MonoBehaviour && TypeHelper.TryToGetAttribute(memberInfo, out bindAttribute))
            {
                Type fieldType = (memberInfo is FieldInfo) ? (memberInfo as FieldInfo).FieldType : (memberInfo as PropertyInfo).PropertyType;
                object objectToBind = FindGameObjectByPath(@object as MonoBehaviour, bindAttribute.path);

                if (fieldType.IsSubclassOf(typeof(Component))) {
                    objectToBind = (objectToBind as GameObject).GetComponent(fieldType);
                }

                if (memberInfo is FieldInfo)
                {
                    (memberInfo as FieldInfo).SetValue(@object, objectToBind);
                }
                else
                {
                    (memberInfo as PropertyInfo).SetValue(@object, objectToBind, null);
                }
            }
        }

        foreach (MethodInfo methodInfo in injectionMethods)
        {
            InvokationHelper.ResolveParametersAndInvokeMethod(this, methodInfo, @object);
        }

        return @object;
    }

    private GameObject FindGameObjectByPath(MonoBehaviour start, string path)
    {
        GameObject result = start.gameObject;
        if (path != null)
        {
            foreach (string token in path.Split('/'))
            {
                if (token.Equals(".."))
                {
                    result = result.transform.parent.gameObject;
                } else
                {
                    result = result.transform.FindChild(token).gameObject;
                }
            }
        }
        return result;
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
            if (TypeHelper.HasAttribute<InjectAttribute>(memberInfo)
                || TypeHelper.HasAttribute<BindAttribute>(memberInfo))
            {
                result.Add(memberInfo);
            }
        }
        return result;
    }

    private void AddBinding(IBinding binding)
    {
        string key = GetKeyForBinding(binding.BoundName, binding.BoundType);
        if (!bindings.ContainsKey(key))
        {
            bindings.Add(key, binding);
        }
        else
        {
            throw new Exception("Already a binding available for " + binding.BoundType.Name + (binding.BoundName != null ? " named with " + binding.BoundName + "." : "."));
        }
    }

    private object GetInternal(string name, Type type, bool allowNullResult)
    {
        IBinding binding;
        if (bindings.TryGetValue(GetKeyForBinding(name, type), out binding))
        {
            object result = binding.GetInstance();
            if (result == null && !allowNullResult)
            {
                throw new Exception("Binding returns null.");
            }
            return result;
        }

        TryToAutoRegisterBinding(name, type);

        if (bindings.TryGetValue(GetKeyForBinding(name, type), out binding))
        {
            object result = binding.GetInstance();
            if (result == null && !allowNullResult)
            {
                throw new Exception("Binding returns null.");
            }
            return result;
        }
        throw new Exception("No binding found for " + type.Name + (name != null ? " named with " + name + "." : "."));
    }

    public object Get(string name, Type type)
    {
        return GetInternal(name, type, false);
    }

    public T Get<T>(string name)
    {
        return (T) Get(name, typeof(T));
    }

    public T Get<T>()
    {
        return Get<T>(null);
    }

    public object NullableGet(string name, Type type)
    {
        return GetInternal(name, type, true);
    }

    public T NullableGet<T>(string name)
    {
        return (T) NullableGet(name, typeof(T));
    }

    public T NullableGet<T>()
    {
        return NullableGet<T>(null);
    }


    public GameObject InstantiateResource(string resource)
    {
        return InstantiatePrefab((GameObject) Resources.Load(resource));
    }

    public GameObject InstantiatePrefab(GameObject prefab)
    {
        GameObject instance = (GameObject.Instantiate(prefab));
        foreach (MonoBehaviour monoBehaviour in instance.GetComponentsInChildren<MonoBehaviour>())
        {
            if (monoBehaviour != null)
            {
                Inject(monoBehaviour);
            }
        }
        return instance;
    }

    private string GetKeyForBinding(string name, Type type)
    {
        return (name != null ? name.Trim() + "::" : "") + type.Name;
    }

}

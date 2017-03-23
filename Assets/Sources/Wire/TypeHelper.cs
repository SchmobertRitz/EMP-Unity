using System;
using System.Collections.Generic;
using System.Reflection;

public class TypeHelper {

    public static bool TryToGetAttribute<TAttribute>(MethodInfo methodInfo, out TAttribute attribute) where TAttribute : Attribute
    {
        attribute = Attribute.GetCustomAttribute(methodInfo, typeof(TAttribute)) as TAttribute;
        return (attribute != null);
    }

    public static bool TryToGetAttribute<TAttribute>(ParameterInfo parameterInfo, out TAttribute attribute) where TAttribute : Attribute
    {
        attribute = Attribute.GetCustomAttribute(parameterInfo, typeof(TAttribute)) as TAttribute;
        return (attribute != null);
    }

    public static bool TryToGetAttribute<TAttribute>(MemberInfo memberInfo, out TAttribute attribute) where TAttribute : Attribute
    {
        attribute = Attribute.GetCustomAttribute(memberInfo, typeof(TAttribute)) as TAttribute;
        return (attribute != null);
    }

    public static bool HasAttribute<TAttribute>(MethodInfo methodInfo) where TAttribute : Attribute
    {
        return Attribute.GetCustomAttribute(methodInfo, typeof(TAttribute)) as TAttribute != null;
    }

    public static bool HasAttribute<TAttribute>(MemberInfo memberInfo) where TAttribute : Attribute
    {
        return Attribute.GetCustomAttribute(memberInfo, typeof(TAttribute)) as TAttribute != null;
    }

    public static bool HasAttribute<TAttribute>(Type type) where TAttribute : Attribute
    {
        return Attribute.GetCustomAttribute(type, typeof(TAttribute)) as TAttribute != null;
    }

    public static bool HasAttribute<TAttribute>(ConstructorInfo constructorInfo) where TAttribute : Attribute
    {
        return Attribute.GetCustomAttribute(constructorInfo, typeof(TAttribute)) as TAttribute != null;
    }

    public static MethodInfo[] AllMethodsOf(Type type)
    {
        return type.GetMethods(BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.Instance | BindingFlags.DeclaredOnly);
    }

    public static MemberInfo[] AllMembersOf(Type type)
    {
        return type.GetMembers(BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.Instance | BindingFlags.DeclaredOnly);
    }

}

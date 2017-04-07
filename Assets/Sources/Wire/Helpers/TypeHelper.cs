using System;
using System.Collections;
using System.Collections.Generic;
using System.Reflection;

namespace EMP.Wire
{
    public class TypeHelper
    {

        public static bool TryToGetAttribute<TAttribute>(MethodInfo methodInfo, out TAttribute attribute) where TAttribute : Attribute
        {
            attribute = FirstOrNull(methodInfo.GetCustomAttributes(typeof(TAttribute), true)) as TAttribute;
            return (attribute != null);
        }

        public static bool TryToGetAttribute<TAttribute>(ParameterInfo parameterInfo, out TAttribute attribute) where TAttribute : Attribute
        {
            attribute = FirstOrNull(parameterInfo.GetCustomAttributes(typeof(TAttribute), true)) as TAttribute;
            return (attribute != null);
        }

        public static bool TryToGetAttribute<TAttribute>(MemberInfo memberInfo, out TAttribute attribute) where TAttribute : Attribute
        {
            attribute = FirstOrNull(memberInfo.GetCustomAttributes(typeof(TAttribute), true)) as TAttribute;
            return (attribute != null);
        }

        public static bool HasAttribute<TAttribute>(MethodInfo methodInfo) where TAttribute : Attribute
        {
            return FirstOrNull(methodInfo.GetCustomAttributes(typeof(TAttribute), true)) as TAttribute != null;
        }

        public static bool HasAttribute<TAttribute>(MemberInfo memberInfo) where TAttribute : Attribute
        {
            return FirstOrNull(memberInfo.GetCustomAttributes(typeof(TAttribute), true)) as TAttribute != null;
        }

        public static bool HasAttribute<TAttribute>(Type type) where TAttribute : Attribute
        {
            return FirstOrNull(type.GetCustomAttributes(typeof(TAttribute), true)) as TAttribute != null;
        }

        public static bool HasAttribute<TAttribute>(ConstructorInfo constructorInfo) where TAttribute : Attribute
        {
            return FirstOrNull(constructorInfo.GetCustomAttributes(typeof(TAttribute), true)) as TAttribute != null;
        }

        private static object FirstOrNull(IEnumerable<object> enumerable)
        {
            IEnumerator e = enumerable.GetEnumerator();
            return e.MoveNext() ? e.Current : null;
        }

        public static IEnumerable<MethodInfo> AllMethodsOf(Type type)
        {
            return type.GetMethods(BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.Instance | BindingFlags.DeclaredOnly);
        }

        public static MemberInfo[] AllMembersOf(Type type)
        {
            return type.GetMembers(BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.Instance | BindingFlags.DeclaredOnly);
        }

    }
}
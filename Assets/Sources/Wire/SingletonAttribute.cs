using System;

namespace EMP.Wire
{
    [AttributeUsage(AttributeTargets.Method | AttributeTargets.Class)]
    public class SingletonAttribute : Attribute
    {
    }
}
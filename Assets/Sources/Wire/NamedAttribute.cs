using System;

namespace EMP.Wire
{
    [AttributeUsage(AttributeTargets.Method | AttributeTargets.Parameter)]
    public class NamedAttribute : Attribute
    {
        public string name;

        public NamedAttribute(string name)
        {
            this.name = name;
        }
    }
}
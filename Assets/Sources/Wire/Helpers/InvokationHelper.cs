
using System;
using System.Reflection;

namespace EMP.Wire
{
    public class InvokationHelper
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
}
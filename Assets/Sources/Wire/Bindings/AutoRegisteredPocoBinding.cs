using System;
using System.Reflection;

public class AutoRegisteredPocoBinding : AbstractBinding
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
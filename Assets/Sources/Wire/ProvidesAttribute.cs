using System;

[AttributeUsage(AttributeTargets.Method)]
public class ProvidesAttribute : Attribute {
    public string name;
    public Type interfaceType;

    public ProvidesAttribute(Type interfaceType, string name)
    {
        this.interfaceType = interfaceType;
        this.name = name;
    }

    public ProvidesAttribute(Type interfaceType) : this(interfaceType, null) { }
    public ProvidesAttribute(string name) : this(null, name) { }
    public ProvidesAttribute() : this(null, null) { }
}

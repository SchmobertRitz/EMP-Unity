using System;

[AttributeUsage(AttributeTargets.Method | AttributeTargets.Field | AttributeTargets.Property)]
public class InjectAttribute : Attribute {
    public string name;

    public InjectAttribute(string name)
    {
        this.name = name;
    }

    public InjectAttribute() : this(null) { }
}

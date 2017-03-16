using System;

[AttributeUsage(AttributeTargets.Method)]
public class GameObjectAttribute : Attribute {
    public string name;

    public GameObjectAttribute(string name)
    {
        this.name = name;
    }

    public GameObjectAttribute() : this(null) { }
}

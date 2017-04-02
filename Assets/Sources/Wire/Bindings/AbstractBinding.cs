﻿using System;

public abstract class AbstractBinding : IBinding
{
    protected Wire wire;

    public Type BoundType
    { get; private set; }

    public string BoundName
    { get; private set; }

    public AbstractBinding(Wire wire, string name, Type type)
    {
        this.wire = wire;
        BoundName = name;
        BoundType = type;
    }

    public abstract object GetInstance();
}
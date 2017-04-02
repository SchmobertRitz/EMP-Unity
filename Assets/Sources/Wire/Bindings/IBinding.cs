using System;

public interface IBinding
{
    Type BoundType
    { get; }

    string BoundName
    { get; }

    object GetInstance();
}
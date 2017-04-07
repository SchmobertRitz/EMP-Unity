using System;

namespace EMP.Wire
{
    public interface IBinding
    {
        Type BoundType
        { get; }

        string BoundName
        { get; }

        object GetInstance();
    }
}
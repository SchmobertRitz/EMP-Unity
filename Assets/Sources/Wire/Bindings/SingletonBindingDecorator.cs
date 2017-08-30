//
// MIT License
// Copyright (c) EMP - https://github.com/SchmobertRitz/EMP-Unity
//
using System;
namespace EMP.Wire
{
    public class SingletonBindingDecorator : IBinding
    {
        private object instance;
        private IBinding delegateBinding;

        public Type BoundType
        { get { return delegateBinding.BoundType; } }

        public string BoundName
        { get { return delegateBinding.BoundName; } }

        public SingletonBindingDecorator(IBinding delegateBinding)
        {
            this.delegateBinding = delegateBinding;
        }

        public object GetInstance()
        {
            if (instance == null)
            {
                instance = delegateBinding.GetInstance();
            }
            return instance;
        }
    }
}
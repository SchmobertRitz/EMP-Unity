//
// MIT License
// Copyright (c) EMP - https://github.com/SchmobertRitz/EMP-Unity
//
using EMP.EventBus;
using EMP.Wire;

[Singleton]
public class WiredEventBus {

    public void Post(object eventObject, string busName) {
        EventBus.Post(eventObject, busName);
    }

    public void Post(object eventObject)
    {
        EventBus.Post(eventObject);
    }

    public void Register(object receiver, string name)
    {
        EventBus.Register(receiver, name);
    }

    public void Register(object receiver)
    {
        EventBus.Register(receiver);
    }

}

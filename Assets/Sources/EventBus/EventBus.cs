//
// MIT License
// Copyright (c) EMP - https://github.com/SchmobertRitz/EMP-Unity
//
using UnityEngine;
using System.Reflection;
using System;
using System.Collections.Generic;

namespace EMP.EventBus
{
    public class EventBus : MonoBehaviour
    {
        private Dictionary<string, Dictionary<string, List<System.Object>>> allRegisteredTargets = new Dictionary<string, Dictionary<string, List<System.Object>>>();
        private Dictionary<string, List<System.Object>> pendingEvents = new Dictionary<string, List<System.Object>>();

        void Update()
        {
            DispatchPending();
        }

        private static EventBus GetInstance()
        {
            GameObject eventBusObject = GameObject.Find("EventBus");
            if (eventBusObject == null)
            {
                eventBusObject = new GameObject();
                eventBusObject.name = "EventBus";
                return eventBusObject.AddComponent<EventBus>();
            }
            EventBus eventBusInstance = eventBusObject.GetComponent<EventBus>();
            if (eventBusInstance == null)
            {
                return eventBusObject.AddComponent<EventBus>();
            }
            return eventBusInstance;
        }

        public static void Register(System.Object target)
        {
            GetInstance().RegisterInternal(target, "");
        }

        public static void Unregister(System.Object target)
        {
            GetInstance().UnregisterInternal(target, "");
        }

        public static void Post(System.Object eventObject)
        {
            GetInstance().PostInternal(eventObject, "");
        }

        public static void Register(System.Object target, string busname)
        {
            GetInstance().RegisterInternal(target, busname);
        }

        public static void Unregister(System.Object target, string busname)
        {
            GetInstance().UnregisterInternal(target, busname);
        }

        public static void Post(System.Object eventObject, string busname)
        {
            GetInstance().PostInternal(eventObject, busname);
        }

        private void PostInternal(System.Object eventObject, string busname)
        {
            if (pendingEvents.ContainsKey(busname))
            {
                pendingEvents[busname].Add(eventObject);
            }
            else
            {
                List<System.Object> pending = new List<System.Object>();
                pending.Add(eventObject);
                pendingEvents[busname] = pending;
            }
        }

        private void RegisterInternal(System.Object target, string busname)
        {
            Dictionary<string, List<System.Object>> registeredTargets = GetRegisteredTargets(busname);
            IEnumerable<MethodInfo> methods = target.GetType().GetMethods((BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.Instance | BindingFlags.DeclaredOnly));
            foreach (MethodInfo method in methods)
            {
                if (method.Name.Equals("OnEvent"))
                {
                    ParameterInfo[] parameters = method.GetParameters();
                    if (parameters.Length == 1)
                    {
                        ParameterInfo parameter = parameters[0];
                        if (!parameter.IsOptional)
                        {
                            string parameterTypeName = GetTypeName(parameter.ParameterType);
                            List<System.Object> targets;
                            if (registeredTargets.ContainsKey(parameterTypeName))
                            {
                                targets = registeredTargets[parameterTypeName];
                            }
                            else
                            {
                                targets = new List<System.Object>();
                                registeredTargets.Add(parameterTypeName, targets);
                            }
                            targets.Add(target);
                        }
                    }
                }
            }

        }

        private Dictionary<string, List<object>> GetRegisteredTargets(string busname)
        {
            if (!allRegisteredTargets.ContainsKey(busname))
            {
                Dictionary<string, List<object>> registeredTargets = new Dictionary<string, List<object>>();
                allRegisteredTargets[busname] = registeredTargets;
                return registeredTargets;
            }
            else
            {
                return allRegisteredTargets[busname];
            }
        }

        private void UnregisterInternal(System.Object target, string busname)
        {
            Dictionary<string, List<System.Object>> registeredTargets = GetRegisteredTargets(busname);
            foreach (List<System.Object> targets in registeredTargets.Values)
            {
                targets.Remove(target);
            }
        }

        private string GetTypeName(Type type)
        {
            return type.Namespace + "." + type.Name;
        }

        private void DispatchPending()
        {
            if (pendingEvents.Count != 0)
            {
                float maxDispatchingTime = Time.realtimeSinceStartup + 0.125f;
                List<string> keysToRemove = null;
                Dictionary<string, List<System.Object>>.Enumerator enumBuses = pendingEvents.GetEnumerator();
                while (Time.realtimeSinceStartup < maxDispatchingTime && enumBuses.MoveNext())
                {
                    KeyValuePair<string, List<System.Object>> item = enumBuses.Current;
                    string busname = item.Key;
                    List<System.Object> list = item.Value;
                    for (int i = list.Count - 1; i >= 0 && Time.realtimeSinceStartup < maxDispatchingTime; i--)
                    {
                        System.Object eventObject = list[i];
                        DispatchEvent(eventObject, busname);
                        list.RemoveAt(i);
                    }
                    if (list.Count == 0)
                    {
                        if (keysToRemove == null)
                        {
                            keysToRemove = new List<string>();
                        }
                        keysToRemove.Add(busname);
                    }
                }
                enumBuses.Dispose();
                if (keysToRemove != null)
                {
                    foreach (string key in keysToRemove)
                    {
                        pendingEvents.Remove(key);
                    }
                }
            }

        }

        private void DispatchEvent(System.Object eventObject, string busname)
        {
            if (eventObject != null)
            {
                Dictionary<string, List<System.Object>> registeredTargets;
                if (allRegisteredTargets.ContainsKey(busname))
                {
                    registeredTargets = allRegisteredTargets[busname];
                    String objectTypeName = GetTypeName(eventObject.GetType());
                    if (registeredTargets.ContainsKey(objectTypeName))
                    {
                        object[] param = new object[] { eventObject };
                        foreach (System.Object target in registeredTargets[objectTypeName])
                        {
                            //TODO Cache Method Objects
                            List<Type> typeList = new List<Type>();
                            typeList.Add(eventObject.GetType());
                            Type[] types = typeList.ToArray(); //FIXME das muss ja wohl auch besser gehen
                            MethodInfo method = target.GetType().GetMethod("OnEvent", (BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.Instance | BindingFlags.DeclaredOnly), null, types, null);
                            if (method != null)
                            {
                                try
                                {
                                    method.Invoke(target, param);
                                }
                                catch (Exception e)
                                {
                                    Debug.Log("Exception occured while dispatching event " + GetTypeName(eventObject.GetType()) + ": " + e.Message);
                                }
                            }
                        }
                    }
                }
            }
        }
    }
}
using System;
using System.Collections.Generic;
using UnityEngine;

public static class Service
{
    private static Dictionary<Type, object> _services = new Dictionary<Type, object>();

    public static bool Register<T>(T service, EManagerType managerType = EManagerType.none) where T : MonoBehaviour
    {
        if (_services.ContainsKey(typeof(T))) return false;

        _services[typeof(T)] = service;

        return true;
    }
    
    public static void UnRegister<T>(T service)  where T : MonoBehaviour
    {
        if(_services.ContainsKey(typeof(T)) && (T)_services[typeof(T)] == service) _services.Remove(typeof(T));
    }
    
    public static T Get<T>()
    {
        if (_services.TryGetValue(typeof(T), out object service)) return (T)service;
        return default;
    }
}

public enum EManagerType
{
    none,
    dontDestroyOnLoad,
    Session
}
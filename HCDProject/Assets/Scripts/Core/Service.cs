using System;
using System.Collections.Generic;
using UnityEngine;

public static class Service
{
    public static IDictionary<Type, object> Services { get => _services; }
    private static Dictionary<Type, object> _services = new();

    public static void Register<T>(T service)
    {
        if (!_services.ContainsKey(typeof(T)))
        {
            _services[typeof(T)] = service;
        }
    }
    
    public static void UnRegister<T>()
    {
        _services.Remove(typeof(T));
    }

    public static T Get<T>()
    {
        return (T)_services[typeof(T)];
    }
}

public interface ISpawnable
{
    void Spawn(int chapter, int stage, int wave);
}

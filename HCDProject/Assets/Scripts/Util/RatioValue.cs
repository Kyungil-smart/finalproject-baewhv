using System;
using UnityEngine;
using UnityEngine.Events;

[Serializable]
public class RatioIntValue : ObserveValue<int>
{
    [SerializeField] private int _maxValue;

    private UnityEvent<float> OnRatioChange = new ();
    public int MaxValue
    {
        get => _maxValue;
        set
        {
            _maxValue = value;
            OnRatioChange.Invoke(_data/(float)_maxValue);
        }
    }

    public RatioIntValue(int maxValue)
    {
        _maxValue = maxValue;
        AddListener(OnChangeValue);
    }

    public RatioIntValue(int maxValue, int currentValue) : this(maxValue)
    {
        _data = currentValue;
    }

    private void OnChangeValue(int value)
    {
        OnRatioChange.Invoke(_data/(float)_maxValue);
    }

    public void AddRatioListener(UnityAction<float> action)
    {
        OnRatioChange.AddListener(action);
    }
}

[Serializable]
public class RatioFloatValue : ObserveValue<float>
{
    [SerializeField] private float _maxValue;

    private UnityEvent<float> OnRatioChange = new ();
    public float MaxValue
    {
        get => _maxValue;
        set
        {
            _maxValue = value;
            OnRatioChange.Invoke(_data/_maxValue);
        }
    }

    public RatioFloatValue(float maxValue)
    {
        _maxValue = maxValue;
        AddListener(OnChangeValue);
    }

    public RatioFloatValue(int maxValue, int currentValue) : this(maxValue)
    {
        _data = currentValue;
    }

    private void OnChangeValue(float value)
    {
        OnRatioChange.Invoke(_data/_maxValue);
    }
    
    public void AddRatioListener(UnityAction<float> action)
    {
        OnRatioChange.AddListener(action);
    }
}
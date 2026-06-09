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
            OnRatioChange?.Invoke(_data/(float)_maxValue);
        }
    }

    public RatioIntValue(int maxValue) : this(maxValue, maxValue) {}
    public RatioIntValue(int maxValue, int currentValue)
    {
        AddListener(OnChangeValue);
        MaxValue = maxValue;
        Value = currentValue;
    }

    private void OnChangeValue(int value)
    {
        OnRatioChange?.Invoke(value/(float)_maxValue);
    }

    public void AddRatioListener(UnityAction<float> action)
    {
        OnRatioChange.AddListener(action);
        OnRatioChange?.Invoke(_data/(float)_maxValue);
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
            OnRatioChange?.Invoke(_data/_maxValue);
        }
    }

    
    public RatioFloatValue(float maxValue) : this(maxValue, maxValue) {}
    public RatioFloatValue(float maxValue, float currentValue)
    {
        AddListener(OnChangeValue);
        MaxValue = maxValue;
        Value = currentValue;
    }

    private void OnChangeValue(float value)
    {
        OnRatioChange.Invoke(value/_maxValue);
    }
    
    public void AddRatioListener(UnityAction<float> action)
    {
        OnRatioChange.AddListener(action);
        OnRatioChange?.Invoke(_data/_maxValue);
    }
}
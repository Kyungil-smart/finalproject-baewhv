using System;
using UnityEngine;

public class VibrationManager : BaseManager<VibrationManager>
{
    private bool _isVibOn = true;
    public bool IsVibOn => _isVibOn;

    public void Start()
    {
        LoadVibSetting();
    }

    public void SetVibration(bool isOn)
    {
        _isVibOn = isOn;
        PlayerPrefs.SetInt("Vibration", isOn ? 1 : 0);
        PlayerPrefs.Save();

        if (isOn)
        {
            Vibrate();
        }
    }

    public void TriggerVibration()
    {
        if (_isVibOn) Vibrate();
    }

    private void Vibrate()
    {
        Handheld.Vibrate();
        Debug.Log("진동");
    }

    public void LoadVibSetting()
    {
        _isVibOn = PlayerPrefs.GetInt("Vibration", 1) == 1;
    }
}

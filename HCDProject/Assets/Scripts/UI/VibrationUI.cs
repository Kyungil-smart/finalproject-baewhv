using System;
using UnityEngine;
using UnityEngine.UI;

public class VibrationUI : MonoBehaviour
{
    public Button on;
    public Button off;
    
    public Color active = Color.green;
    public Color deactive = Color.white;

    private void Start()
    {
        if (on != null) on.onClick.AddListener((() => OnClickVibrationButton(true)));
        if (off != null) off.onClick.AddListener((() => OnClickVibrationButton(false)));

        UpdateColor(Service.Get<VibrationManager>().IsVibOn);
    }

    private void OnClickVibrationButton(bool isOn)
    {
        Service.Get<VibrationManager>()?.SetVibration(isOn);

        UpdateColor(isOn);
    }

    private void UpdateColor(bool isOn)
    {
        if (on != null && off != null)
        {
            on.GetComponent<Image>().color = isOn ? active : deactive;
            off.GetComponent<Image>().color = isOn ? deactive : active;
        }
    }
}

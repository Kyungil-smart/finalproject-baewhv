using System;
using System.Collections.Generic;
using DG.Tweening;
using UnityEngine;
using UnityEngine.UI;


//UI에 접근할 수 있게끔 서비스 로케이터 형으로 제작.
public class UIManager : BaseManager<UIManager>
{
    private Dictionary<Type, object> _uiControllers = new();
    [SerializeField] private Image fader;
    [SerializeField] private SettingPopupUI OptionPopup;
    public Image GetFader => fader;

    protected override void Awake()
    {
        base.Awake();
        fader.gameObject.SetActive(true);
        fader.DOFade(0.0f, 1.0f);
    }

    public bool Register<T>(T service) where T : MonoBehaviour
    {
        if (_uiControllers.ContainsKey(typeof(T))) return false;
        _uiControllers[typeof(T)] = service;
        Debug.Log($"AddUI{service.name}");

        return true;
    }

    public void UnRegister<T>(T service) where T : MonoBehaviour
    {
        Debug.Log($"RemoveUI{service.name}");
        if (_uiControllers.ContainsKey(typeof(T)) && (T)_uiControllers[typeof(T)] == service)
            _uiControllers.Remove(typeof(T));
    }

    public T GetUI<T>() where T : MonoBehaviour
    {
        if (_uiControllers.TryGetValue(typeof(T), out object service)) return (T)service;
        return null;
    }

    public void RemoveFader()
    {
        if(fader)
            Destroy(fader.gameObject);
    }

    public void OpenOption(ESettingPopupType type = ESettingPopupType.none)
    {
        OptionPopup.OpenPopup(type);
    }
}
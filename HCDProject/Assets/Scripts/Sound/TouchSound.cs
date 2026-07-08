using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.InputSystem;
using UnityEngine.UI;

public class TouchSound : MonoBehaviour
{
    [SerializeField] private PlayerInput _playerInput;
    [SerializeField] private AudioSource _audioSource;
    
    private InputAction _touchAction;

    private void Awake()
    {
        if (_playerInput != null)
        {
            _touchAction = _playerInput.actions["Touch"];
        }
    }

    private void OnEnable()
    {
        if (_touchAction != null) _touchAction.started += OnTouchSound;
    }

    private void OnDisable()
    {
        if (_touchAction != null) _touchAction.started -= OnTouchSound;
    }

    private void OnTouchSound(InputAction.CallbackContext ctx)
    {
        if (Touchscreen.current == null) return;

        StartCoroutine(TouchSoundRoutine());
    }

    private IEnumerator TouchSoundRoutine()
    {
        yield return null;

        if (EventSystem.current != null && Touchscreen.current != null)
        {
            int touchId = Touchscreen.current.primaryTouch.touchId.ReadValue();
            Vector2 touchPosition = Touchscreen.current.primaryTouch.position.ReadValue();

            if (EventSystem.current.IsPointerOverGameObject(touchId))
            {
                PointerEventData eventData = new PointerEventData(EventSystem.current);
                eventData.position = touchPosition;
                
                List<RaycastResult> results = new List<RaycastResult>();
                EventSystem.current.RaycastAll(eventData, results);

                if (results.Count > 0)
                {
                    GameObject clickUi = results[0].gameObject;
                
                    Debug.Log(clickUi.name);
                
                    OtherSoundButton otherSoundButton = results[0].gameObject.GetComponentInParent<OtherSoundButton>();

                    if (otherSoundButton != null)
                    {
                        otherSoundButton.PlayOtherSound();
                        yield break;
                    }
                }
            }
            if (_audioSource != null ) Service.Get<SoundManager>()?.PlaySfxSound("Touch");
        }
    }
}

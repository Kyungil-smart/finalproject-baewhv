using System;
using UnityEngine;
using UnityEngine.InputSystem;

public class SceneManager : MonoBehaviour
{
    private void Awake()
    {
        Service.Register<SceneManager>(this);
    }
    
    private void Update()
    {
        if (Keyboard.current.iKey.wasPressedThisFrame) ChangeScene("TestSceneKDH1");
    }

    private void OnDestroy()
    {
        Service.UnRegister<SceneManager>();
    }

    

    public void ChangeScene(string sceneName)
    {
        UnityEngine.SceneManagement.SceneManager.LoadScene(sceneName);
    }
}

using System;
using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.SceneManagement;

public class SceneController : BaseManager<SceneController>
{
    private Scene _sessionScene;
    private SceneType _currentScene = SceneType.Title;

    private void Update()
    {
        if (Keyboard.current.iKey.wasPressedThisFrame) ChangeScene(SceneType.Title);
    }

    public void ChangeScene(SceneType scene)
    {
        if (scene == SceneType.Title ||  scene == SceneType.ModeSelect || scene == SceneType.StageSelect)
        {
            SceneManager.LoadScene((int)scene, LoadSceneMode.Single);
            _currentScene = scene;
        }
        else if (scene == SceneType.InGame)
        {
            if (_currentScene == SceneType.StageSelect) SceneManager.UnloadSceneAsync((int)SceneType.StageSelect);
            
            SceneManager.LoadScene((int)scene, LoadSceneMode.Additive);
            _currentScene = scene;
        }
    }

    public void MoveGameObjectToSessionScene(GameObject gObj)
    {
        if (!_sessionScene.isLoaded) return;
        SceneManager.MoveGameObjectToScene(gObj, _sessionScene);
    }
    
    public void CreateSession()
    {
        if (_sessionScene.isLoaded) return; 
        
        _sessionScene = SceneManager.CreateScene("Session",new CreateSceneParameters());
    }
}

public enum SceneType
{
    Title = 0,
    ModeSelect = 1,
    StageSelect = 2,
    InGame = 3
}

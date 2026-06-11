using System;
using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.SceneManagement;

public class SceneController : BaseManager<SceneController>
{
    private Scene _sessionScene;
    private SceneType _currentScene = SceneType.Title;

    private void Awake()
    {
        base.Awake();

        SceneManager.sceneLoaded += OnSceneLoaded;
    }
    
    private void Update()
    {
        if (Keyboard.current.iKey.wasPressedThisFrame) ChangeScene(SceneType.Title);
    }

    private void OnDestroy()
    {
        SceneManager.sceneLoaded -= OnSceneLoaded;
        base.OnDestroy();
    }

    private void OnSceneLoaded(Scene scene, LoadSceneMode mode)
    {
        if (scene.name != "Session") SceneManager.SetActiveScene(scene);
    }

    public void ChangeScene(SceneType scene)
    {
        if (scene == SceneType.StageSelect) CreateSession();
        
        if (scene == SceneType.Title ||  scene == SceneType.ModeSelect)
        {
            SceneManager.LoadScene((int)scene, LoadSceneMode.Single);
            _currentScene = scene;
        }
        else if (scene == SceneType.StageSelect)
        {
            if (_currentScene == SceneType.InGame) SceneManager.UnloadSceneAsync((int)SceneType.InGame);
            if (_currentScene == SceneType.ModeSelect) SceneManager.UnloadSceneAsync((int)SceneType.ModeSelect);
            
            SceneManager.LoadScene((int)scene, LoadSceneMode.Additive);
            _currentScene = scene;
        }
        else if (scene == SceneType.InGame)
        {
            if (_currentScene == SceneType.StageSelect) SceneManager.UnloadSceneAsync((int)SceneType.StageSelect);
            if (_currentScene == SceneType.InGame) SceneManager.UnloadSceneAsync((int)SceneType.InGame);
            
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

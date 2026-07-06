using System;
using System.Collections;
using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.SceneManagement;

public class SceneController : BaseManager<SceneController>
{
    private Scene _sessionScene;
    private SceneType _currentScene = SceneType.Title;
    
    protected override void Awake()
    {
        base.Awake();
        
        if (IsManagerDestroy) return;

        SceneManager.sceneLoaded += OnSceneLoaded;
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
        if (scene == SceneType.StageSelect)
        {
            CreateSession();
            Service.Get<TimeManager>()?.ResetTimeScale();
        }
        
        LoadSceneMode sceneMode = (scene == SceneType.Title || scene == SceneType.ModeSelect) ? LoadSceneMode.Single : LoadSceneMode.Additive;

        SceneType backupScene = _currentScene;
        _currentScene = scene;

        var loadManager = Service.Get<LoadManager>();
        if (loadManager != null)
        {
            loadManager.StartLoading(scene, sceneMode, backupScene);
        }
    }

    public void PlaySceneBgm(SceneType scene)
    {
        var soundManager = Service.Get<SoundManager>();
        
        if (soundManager != null)
        {
            switch (scene)
            {
                case SceneType.Title:
                    soundManager.PlayBgmSound("Title");
                    break;
                case SceneType.ModeSelect:
                    soundManager.PlayBgmSound("Title");
                    break;
                case SceneType.StageSelect:
                    soundManager.PlayBgmSound("StageSelect");
                    break;
                case SceneType.InGame:
                    soundManager.PlayBgmSound("Battle_1");
                    break;
                case SceneType.Tutorial:
                    soundManager.PlayBgmSound("Battle_1");
                    break;
            }
        }
    }

    public IEnumerator UnLoadActiveSceneRoutine(SceneType sceneType)
    {
        if (sceneType == SceneType.ModeSelect || sceneType == SceneType.StageSelect || sceneType == SceneType.InGame || sceneType == SceneType.Tutorial)
        {
            Scene targerScene = SceneManager.GetSceneByBuildIndex((int)sceneType);
            if (targerScene.isLoaded)
            {
                AsyncOperation unload = SceneManager.UnloadSceneAsync((int)sceneType);
                if (unload != null)
                {
                    while (!unload.isDone) yield return null;
                }
            }
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
    InGame = 3,
    Tutorial = 4
}

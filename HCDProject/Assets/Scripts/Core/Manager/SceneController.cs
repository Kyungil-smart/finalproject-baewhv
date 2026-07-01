using System;
using System.Collections;
using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.SceneManagement;

public class SceneController : BaseManager<SceneController>
{
    private Scene _sessionScene;
    private SceneType _currentScene = SceneType.Title;

    public event Action<float> OnLoading;
    public event Action OnLoadingComplete;
    
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

        StartCoroutine(LoadSceneRoutine(scene, sceneMode));
    }

    private IEnumerator LoadSceneRoutine(SceneType scene, LoadSceneMode mode)
    {
        OnLoading?.Invoke(0f);
        
        SceneType backupScene = _currentScene;
        _currentScene = scene;

        if (mode == LoadSceneMode.Additive && backupScene == scene)
        {
            yield return StartCoroutine(UnLoadActiveSceneRoutine(backupScene));

            yield return new WaitForSecondsRealtime(0.5f);
        }
        
        AsyncOperation async = SceneManager.LoadSceneAsync((int)scene, mode);

        if (async != null) async.allowSceneActivation = false;
        
        float loadTime = 0f;
        float needTime = 5f;
        
        while (async.progress < 0.9f)/*|| loadTime < needTime*/
        {
            yield return null;

            loadTime += Time.unscaledDeltaTime;
            
            float time = loadTime / needTime;
            float progress = Mathf.Clamp01(time);
            
            OnLoading?.Invoke(progress);
        }
        OnLoading?.Invoke(1f);

        yield return new WaitForSecondsRealtime(0.5f);
        
        if (async != null)  async.allowSceneActivation = true;

        PlaySceneBgm(scene);

        while (!async.isDone) yield return null;

        if (mode == LoadSceneMode.Additive && backupScene != scene) yield return StartCoroutine(UnLoadActiveSceneRoutine(backupScene));
        
        OnLoadingComplete?.Invoke();
    }

    private void PlaySceneBgm(SceneType scene)
    {
        var soundManager = Service.Get<SoundManager>();
        
        if (soundManager != null)
        {
            switch (scene)
            {
                case SceneType.Title:
                    soundManager.PlayBgmSound("HCD_Title");
                    break;
                case SceneType.ModeSelect:
                    soundManager.PlayBgmSound("HCD_Title");
                    break;
                case SceneType.StageSelect:
                    soundManager.PlayBgmSound("HCD_Stage");
                    break;
                case SceneType.InGame:
                    soundManager.PlayBgmSound("HCD_Battle_1");
                    break;
                case SceneType.Tutorial:
                    soundManager.PlayBgmSound("HCD_Battle_1");
                    break;
            }
        }
    }

    private IEnumerator UnLoadActiveSceneRoutine(SceneType sceneType)
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

using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using NUnit.Framework;
using UnityEngine;
using UnityEngine.AddressableAssets;
using UnityEngine.ResourceManagement.AsyncOperations;
using UnityEngine.SceneManagement;

public class LoadManager : BaseManager<LoadManager>
{
    public event Action<float> OnLoading;
    public event Action OnLoadingComplete;
    
    [SerializeField] private List<GameObject> _managerPrefabs = new List<GameObject>();
    
    private List<AsyncOperationHandle> _loadedHandles = new List<AsyncOperationHandle>();
    private List<GameObject> _loadManagers = new List<GameObject>();

    protected override void Awake()
    {
        base.Awake();
        
        if (IsManagerDestroy) return;
    }

    public void StartLoading(SceneType targetScene, LoadSceneMode mode, SceneType backupScene)
    {
        StopAllCoroutines();
        StartCoroutine(StartLoadingRoutine(targetScene, mode, backupScene));
    }

    private IEnumerator StartLoadingRoutine(SceneType targetScene, LoadSceneMode mode, SceneType backupScene)
    {
        OnLoading?.Invoke(0f);
        float currentProgress = 0f;

        yield return StartCoroutine(UnloadSceneRoutine(backupScene));
        
        currentProgress = 0.1f;
        OnLoading?.Invoke(currentProgress);
        
        AsyncOperation loadScene = SceneManager.LoadSceneAsync((int)targetScene, mode);
        if (loadScene != null) loadScene.allowSceneActivation = false;

        while (loadScene != null && loadScene.progress < 0.9f)
        {
            currentProgress = 0.1f + (loadScene.progress / 0.9f) * 0.3f;
            OnLoading?.Invoke(currentProgress);
            yield return null;
        }
        
        yield return StartCoroutine(SpawnManagerRoutine(targetScene));
        currentProgress = 0.5f;
        OnLoading?.Invoke(currentProgress);
        
        yield return null;
        
        yield return StartCoroutine(LoadAddressableRoutine(targetScene));
        currentProgress = 0.8f;
        OnLoading?.Invoke(currentProgress);
        
        yield return null;

        yield return new WaitForSecondsRealtime(0.5f);
        
        currentProgress = 0.9f;
        OnLoading?.Invoke(currentProgress);
        
        yield return new WaitForSecondsRealtime(0.5f);
        
        OnLoading?.Invoke(1f);

        loadScene.allowSceneActivation = true;
        while (!loadScene.isDone) yield return null;
        
        Scene scene = SceneManager.GetSceneByBuildIndex((int)targetScene);
        if (scene.IsValid())
        {
            SceneManager.SetActiveScene(scene);
            MoveManagers(scene);
        }
        
        var uiManager = Service.Get<UIManager>();
        if (uiManager != null) yield return StartCoroutine(uiManager.WaitUiRoutine(targetScene));

        if (targetScene == SceneType.InGame || targetScene == SceneType.Tutorial)
        {
            var playerManager = Service.Get<PlayerManager>();
            if (playerManager != null) playerManager.SpawnAllCharacters();
        }
        
        Service.Get<SceneController>()?.PlaySceneBgm(targetScene);
        
        OnLoadingComplete?.Invoke();
    }

    private IEnumerator UnloadSceneRoutine(SceneType backupScene)
    {
        foreach (var handle in _loadedHandles)
        {
            if (handle.IsValid()) Addressables.Release(handle);
        }
        _loadedHandles.Clear();
        
        yield return StartCoroutine(Service.Get<SceneController>()?.UnLoadActiveSceneRoutine(backupScene));
    }

    public IEnumerator SpawnManagerRoutine(SceneType targetScene)
    {
        if (targetScene == SceneType.InGame || targetScene == SceneType.Tutorial)
        {
            foreach (var prefab in _managerPrefabs)
            {
                if (prefab == null) continue;

                GameObject manager = Instantiate(prefab);
                
                DontDestroyOnLoad(manager);
                _loadManagers.Add(manager);
            }
        }
        yield return null;
    }

    private void MoveManagers(Scene targetScene)
    {
        if (_loadManagers.Count == 0) return;
        
        if (targetScene.IsValid())
        {
            foreach (var manager in _loadManagers)
            {
                if (manager != null) SceneManager.MoveGameObjectToScene(manager, targetScene);
            }
        }
        _loadManagers.Clear();
    }

    private IEnumerator LoadAddressableRoutine(SceneType targetScene)
    {
        if (targetScene == SceneType.InGame || targetScene == SceneType.Tutorial)
        {
            var gameManager = Service.Get<GameManager>();
            if (gameManager == null) yield break;
            
            if (gameManager.ids == null || gameManager.ids.Count == 0)
            {
                HashSet<string> stageMonsters = new HashSet<string>();
                var mapTable = Service.Get<DataManager>()?.MapTable.data;
                if (mapTable != null)
                {
                    var waves = mapTable.FindAll(x => x.CHAPTER == gameManager.CurrentChapter && x.STAGE == gameManager.CurrentStage);
                    foreach (var wave in waves)
                    {
                        if (!string.IsNullOrEmpty(wave.SPAWN_MONSTER_ID_01)) stageMonsters.Add(wave.SPAWN_MONSTER_ID_01.Trim());
                        if (!string.IsNullOrEmpty(wave.SPAWN_MONSTER_ID_02)) stageMonsters.Add(wave.SPAWN_MONSTER_ID_02.Trim());
                        if (!string.IsNullOrEmpty(wave.SPAWN_MONSTER_ID_03)) stageMonsters.Add(wave.SPAWN_MONSTER_ID_03.Trim());
                        if (!string.IsNullOrEmpty(wave.SPAWN_MONSTER_ID_04)) stageMonsters.Add(wave.SPAWN_MONSTER_ID_04.Trim());
                        if (!string.IsNullOrEmpty(wave.SPAWN_MONSTER_ID_05)) stageMonsters.Add(wave.SPAWN_MONSTER_ID_05.Trim());
                        if (!string.IsNullOrEmpty(wave.SPAWN_MONSTER_ID_06)) stageMonsters.Add(wave.SPAWN_MONSTER_ID_06.Trim());
                        if (!string.IsNullOrEmpty(wave.SPAWN_MONSTER_ID_07)) stageMonsters.Add(wave.SPAWN_MONSTER_ID_07.Trim());
                    }
                }
                gameManager.ids = stageMonsters;
            }
            
            List<AsyncOperationHandle> loadHandles = new List<AsyncOperationHandle>();

            if (gameManager.ids.Count > 0)
            {
                var ids = gameManager.ids.Where(x => !string.IsNullOrWhiteSpace(x) && x.Trim().ToUpper() != "NONE");
                
                foreach (var id in ids)
                {
                    var handle = Addressables.LoadAssetAsync<GameObject>(id);
                    loadHandles.Add(handle);
                    _loadedHandles.Add(handle);
                }

                var wall = gameManager.loadWall();
                loadHandles.Add(wall);
                _loadedHandles.Add(wall);

                foreach (var handle in loadHandles)
                {
                    while (!handle.IsDone) yield return null;
                }
            }
            
            var spawnManager = Service.Get<MonsterSpawnManager>();
            if (gameManager != null && spawnManager != null)
            {
                if (gameManager.ids.Count > 0)
                {
                    yield return StartCoroutine(spawnManager.LoadStageMonstersRoutine(gameManager.ids.ToList()));
                }
            }
            
            var playerManager = Service.Get<PlayerManager>();
            if (playerManager != null)
            {
                yield return StartCoroutine(playerManager.LoadCharcterPrefabRoutine());
            }
        }
        
        yield return null;
    }
}

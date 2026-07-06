using UnityEngine;

public class ReadyState : IState
{
    private GameManager _manager;
    
    public ReadyState(GameManager manager) => _manager = manager;
    
    public void Enter()
    {
        var playerManager = Service.Get<PlayerManager>();
        if (playerManager != null && playerManager.isAllSpawn != null)
        {
            playerManager.isAllSpawn.Value = false;
        }

        var soundManager = Service.Get<SoundManager>();
        if (soundManager != null)
        {
            var bgm = Service.Get<DataManager>()?.MapTable.data.Find(x => x.CHAPTER == _manager.CurrentChapter)?.BGM;
            
            soundManager.PlayBgmSound(bgm);
        }
    }

    public void Update()
    {
        var playerManager = Service.Get<PlayerManager>();
        
        if (playerManager == null || !playerManager.IsPrefabLoaded) return;
        
        if (playerManager != null)
        {
            if (Service.Get<PlayerManager>().isAllSpawn.Value) _manager.CurrentState.Value = GameState.Sort;
        }
    }

    public void Exit()
    {
    }
}

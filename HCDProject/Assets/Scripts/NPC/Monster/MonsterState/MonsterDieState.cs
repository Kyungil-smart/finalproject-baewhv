using UnityEngine;

public class MonsterDieState : IState
{
    private BaseMonster _controller;

    public MonsterDieState(BaseMonster controller)
    {
        _controller = controller;
    }
    
    public void Enter()
    {
        Service.Get<EffectManager>()?.SpawnEffect(_controller.Stat.MONSTER_DEATH_EFEECT, _controller.transform.position, Quaternion.identity);
        Service.Get<PlayerManager>()?.GetExp((int)_controller.Stat.EXP);
        // Service.Get<SoundManager>()?.PlaySfxSound(_controller.Stat.MONSTER_HIT_SFX);
        Service.Get<SoundManager>()?.PlaySfxSound("MonsterKill");
        Service.Get<MonsterSpawnManager>().DespawnMonster(_controller.PrefabIndex, _controller.gameObject);
    }

    public void Update()
    {
        
    }

    public void Exit()
    {
        
    }

}

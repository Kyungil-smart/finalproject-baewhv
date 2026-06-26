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
        
    }

    public void Update()
    {
        Service.Get<EffectManager>()?.SpawnEffect(_controller.Stat.MONSTER_HIT_FX, _controller.transform.position, Quaternion.identity);
        Service.Get<PlayerManager>()?.GetExp((int)_controller.Stat.EXP);
        Service.Get<MonsterSpawnManager>().DespawnMonster(_controller.PrefabIndex, _controller.gameObject);
    }

    public void Exit()
    {
        
    }

}

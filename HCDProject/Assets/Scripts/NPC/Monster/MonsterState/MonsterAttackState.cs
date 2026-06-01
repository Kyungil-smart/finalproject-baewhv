using UnityEngine;

public class MonsterAttackState : IState
{
    private BaseMonster _controller;
    private float _timer;

    public MonsterAttackState(BaseMonster controller)
    {
        _controller = controller;
    }
    
    public void Enter()
    {
        _timer = 0f;
    }

    public void Update()
    {
        if (Vector2.Distance(_controller.transform.position, _controller.Target) >= _controller.Stats._attackRange)
        {
            _controller.CurrentState.Value = EStateType.Chase;
            return;
        }
        
        _timer += Time.deltaTime;
        if (_timer >= 2f)
        {
            _timer = 0f;
            _controller.UseSkill((int)ESkillType.Normal);
        }
    }
    
    public void Exit()
    {

    }
}

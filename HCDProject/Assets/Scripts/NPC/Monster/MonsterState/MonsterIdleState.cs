using UnityEngine;

public class MonsterIdleState : IState
{
    private BaseMonster _controller;

    public MonsterIdleState(BaseMonster controller)
    {
        _controller = controller;
    }
    
    public void Enter()
    {

    }
    
    public void Update()
    {
        if (_controller.BaseSkill.skills.Count > 1 && _controller.BaseSkill.skills[1].SKILL_TIME < 0f)
        {
            _controller.BaseSkill.UseSkill(1);
        }
        
        _controller.CurrentState.Value = EStateType.Chase;
    }
    
    public void Exit()
    {
        
    }
}

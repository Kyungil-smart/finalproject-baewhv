using UnityEngine;

public class MonsterAttackState : IState
{
    private BaseMonster _controller;
    private float _timer;
    private float _bossSkillTimer;

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
        if (Vector3.Distance(_controller.transform.position, _controller.Target) 
              > _controller.BaseSkill.skills[0].SKILL_IS + _controller.GetRadius() + _controller.GetCurrentTarget.GetRadius())
        {
            _controller.CurrentState.Value = EStateType.Chase;
            return;
        }
        
        _timer += Time.deltaTime;
        _bossSkillTimer += Time.deltaTime;

        if (_controller.BaseSkill.skills[0].ATK_TYPE == EAtkType.NORMAL)
        {
            if (_timer >= _controller.Stat.ATK_SPEED)
            {
                _timer = 0f;
                _controller.BaseSkill.UseSkill(0);
            }
        }
        else
        {
            if (_timer >= _controller.BaseSkill.skills[0].SKILL_TIME)
            {
                _timer = 0f;
                _controller.BaseSkill.UseSkill(0);
            }
        }

        if (_controller.BaseSkill.skills.Count > 1)
        {
            if (_bossSkillTimer >= _controller.BaseSkill.skills[1].SKILL_TIME)
            {
                _bossSkillTimer = 0f;
                _controller.BaseSkill.UseSkill(1);
            }
        }
    }
    
    public void Exit()
    {

    }
}
